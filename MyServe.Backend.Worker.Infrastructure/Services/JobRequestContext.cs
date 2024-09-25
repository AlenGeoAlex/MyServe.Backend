using System.Diagnostics.CodeAnalysis;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Impl;

namespace MyServe.Backend.Worker.Infrastructure.Services;

public class JobRequestContext : IRequestContext
{
    public IRequester Requester => new NoRequester();
    public bool TryGetHeader(string header, [MaybeNullWhen(false)] out string response)
    {
        response = null;
        return false;
    }
    public ICacheControl CacheControl => new CacheControl();
}