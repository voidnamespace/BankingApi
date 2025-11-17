namespace BankApi.Services;
using StackExchange.Redis;
using System.Text.Json;

public class CacheService
{
    private readonly IConnectionMultiplexer _redis;

    public CacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);

        string? stringValue = value;          
        if (string.IsNullOrEmpty(stringValue))
            return default;

        return JsonSerializer.Deserialize<T>(stringValue)!; 
    }


    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, json, ttl);
    }

    public async Task RemoveAsync(string key)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}

