using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.Common.Abstract;

namespace MyServe.Backend.App.Domain.Extensions;

public static class BootstrapExtension
{
    public static async Task<IServiceCollection> ConfigureDomain(this IServiceCollection services, IConfiguration configuration, ISecretClient secretClient)
    {
        return services;
    }
    
}