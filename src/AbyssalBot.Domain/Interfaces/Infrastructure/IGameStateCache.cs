namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides caching for game state to reduce memory reading overhead
/// </summary>
public interface IGameStateCache
{
    /// <summary>
    /// Caches the current game state
    /// </summary>
    /// <param name="state">Game state to cache</param>
    /// <param name="expirationSeconds">Cache expiration in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetGameStateAsync(
        GameState state,
        int expirationSeconds = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the cached game state if available
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<GameState?> GetGameStateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Caches ship state information
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expirationSeconds">Cache expiration in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetShipStateAsync<T>(
        string key,
        T value,
        int expirationSeconds = 2,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets cached ship state information
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<T?> GetShipStateAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Invalidates all cached game state
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates a specific cache entry
    /// </summary>
    /// <param name="key">Cache key to invalidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a cache entry exists and is not expired
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents cache statistics
/// </summary>
public record CacheStatistics(
    long TotalKeys,
    long Hits,
    long Misses,
    double HitRate
)
{
    /// <summary>
    /// Gets the hit rate as a percentage
    /// </summary>
    public double HitRatePercentage => HitRate * 100;
}
