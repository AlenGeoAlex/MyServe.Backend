namespace MyServe.Backend.Common.Abstract;

public interface ICacheControl
{
    /**
     * Get the depending on cache keys for this request
     */
    IReadOnlySet<string> ExpiryKeys { get; }
    
    /**
     * Add keys to the cache module
     */
    void AddKeyToExpire(params string[] keys);
    
    /**
     * Removes the exact key to delete 
     */
    void AddExactKeyToExpire(params string[] keys);
    
    /**
     * Cache Key of the particular request
     */
    string EndpointCacheKey { get; }
    
    /**
     * Construct a EndpointCacheKey
     */
    void FrameEndpointCacheKey(string module, params string[] keys);

    /**
     * Can the endpoint be cached
     */
    bool IsEndpointCached { get; }
}