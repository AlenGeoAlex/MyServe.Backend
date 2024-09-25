using System.Text;
using FluentValidation;
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
}