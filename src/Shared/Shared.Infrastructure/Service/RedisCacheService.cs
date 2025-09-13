using System.Text.Json;
using Shared.Core.Interface;
using StackExchange.Redis;

namespace Shared.Infrastructure.Service;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[RedisCache] Failed to deserialize key '{key}': {ex.Message}");
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiration ?? TimeSpan.FromMinutes(30));
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}