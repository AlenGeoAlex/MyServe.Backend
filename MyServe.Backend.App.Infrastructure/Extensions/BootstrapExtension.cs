using System.Data;
using System.Data.Common;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Util;
using Dapper;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.App.Domain.Models.Profile;
using MyServe.Backend.App.Domain.Repositories;
using MyServe.Backend.App.Infrastructure.Abstract.UnitOfWork;
using MyServe.Backend.App.Infrastructure.Client;
using MyServe.Backend.App.Infrastructure.Client.Cache;
using MyServe.Backend.App.Infrastructure.Client.Storage;
using MyServe.Backend.App.Infrastructure.Repositories;
using MyServe.Backend.App.Infrastructure.Services;
using MyServe.Backend.App.Infrastructure.TypeHandler;
using MyServe.Backend.Common.Extensions;
using MyServe.Backend.Common.Models;
using MyServe.Backend.Common.Options;
using Npgsql;
using Serilog;
using StackExchange.Redis;
using Supabase;

namespace MyServe.Backend.App.Infrastructure.Extensions;

public static class BootstrapExtension
{
    public static async Task ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration,
        ISecretClient secretClient)
    {
        // await services.ConfigureSupabase(secretClient);
        await services.ConfigureCache(configuration, secretClient);
        await services.ConfigureDatabase(secretClient);
        await services.ConfigureMessaging(configuration, secretClient);
        await services.ConfigureStorage(configuration, secretClient);
        
        services.RegisterInfrastructureServices();
        services.RegisterRepositories();
        services.RegisterTypeHandlers();
        services.ConfigureInfrastructureLayer();
    }

    private static void ConfigureInfrastructureLayer(this IServiceCollection services)
    {
        services.AddSingleton<IRandomStringGeneratorClient, RandomStringGeneratorClient>();
        services.AddSingleton<IAccessTokenClient, JwtAccessTokenClient>();
    }
    
    private static async Task ConfigureSupabase(this IServiceCollection serviceCollection, ISecretClient secretClient)
    {
        if (serviceCollection.IsRegistered(typeof(Supabase.Client)))
        {
            Log.Logger.Information("Supabase is already configured, Skipping.");
            return;
        }
        
        var secretTasks = new List<Task<string>>();
        secretTasks.AddRange([secretClient.GetSecretAsync(VaultConstants.Supabase.ProjectUrl),secretClient.GetSecretAsync(VaultConstants.Supabase.ProjectKey)]);
        var secretValues = await Task.WhenAll(secretTasks);
        var projectUrl = secretValues[0];
        var projectKey = secretValues[1];

        Log.Logger.Information("Supabase Project URL has been configured to: {ProjectUrl}", projectUrl);
        serviceCollection.AddScoped<Supabase.Client>(provider => new Supabase.Client(projectUrl, projectKey, new SupabaseOptions()
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        }));
    }

    private static async Task ConfigureS3(this IServiceCollection serviceCollection, ISecretClient secretClient)
    {
        return;
    }
    
    private static async Task ConfigureCache(this IServiceCollection services, IConfiguration configuration,
        ISecretClient secretClient)
    {
        var configurationSection = configuration.GetSection("Cache");
        if (!bool.TryParse(configurationSection["Enabled"] ??= "false", out var enableRedisCaching) || !enableRedisCaching)
        {
            Log.Logger.Warning("Cache is not enabled");
            services.AddSingleton<ICacheService, NoCacheServices>();
            return;
        }

        var redisConnection = await TryAndConnectRedis(secretClient);
        if (redisConnection == null)
        {
            Log.Logger.Warning("Redis connection is not configured, Falling back to no cache connection");
            services.AddSingleton<ICacheService, NoCacheServices>();
            return;
        }

        services.AddSingleton<IConnectionMultiplexer>(redisConnection);
        services.AddScoped<ICacheService, RedisCacheService>();
        Log.Logger.Information("Connected to Redis...");
    }

    private static async Task<ConnectionMultiplexer?> TryAndConnectRedis(ISecretClient secretClient)
    {
        try
        {
            var connectionString = await secretClient.GetSecretAsync(VaultConstants.Redis.RedisConnectionString);
            if (string.IsNullOrWhiteSpace(connectionString))
                return null;

            var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configuration: connectionString,
                options =>
                {
                    options.ConnectRetry = 5;
                    options.ReconnectRetryPolicy = new LinearRetry(TimeSpan.FromMinutes(10).Milliseconds);
                });

            return connectionMultiplexer;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.Message);
            if (e.StackTrace != null) Log.Logger.Error(e.StackTrace);
            return null;
        }
    }

    private static void RegisterInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IUserOtpService, UserOtpService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    }

    private static void RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserOtpRepository, UserOtpRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    }

    private static void RegisterTypeHandlers(this IServiceCollection services)
    {
        SqlMapper.AddTypeHandler(typeof(ProfileSettings), new ProfileSettingsTypeHandler());
    }

    private static async Task ConfigureDatabase(this IServiceCollection collection, ISecretClient secretClient)
    {
        var databaseString = await secretClient.GetSecretAsync(VaultConstants.Database.ReadWriteDatabaseName);
        collection.AddKeyedScoped<NpgsqlConnection>("read-only-connection", (s, o) => new NpgsqlConnection(databaseString));
        collection.AddKeyedScoped<NpgsqlConnection>("read-write-connection", (s, o) => new NpgsqlConnection(databaseString));
        
        
        collection.AddScoped<IReadOnlyUnitOfWork, ReadOnlyUnitOfWork>();
        collection.AddScoped<IReadWriteUnitOfWork, ReadWriteUnitOfWork>();
        collection.ConfigurePgSql();
        
    }

    private static async Task ConfigureMessaging(this IServiceCollection collection, IConfiguration configuration, ISecretClient secretClient)
    {
        var messagingTypeConfiguration = configuration["Messaging:Type"];
        if(string.IsNullOrWhiteSpace(messagingTypeConfiguration))
            throw new ApplicationException("Missing messaging type in configuration!");
        
        if (!Enum.TryParse(messagingTypeConfiguration, out MessagingType messagingType))
            throw new ApplicationException("Invalid messaging type in configuration!");

        if (messagingType == MessagingType.RabbitMQ)
            await collection.ConfigureRabbitMq(configuration, secretClient);
        else
            await collection.ConfigureAmazonSqs(secretClient);
    }

    private static async Task ConfigureStorage(this IServiceCollection collection, IConfiguration configuration, ISecretClient secretClient)
    {
        var storageConfiguration = new StorageConfiguration();
        configuration.GetSection("Storage").Bind(storageConfiguration);
        
        HashSet<string> keys = new();
        if (storageConfiguration.Profile.Type == StorageType.S3)
        {
            var profileS3ConnectionString = VaultConstants.Storage.S3.EndpointWithPrefix(storageConfiguration.Profile.VaultPrefix);
            var profileS3Region = VaultConstants.Storage.S3.BucketWithPrefix(storageConfiguration.Profile.VaultPrefix);
            var profileS3AccessKey = VaultConstants.Storage.S3.ConnectionAccessKeyWithPrefix(storageConfiguration.Profile.VaultPrefix);
            var profileS3SecretKey = VaultConstants.Storage.S3.ConnectionSecretKeyWithPrefix(storageConfiguration.Profile.VaultPrefix);
            keys.Add(profileS3ConnectionString);
            keys.Add(profileS3Region);
            keys.Add(profileS3AccessKey);
            keys.Add(profileS3SecretKey);
        }
        else
        {
            
        }


        if (storageConfiguration.Files.Type == StorageType.S3)
        {
            var fileS3ConnectionString = VaultConstants.Storage.S3.EndpointWithPrefix(storageConfiguration.Files.VaultPrefix);
            var fileS3Region = VaultConstants.Storage.S3.BucketWithPrefix(storageConfiguration.Files.VaultPrefix);
            var fileS3AccessKey = VaultConstants.Storage.S3.ConnectionAccessKeyWithPrefix(storageConfiguration.Files.VaultPrefix);
            var fileS3SecretKey = VaultConstants.Storage.S3.ConnectionSecretKeyWithPrefix(storageConfiguration.Files.VaultPrefix);
            keys.Add(fileS3ConnectionString);
            keys.Add(fileS3Region);
            keys.Add(fileS3AccessKey);
            keys.Add(fileS3SecretKey);
        }
        else
        {
            
        }

        Dictionary<string, Task<string>> secretTasks = new();
        foreach (var eachStorageKey in keys)
        {
            secretTasks[eachStorageKey] = secretClient.GetSecretAsync(eachStorageKey);
        }

        await Task.WhenAll(secretTasks.Values);


        if (storageConfiguration.Profile.Type == StorageType.S3)
        {
            var serviceUrl = secretTasks[VaultConstants.Storage.S3.EndpointWithPrefix(storageConfiguration.Profile.VaultPrefix)].Result;
            var accessKey = secretTasks[VaultConstants.Storage.S3.ConnectionAccessKeyWithPrefix(storageConfiguration.Profile.VaultPrefix)].Result;
            var secretKey = secretTasks[VaultConstants.Storage.S3.ConnectionSecretKeyWithPrefix(storageConfiguration.Profile.VaultPrefix)].Result;
            var bucket = secretTasks[VaultConstants.Storage.S3.BucketWithPrefix(storageConfiguration.Profile.VaultPrefix)].Result;
            
            var profiles3Client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.APSouth2);

            var bucketExists = false;
            try
            {
                bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(profiles3Client, bucket);
            }
            catch (Exception e)
            {
                Log.Logger.Error("An unknown error occured while checking whether the bucket is created!");
                Log.Logger.Error(e.Message);
            }
            
            if(!bucketExists)
                throw new ApplicationException($"The bucket does not exist for profile!");

            BucketCustomConfiguration bucketConfiguration = new(bucket, serviceUrl, storageConfiguration.Profile.CustomDomain);
            collection.AddKeyedSingleton<IAmazonS3>(ServiceKeyConstants.Storage.ProfileStorage, profiles3Client);
            collection.AddKeyedSingleton(ServiceKeyConstants.Storage.ProfileStorage, bucketConfiguration);
            collection.AddKeyedScoped<IStorageClient, ProfileS3StorageClient>(ServiceKeyConstants.Storage.ProfileStorage);
            Log.Logger.Information("Created profile storage client with bucket configuration of {BucketName} on {ServiceUrl} or {CustomDomain}", bucketConfiguration.Bucket, bucketConfiguration.RegionBasedUrl, bucketConfiguration.CustomDomainUrl ?? "N/A");
        }
        else
        {
            
        }
        
        if (storageConfiguration.Files.Type == StorageType.S3)
        {
            var region = RegionEndpoint.GetBySystemName(secretTasks[VaultConstants.Storage.S3.BucketWithPrefix(storageConfiguration.Files.VaultPrefix)].Result);
            var serviceUrl = secretTasks[VaultConstants.Storage.S3.EndpointWithPrefix(storageConfiguration.Files.VaultPrefix)].Result;
            var accessKey = secretTasks[VaultConstants.Storage.S3.ConnectionAccessKeyWithPrefix(storageConfiguration.Files.VaultPrefix)].Result;
            var secretKey = secretTasks[VaultConstants.Storage.S3.ConnectionSecretKeyWithPrefix(storageConfiguration.Files.VaultPrefix)].Result;
        }
        else
        {
            
        }
    }

    private static async Task ConfigureRabbitMq(this IServiceCollection collection, IConfiguration configuration, ISecretClient secretClient)
    {
        List<Task<string>> vaultTasks =
        [
            secretClient.GetSecretAsync(VaultConstants.Messaging.RabbitMQ.HostName),
            secretClient.GetSecretAsync(VaultConstants.Messaging.RabbitMQ.Port),
            secretClient.GetSecretAsync(VaultConstants.Messaging.RabbitMQ.Username),
            secretClient.GetSecretAsync(VaultConstants.Messaging.RabbitMQ.Password),
            secretClient.GetSecretAsync(VaultConstants.Messaging.RabbitMQ.VirtualHost),
        ];

        var secretTaskValues = await Task.WhenAll(vaultTasks);
        var rabbitMqHostName = secretTaskValues[0];
        var rabbitMqPort = secretTaskValues[1];
        var rabbitMqUserName = secretTaskValues[2];
        var rabbitMqPassword = secretTaskValues[3];
        var rabbitMqVirtualHost = secretTaskValues[4];
        
        if (!ushort.TryParse(rabbitMqPort, out var port))
            throw new ApplicationException("Invalid port in messaging configuration!");

        var connectionName = configuration["Messaging:ConnectionName"];
        collection.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();
            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(rabbitMqHostName, port: port, rabbitMqVirtualHost, h =>
                {
                    if(!string.IsNullOrWhiteSpace(connectionName))
                        h.ConnectionName(connectionName);
                    
                    h.Username(rabbitMqUserName);
                    h.Password(rabbitMqPassword);
                });
                
                configurator.ConfigureEndpoints(context);
            });
        });
    }

    private static Task ConfigureAmazonSqs(this IServiceCollection collection, ISecretClient secretClient)
    {
        return Task.CompletedTask;
    }


    private static void ConfigurePgSql(this IServiceCollection serviceCollection)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}