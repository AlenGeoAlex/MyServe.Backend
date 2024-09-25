using System.Diagnostics.CodeAnalysis;

namespace MyServe.Backend.Common.Abstract;

public interface IRequestContext
{
    /**
     * Information regarding the request 
     */
    IRequester Requester { get; }

    /**
     * Get a value from Header
     */
    bool TryGetHeader(string header, [MaybeNullWhen(false)] out string response);
    
    /**
     * Endpoint CacheControl
     */
    ICacheControl CacheControl { get; }
    

}