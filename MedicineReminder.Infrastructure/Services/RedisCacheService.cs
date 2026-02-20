using MedicineReminder.Application.Common.Interfaces;
using StackExchange.Redis;

namespace MedicineReminder.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private const string HotSetKey = "reminders_hot";

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task AddReminderToHotSetAsync(string reminderId, DateTime reminderUtc)
    {
        var db = _redis.GetDatabase();
        var score = new DateTimeOffset(reminderUtc).ToUnixTimeSeconds();
        await db.SortedSetAddAsync(HotSetKey, reminderId, score);
    }

    public async Task RemoveReminderFromHotSetAsync(string reminderId)
    {
        var db = _redis.GetDatabase();
        await db.SortedSetRemoveAsync(HotSetKey, reminderId);
    }

    public async Task<List<string>> GetDueRemindersAsync(DateTime utcNow)
    {
        var db = _redis.GetDatabase();
        var currentScore = new DateTimeOffset(utcNow).ToUnixTimeSeconds();

        var script = LuaScript.Prepare(@"
            local items = redis.call('ZRANGEBYSCORE', @key, '-inf', @score)
            if #items > 0 then
                redis.call('ZREM', @key, unpack(items))
            end
            return items");

        var result = (RedisResult[]?)await db.ScriptEvaluateAsync(script, new { key = (RedisKey)HotSetKey, score = currentScore });

        return result?.Select(x => x.ToString()).ToList() ?? new List<string>();
    }
}
