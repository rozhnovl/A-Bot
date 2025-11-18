namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Executes motion recommendations from the bot logic
/// </summary>
public interface IMotionExecutor
{
    /// <summary>
    /// Executes a sequence of motion commands
    /// </summary>
    /// <param name="motions">Motion commands to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<MotionExecutionResult> ExecuteMotionsAsync(
        IEnumerable<MotionCommand> motions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a single motion command
    /// </summary>
    /// <param name="motion">Motion command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<MotionExecutionResult> ExecuteMotionAsync(
        MotionCommand motion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the game window is in the foreground
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task EnsureWindowForegroundAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets the delay between motion commands in milliseconds
    /// </summary>
    int MotionDelayMilliseconds { get; set; }
}

/// <summary>
/// Represents a motion command
/// </summary>
public record MotionCommand(
    MotionType Type,
    int? X = null,
    int? Y = null,
    MouseButton? MouseButton = null,
    string? Key = null,
    string? Text = null,
    bool BringWindowToForeground = false
);

/// <summary>
/// Represents different motion types
/// </summary>
public enum MotionType
{
    MouseMove,
    MouseClick,
    MouseDown,
    MouseUp,
    KeyPress,
    KeyDown,
    KeyUp,
    TextEntry,
    Delay
}

/// <summary>
/// Represents the result of motion execution
/// </summary>
public record MotionExecutionResult(
    bool Success,
    int MotionsExecuted,
    string? ErrorMessage = null,
    Exception? Exception = null
);
