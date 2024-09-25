using MyServe.Backend.Api.Attributes;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.App.Application.Client;
using Serilog.Core;
using ILogger = Serilog.ILogger;

namespace MyServe.Backend.Api.Middleware;

public class CacheMiddleware(RequestDelegate next, ILogger logger)
{

    public async Task InvokeAsync(HttpContext context, IRequestContext requestContext, ICacheService cacheService)
    {
        await next(context);
        var endpoint = context.GetEndpoint();
        {
            var noCacheWipeAttribute = endpoint?.Metadata.GetMetadata<NoCacheWipeAttribute>();
            if(noCacheWipeAttribute != null)
                return;

            await ClearCacheAsync(requestContext, cacheService);
        }
    }

    private async Task StoreCacheAsync(HttpContext context, IRequestContext requestContext, ICacheService cacheService)
    {
        try
        {
            var key = requestContext.CacheControl.EndpointCacheKey;
        }
        catch (Exception e)
        {
            logger.Error(e, e.Message);
        }
    }

    private async Task ClearCacheAsync(IRequestContext requestContext, ICacheService cacheService)
    {
        try
        {
            if (requestContext.CacheControl.ExpiryKeys.Count == 0)
                return;

            await cacheService.ClearMatchingAsync((HashSet<string>)requestContext.CacheControl.ExpiryKeys);
        }
        catch (Exception e)
        {
            logger.Error(e, e.Message);
        }
    }
    
}