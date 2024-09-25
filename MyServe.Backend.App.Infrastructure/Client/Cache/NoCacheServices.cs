using MyServe.Backend.App.Application.Client;

namespace MyServe.Backend.App.Infrastructure.Client.Cache;

public class NoCacheServices : ICacheService 
{
    public Task<T?> GetAsync<T>(string key, T? defaultValue = default, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((T?)default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = default, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<int> ClearMatchingAsync(HashSet<string> patterns, CancellationToken token = default)
    {
        return Task.FromResult(0);
    }
}