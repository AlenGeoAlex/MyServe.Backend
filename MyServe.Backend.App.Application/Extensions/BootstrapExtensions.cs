using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.App.Application.Behaviours;

namespace MyServe.Backend.App.Application.Extensions;

public static class BootstrapExtension
{
    public static async Task<IServiceCollection> ConfigureApplication(this IServiceCollection services, IConfiguration configuration, ISecretClient secretClient)
    {
        var assembly = typeof(BootstrapExtension).Assembly;
        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(conf =>
        {
            conf.RegisterServicesFromAssembly(assembly);
            conf.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });
        return services;
    }
    
}