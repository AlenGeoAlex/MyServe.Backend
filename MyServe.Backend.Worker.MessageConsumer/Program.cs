using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyServe.Backend.App.Application.Extensions;
using MyServe.Backend.App.Domain.Extensions;
using MyServe.Backend.App.Infrastructure.Extensions;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants.EmailTemplate;
using MyServe.Backend.Common.Extensions;
using MyServe.Backend.Worker.Infrastructure.Extensions;
using MyServe.Backend.Worker.Infrastructure.Services;
using MyServe.Backend.Worker.MessageConsumer.Extensions;
using Serilog;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        builder.Services.AddSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(configuration);
        });

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    
        var secretClient = await builder.Services.InitializeSecretClient(configuration);

        await builder.Services.ConfigureJob(configuration, secretClient);
        await builder.Services.ConfigureJobInfrastructure(configuration, secretClient);
        await builder.Services.ConfigureInfrastructure(configuration, secretClient);
        await builder.Services.ConfigureApplication(configuration, secretClient);
        await builder.Services.ConfigureDomain(configuration, secretClient);

        var host = builder.Build();
        
        await host.RunAsync();
    }
}