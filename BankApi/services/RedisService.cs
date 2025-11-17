namespace BankApi.Services;
using StackExchange.Redis;

public class RedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;                
        _db = _redis.GetDatabase();     
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        await _db.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetStringAsync(string key)
    {
        return await _db.StringGetAsync(key);
    }

    public async Task<TimeSpan?> GetTTLAsync(string key)
    {
        return await _db.KeyTimeToLiveAsync(key);
    }
}
