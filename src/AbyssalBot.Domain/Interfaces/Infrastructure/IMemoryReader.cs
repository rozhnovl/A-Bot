namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides low-level raw memory access to EVE Online client process
/// </summary>
public interface IMemoryReader
{
    /// <summary>
    /// Reads raw memory measurement from the game client
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Raw memory measurement data</returns>
    Task<RawMemoryMeasurement> ReadMemoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the window handle of the EVE client
    /// </summary>
    IntPtr WindowHandle { get; }

    /// <summary>
    /// Checks if the memory reader is connected to a valid process
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Attempts to attach to an EVE Online client process
    /// </summary>
    /// <param name="processId">Optional process ID to attach to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> AttachToProcessAsync(int? processId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Detaches from the current process
    /// </summary>
    void Detach();
}

/// <summary>
/// Represents raw memory measurement data from EVE client
/// </summary>
public record RawMemoryMeasurement(
    DateTimeOffset Timestamp,
    object MemoryData,
    IntPtr WindowHandle
);
