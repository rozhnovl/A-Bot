namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides time-based caching with automatic expiration and sliding windows
/// </summary>
public interface ITemporalCache
{
    /// <summary>
    /// Gets a value with sliding expiration (expiration resets on each access)
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<T?> GetWithSlidingExpirationAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value with sliding expiration
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="slidingExpirationSeconds">Sliding expiration window in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetWithSlidingExpirationAsync<T>(
        string key,
        T value,
        int slidingExpirationSeconds,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets a value with absolute expiration (expires at a specific time)
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<T?> GetWithAbsoluteExpirationAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value with absolute expiration
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expiresAt">Absolute expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetWithAbsoluteExpirationAsync<T>(
        string key,
        T value,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets a time-series value (last N values over time)
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="count">Number of recent values to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<TimestampedValue<T>>> GetTimeSeriesAsync<T>(
        string key,
        int count = 10,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Adds a value to a time-series
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to add</param>
    /// <param name="maxCount">Maximum number of values to keep in the series</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddToTimeSeriesAsync<T>(
        string key,
        T value,
        int maxCount = 100,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets the remaining time until a cache entry expires
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<TimeSpan?> GetTimeToLiveAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the expiration time for a sliding expiration entry
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RefreshAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a value with a timestamp
/// </summary>
/// <typeparam name="T">Type of the value</typeparam>
public record TimestampedValue<T>(
    T Value,
    DateTimeOffset Timestamp
) where T : class
{
    /// <summary>
    /// Gets the age of this value
    /// </summary>
    public TimeSpan Age => DateTimeOffset.UtcNow - Timestamp;

    /// <summary>
    /// Checks if this value is older than a specific duration
    /// </summary>
    public bool IsOlderThan(TimeSpan duration) => Age > duration;
}
