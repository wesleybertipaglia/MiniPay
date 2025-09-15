using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shared.Core.Interface;
using StackExchange.Redis;

namespace Shared.Infrastructure.Service;

public class RedisCacheService(
    IConnectionMultiplexer redis,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                logger.LogDebug("Cache miss for key '{Key}'", key);
                return default;
            }

            var result = JsonSerializer.Deserialize<T>(value!);
            logger.LogDebug("Cache hit for key '{Key}'", key);
            return result;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize cache key '{Key}'", key);
            return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving key '{Key}'", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiration ?? TimeSpan.FromMinutes(30));

            logger.LogInformation("Cached key '{Key}' with expiration {Expiration}",
                key, expiration ?? TimeSpan.FromMinutes(30));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set cache for key '{Key}'", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
            logger.LogInformation("Removed cache key '{Key}'", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove cache key '{Key}'", key);
        }
    }
}
