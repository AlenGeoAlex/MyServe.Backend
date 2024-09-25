using Microsoft.AspNetCore.Mvc;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.App.Application.Client;
using ILogger = Serilog.ILogger;

namespace MyServe.Backend.Api.Controllers;

public class AbstractController(ICacheService cacheService, ILogger logger, IRequestContext requestContext) : ControllerBase
{
    
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);
    protected CancellationToken RequestCancellationToken => HttpContext.RequestAborted;
    
    private const string CacheControlHeader = "Cache-Control";
    private const string NoCache = "no-cache";
    private const string NoStore = "no-store";
    private const string CacheStatusHeader = "X-Cache-Status";
    private const string SkippedDueToDirective = "Skipped due to directive";
    private const string CachedResourceHeader = "response-cache";
    
    protected async Task<TEntity?> ScanAsync<TEntity>(bool appendHeaderIfMatched = true, CancellationToken token = default)
    {
        try
        {
            if (!requestContext.CacheControl.IsEndpointCached)
                return default;
            
            if (ShouldSkipCache(NoCache))
            {
                Response.Headers.Append(CacheStatusHeader, SkippedDueToDirective);
                return default;
            }
            
            var cacheObject = await cacheService.GetAsync<TEntity>(requestContext.CacheControl.EndpointCacheKey, cancellationToken: token);
            if (cacheObject == null)
                return default;

            if (!appendHeaderIfMatched) 
                return cacheObject;
            
            Response.Headers.Append(CachedResourceHeader, "true");
            Response.Headers.AccessControlExposeHeaders = CachedResourceHeader;

            return cacheObject;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error during cache retrieval");
            return default;
        }
    }

    protected async Task CacheAsync(object entityResponse, TimeSpan? ttl = null, CancellationToken token = default)
    {
        try
        {
            if (!requestContext.CacheControl.IsEndpointCached)
                return;
            
            if (ShouldSkipCache(NoStore))
            {
                Response.Headers.Append(CacheStatusHeader, SkippedDueToDirective);
                return;
            }

            ttl ??= CacheTtl;
            await cacheService.SetAsync(requestContext.CacheControl.EndpointCacheKey, entityResponse, ttl, token);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error during cache store");
        }
    }

    private bool ShouldSkipCache(string directive)
    {
        return requestContext.TryGetHeader(CacheControlHeader, out var headerValue) && headerValue.Equals(directive, StringComparison.OrdinalIgnoreCase);
    }
    

    protected string GetQuery()
    {
        return Request.QueryString.ToString();
    }
    

    protected bool HasFiles => Request.Form.Files.Any();
    protected bool HasFilesWithName(string fileName) => Request.Form.Files.Any(x => x.Name == fileName);

    protected Stream? GetFileAsStream(string fileName)
    {
        var formFile = Request.Form.Files.GetFile(fileName);
        return formFile?.OpenReadStream();
    }
    
}