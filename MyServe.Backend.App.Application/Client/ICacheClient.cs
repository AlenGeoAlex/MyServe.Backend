namespace MyServe.Backend.App.Application.Client;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, T? defaultValue = default, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = default, CancellationToken cancellationToken = default);

    Task<int> ClearMatchingAsync(HashSet<string> patterns, CancellationToken token = default);


}