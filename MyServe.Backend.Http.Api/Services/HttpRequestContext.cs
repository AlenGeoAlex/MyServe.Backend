using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Impl;

namespace MyServe.Backend.Api.Services;

public class HttpRequestContext(IHttpContextAccessor contextAccessor) : IRequestContext
{
    private readonly HashSet<string> _dependingCacheKeys = [];
    private readonly CacheControl _cacheControl = new CacheControl();
    private IRequester? _requester;
    public IRequester Requester => _requester ??= Initialize();
    
    public bool TryGetHeader(string header, [MaybeNullWhen(false)] out string response)
    {
        response = null;
        
        if (contextAccessor.HttpContext is null) return false;
        if (!contextAccessor.HttpContext.Request.Headers.TryGetValue(header, out var data)) return false;
        response = data;
        
        #pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
        return true;
        #pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
    }

    public ICacheControl CacheControl => _cacheControl;

    public HashSet<string> DependingCacheKeys => _dependingCacheKeys;
    
    public void AddDependingCacheKey(params string[] keys)
    {
        foreach (var key in keys)
        {
            _dependingCacheKeys.Add(key);
        }
    }

    private IRequester Initialize()
    {
        if (contextAccessor.HttpContext is null)
            return new NoRequester();
        
        if(string.IsNullOrEmpty(contextAccessor.HttpContext.Request.Headers.Authorization))
            return new NoRequester();
        
        var contextUser = contextAccessor.HttpContext.User;
        if(!contextUser.Identity?.IsAuthenticated ?? false)
            return new NoRequester();
        
        var userId = contextUser.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = contextUser.FindFirstValue(ClaimTypes.Email);
        
        if(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || !Guid.TryParse(userId, out var parsedUserId))
            throw new UnauthorizedAccessException();
        
        _requester = new UserRequester(
            parsedUserId,
            email
        );
        return _requester;
    }
}