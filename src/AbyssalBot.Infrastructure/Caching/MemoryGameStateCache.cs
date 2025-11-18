using AbyssalBot.Domain.Interfaces.Infrastructure;
using Microsoft.Extensions.Caching.Memory;

namespace AbyssalBot.Infrastructure.Caching;

/// <summary>
/// In-memory implementation of IGameStateCache
/// </summary>
public class MemoryGameStateCache : IGameStateCache
{
    private readonly IMemoryCache _cache;
    private long _hits;
    private long _misses;

    public MemoryGameStateCache(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task SetGameStateAsync(
        GameState state,
        int expirationSeconds = 5,
        CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationSeconds)
        };

        _cache.Set("gamestate:current", state, options);
        await Task.CompletedTask;
    }

    public async Task<GameState?> GetGameStateAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue("gamestate:current", out GameState? state))
        {
            Interlocked.Increment(ref _hits);
            return await Task.FromResult(state);
        }

        Interlocked.Increment(ref _misses);
        return null;
    }

    public async Task SetShipStateAsync<T>(
        string key,
        T value,
        int expirationSeconds = 2,
        CancellationToken cancellationToken = default) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationSeconds)
        };

        _cache.Set($"shipstate:{key}", value, options);
        await Task.CompletedTask;
    }

    public async Task<T?> GetShipStateAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue($"shipstate:{key}", out T? value))
        {
            Interlocked.Increment(ref _hits);
            return await Task.FromResult(value);
        }

        Interlocked.Increment(ref _misses);
        return null;
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        // MemoryCache doesn't support clearing all entries
        // Would need to track keys separately for full implementation
        await Task.CompletedTask;
    }

    public async Task InvalidateAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public async Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var total = _hits + _misses;
        var hitRate = total > 0 ? (double)_hits / total : 0;

        return await Task.FromResult(new CacheStatistics(
            TotalKeys: 0, // Would need to track keys
            Hits: _hits,
            Misses: _misses,
            HitRate: hitRate
        ));
    }
}
