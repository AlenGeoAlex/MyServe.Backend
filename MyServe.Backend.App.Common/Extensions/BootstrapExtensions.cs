using Infisical.Sdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Impl;
using MyServe.Backend.Common.Impl.Vault;
using Serilog;

namespace MyServe.Backend.Common.Extensions;

public static class BootstrapExtensions
{
    
    /**
     * Initialize the Vault Service for the task
     */
    public static async Task<ISecretClient> InitializeSecretClient(this IServiceCollection collection, IConfiguration configuration)
    {
        if (!Enum.TryParse<VaultTypes>(configuration["Vault:Type"], out var type))
            throw new ArgumentException($"Invalid vault type: {configuration["Vault:Type"]}.");

        if (type == VaultTypes.Infisical)
        {
            var secretClient = new InfisicalSecretClient(configuration);
            await secretClient.InitializeAsync();
            Log.Logger.Information("Initialized Infisical secret client");
            collection.AddSingleton<ISecretClient>(secretClient);
            return secretClient;   
        }
        else
        {
            var secretClient = new HashiCorpSecretClient(configuration);
            await secretClient.InitializeAsync();
            Log.Logger.Information("Initialized HashiCorp secret client");
            collection.AddSingleton<ISecretClient>(secretClient);
            return secretClient;   
        }
    }


    public static bool IsRegistered(this IServiceCollection collection, Type type)
    {
        return collection.Any(x => x.ServiceType == type);
    }
    
}