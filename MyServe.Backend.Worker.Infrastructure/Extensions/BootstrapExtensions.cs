using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Worker.Infrastructure.Services;
using Serilog;

namespace MyServe.Backend.Worker.Infrastructure.Extensions;

public static class BootstrapExtensions
{
    public static async Task ConfigureJobInfrastructure(this IServiceCollection services, IConfiguration configuration, ISecretClient secretClient)
    {
        services.AddSingleton<IRequestContext, JobRequestContext>();
        services.ConfigureSmtp(configuration);
    }

    private static void ConfigureSmtp(this IServiceCollection services, IConfiguration configuration)
    {
        var smtpEnabled = configuration["Smtp:Enabled"] ?? bool.FalseString;

        if (!bool.TryParse(smtpEnabled, out var smtpEnabledValue))
        {
            Log.Logger.Information("Smtp enabled is false since the provided value is "+smtpEnabledValue);
            smtpEnabledValue = false;
        }

        if (!smtpEnabledValue)
            services.AddSingleton<IEmailClient, UnknownEmailClient>();
        else
            services.AddSingleton<IEmailClient, SmtpEmailClient>();
    }
}