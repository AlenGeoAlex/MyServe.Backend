using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Impl;
using Serilog;

namespace MyServe.Backend.Api.Extensions;

public static class BootstrapExtensions
{

    public static async Task<IServiceCollection> ConfigureApi(this IServiceCollection services, IConfiguration configuration, ISecretClient secretClient)
    {
        services.AddValidatorsFromAssembly(typeof(BootstrapExtensions).Assembly);
        await services.ConfigureTokenValidation(secretClient);
        ConfigureCors(services);
        services.BuildRateLimiter();
        return services;
    }

    private static async Task ConfigureTokenValidation(this IServiceCollection services, ISecretClient secretClient)
    {
        var secretTasks = new List<Task<string>>();
        secretTasks.AddRange([secretClient.GetSecretAsync(VaultConstants.Jwt.SigningKey), secretClient.GetSecretAsync(VaultConstants.Jwt.EncryptionKey)]);
        var secretTaskResponse = await Task.WhenAll(secretTasks);
        var audienceValue = TokenGenerationOption.JwtAudience;
        var issuerValue = TokenGenerationOption.JwtIssuer;
        var issuerSigningValue = secretTaskResponse[0];
        var issuerDecryptionValue = secretTaskResponse[01];
        var signingKeyBytes = Encoding.UTF8.GetBytes(issuerSigningValue);
        var decryptionKeyBytes = Encoding.UTF8.GetBytes(issuerDecryptionValue);
        Log.Logger.Information("Configured token validation");
        services.AddAuthentication().AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                TokenDecryptionKey = new SymmetricSecurityKey(decryptionKeyBytes),
                IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
                ValidAudience = audienceValue,
                ValidIssuer = issuerValue
            };
        });
    }

    private static IServiceCollection ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(x =>
        {
            x.AddPolicy("cors", builder =>
            {
                builder.AllowAnyMethod();
                builder.AllowCredentials();
                builder.AllowAnyHeader();
                builder.SetIsOriginAllowed(_ => true);
            });
        });
        return services;
    }
    
    public static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
    {
        var builder = new ServiceCollection()
            .AddLogging()
            .AddMvc()
            .AddNewtonsoftJson()
            .Services.BuildServiceProvider();

        return builder
            .GetRequiredService<IOptions<MvcOptions>>()
            .Value
            .InputFormatters
            .OfType<NewtonsoftJsonPatchInputFormatter>()
            .First();
    }

    private static IServiceCollection BuildRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(policyName: RateLimitingPolicyConstants.RateLimit1To1, policyOptions =>
            {
                policyOptions.PermitLimit = 1;
                policyOptions.Window = TimeSpan.FromSeconds(1);
                policyOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                policyOptions.QueueLimit = 20;
            });
        });
        return services;
    }
}