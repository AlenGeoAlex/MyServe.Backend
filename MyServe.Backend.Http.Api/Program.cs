using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using MyServe.Backend.Api.Extensions;
using MyServe.Backend.Api.Middleware;
using MyServe.Backend.Api.Services;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Extensions;
using MyServe.Backend.App.Application.Extensions;
using MyServe.Backend.App.Domain.Extensions;
using MyServe.Backend.App.Infrastructure.Extensions;
using Serilog;

namespace MyServe.Backend.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
        });
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();
        
        var secretClient = await builder.Services.InitializeSecretClient(builder.Configuration);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
        
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IRequestContext, HttpRequestContext>();

        await builder.Services.ConfigureApi(builder.Configuration, secretClient);
        await builder.Services.ConfigureApplication(builder.Configuration, secretClient);
        await builder.Services.ConfigureInfrastructure(builder.Configuration, secretClient);
        await builder.Services.ConfigureDomain(builder.Configuration, secretClient);
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseSerilogRequestLogging(); // Log requests

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(); // Swagger for API documentation
            app.UseSwaggerUI();
        }
        
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        
        app.UseCors("cors"); 
        app.UseHttpsRedirection(); 
        app.UseAuthentication(); 
        app.UseRouting(); 
        // app.UseRateLimiter(); 
        app.UseAuthorization();
        app.UseMiddleware<CacheMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapControllers(); 

        await app.RunAsync();
    }
}