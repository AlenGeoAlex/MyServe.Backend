using System.Data;
using System.Data.Common;
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
using MyServe.Backend.App.Infrastructure.Repositories;
using MyServe.Backend.App.Infrastructure.Services;
using MyServe.Backend.App.Infrastructure.TypeHandler;
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
        //await services.ConfigureSupabase(secretClient);
        await services.ConfigureCache(configuration, secretClient);
        await services.ConfigureDatabase(secretClient);
        await services.ConfigureMessaging(configuration, secretClient);
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