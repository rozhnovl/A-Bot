namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Generic cache provider for arbitrary data
/// </summary>
/// <typeparam name="T">Type of data to cache</typeparam>
public interface ICacheProvider<T> where T : class
{
    /// <summary>
    /// Gets a value from the cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<T?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in the cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expirationSeconds">Cache expiration in seconds (null = no expiration)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync(
        string key,
        T value,
        int? expirationSeconds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from cache or computes it if not found
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Factory function to compute value if not cached</param>
    /// <param name="expirationSeconds">Cache expiration in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<T> GetOrCreateAsync(
        string key,
        Func<Task<T>> factory,
        int? expirationSeconds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all values matching a key pattern
    /// </summary>
    /// <param name="pattern">Key pattern (e.g., "ship:*")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in the cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all cached values
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the expiration time for an existing cache entry
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="expirationSeconds">New expiration in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetExpirationAsync(
        string key,
        int expirationSeconds,
        CancellationToken cancellationToken = default);
}
