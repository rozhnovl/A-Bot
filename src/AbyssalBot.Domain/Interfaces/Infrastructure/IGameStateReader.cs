namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides parsed game state information from memory
/// </summary>
public interface IGameStateReader
{
    /// <summary>
    /// Reads the current game state
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current game state</returns>
    Task<GameState> ReadCurrentStateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current system location
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<SystemLocation?> GetCurrentLocationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the player is currently in an abyssal deadspace
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> IsInAbyssalSpaceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current game time
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<DateTimeOffset> GetGameTimeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the game client is ready for input
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> IsClientReadyAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the complete game state
/// </summary>
public record GameState(
    SystemLocation? Location,
    DateTimeOffset Timestamp,
    bool IsInAbyssalSpace,
    bool IsClientReady,
    WindowState WindowState
);

/// <summary>
/// Represents a system location in EVE
/// </summary>
public record SystemLocation(
    string SystemName,
    string RegionName,
    double SecurityStatus
);

/// <summary>
/// Represents window state information
/// </summary>
public record WindowState(
    bool IsVisible,
    bool IsForeground,
    IntPtr Handle
);
