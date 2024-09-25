using System.Text.Json;
using MyServe.Backend.App.Application.Client;
using Serilog;
using StackExchange.Redis;

namespace MyServe.Backend.App.Infrastructure.Client.Cache;

public class RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger logger) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, T? defaultValue = default, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisResponse = await Database.StringGetAsync(new RedisKey(key));
            if (string.IsNullOrWhiteSpace(redisResponse.ToString()))
            {
                logger.Information("Redis has been missed for key: {Key}", key);
                return defaultValue;
            }

            logger.Information("Redis has been successfully hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(redisResponse.ToString());
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Failed to get the value from redis database with key: {Key}", key);
            return defaultValue;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = default, CancellationToken cancellationToken = default)
    {
        try
        {
            if(value == null)
                return;

            string stringValue;
            if (value is string castString)
                stringValue = castString;
            else
                stringValue = JsonSerializer.Serialize(value);
            
            expiration ??= TimeSpan.FromMinutes(10);
            await Database.StringSetAsync(new RedisKey(key), stringValue, expiration, When.Always, CommandFlags.FireAndForget);
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to set the value from redis database with key: {key}", key);
        }
    }

    public async Task<int> ClearMatchingAsync(HashSet<string> patterns, CancellationToken token = default)
    {
        var deletedCount = 0;
        try
        {
            foreach (var pattern in patterns)
            {
                var wildcardPattern = pattern;
                if (!wildcardPattern.EndsWith("*"))
                    wildcardPattern += "*";
                long cursor = 0;
                do
                {
                    var scanResult = await Database.ExecuteAsync("SCAN", cursor.ToString(), "MATCH", wildcardPattern);
                    var innerResult = (RedisResult[])scanResult;

                    cursor = (long)innerResult[0];
                    var keysToDelete = (RedisKey[])innerResult[1];

                    if ( keysToDelete is { Length: > 0 })
                    {
                        var deleteTasks = keysToDelete.Select(key => Database.KeyDeleteAsync(key));
                        var results = await Task.WhenAll(deleteTasks);
                        deletedCount += results.Count(deleted => deleted);
                    }

                    if (!token.IsCancellationRequested) continue;
                    
                    logger.Information("Cancellation requested, stopping deletion.");
                    break;
                } while (cursor != 0);
            }
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to delete keys with patterns {Key}", string.Join(",", patterns));
        }
        return deletedCount;
    }
    
    private IDatabase Database => connectionMultiplexer.GetDatabase();
}