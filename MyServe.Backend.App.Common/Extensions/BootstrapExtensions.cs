using Infisical.Sdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Impl;
using Serilog;

namespace MyServe.Backend.Common.Extensions;

public static class BootstrapExtensions
{
    
    /**
     * Initialize the Vault Service for the task
     */
    public static async Task<ISecretClient> InitializeSecretClient(this IServiceCollection collection, IConfiguration configuration)
    {
        var secretClient = new InfisicalSecretClient(configuration);
        await secretClient.InitializeAsync();
        Log.Logger.Information("Initialized secret client");
        collection.AddSingleton<ISecretClient>(secretClient);
        return secretClient;
    }
    
}