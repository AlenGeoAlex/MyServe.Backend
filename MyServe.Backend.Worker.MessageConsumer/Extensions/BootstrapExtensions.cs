using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Worker.MessageConsumer.Consumer;
using Serilog;

namespace MyServe.Backend.Worker.MessageConsumer.Extensions;

public static class BootstrapExtensions
{
    public static async Task ConfigureJob(this IServiceCollection services, IConfiguration configuration, ISecretClient secretClient)
    {
        services.RegisterMessageConsumers();
    }

    private static void RegisterMessageConsumers(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumersFromNamespaceContaining<ConsumerMarker>((type =>
            {
                Log.Logger.Information($"Consumer {type.Name} configured as a consumer");
                return true;
            }));
        });
    }
}