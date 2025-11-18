namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides high-level input simulation abstraction for mouse and keyboard
/// </summary>
public interface IInputSimulator
{
    /// <summary>
    /// Executes a sequence of input actions
    /// </summary>
    /// <param name="actions">Sequence of input actions to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<InputResult> ExecuteActionsAsync(
        IEnumerable<InputAction> actions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clicks at a specific position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="button">Mouse button to click</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<InputResult> ClickAsync(
        int x,
        int y,
        MouseButton button = MouseButton.Left,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a keyboard hotkey
    /// </summary>
    /// <param name="hotkey">Hotkey to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<InputResult> SendHotkeyAsync(
        Hotkey hotkey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enters text at the current cursor position
    /// </summary>
    /// <param name="text">Text to enter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<InputResult> EnterTextAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the delay between input events in milliseconds
    /// </summary>
    int InputDelayMilliseconds { get; set; }
}

/// <summary>
/// Represents an input action
/// </summary>
public abstract record InputAction;

/// <summary>
/// Represents a mouse click action
/// </summary>
public record MouseClickAction(
    int X,
    int Y,
    MouseButton Button = MouseButton.Left
) : InputAction;

/// <summary>
/// Represents a mouse move action
/// </summary>
public record MouseMoveAction(
    int X,
    int Y
) : InputAction;

/// <summary>
/// Represents a keyboard action
/// </summary>
public record KeyboardAction(
    Hotkey Hotkey
) : InputAction;

/// <summary>
/// Represents a text entry action
/// </summary>
public record TextEntryAction(
    string Text
) : InputAction;

/// <summary>
/// Represents a delay action
/// </summary>
public record DelayAction(
    int Milliseconds
) : InputAction;

/// <summary>
/// Represents mouse buttons
/// </summary>
public enum MouseButton
{
    Left,
    Right,
    Middle
}

/// <summary>
/// Represents a keyboard hotkey
/// </summary>
public record Hotkey(
    string Key,
    bool Ctrl = false,
    bool Alt = false,
    bool Shift = false
);

/// <summary>
/// Represents the result of an input operation
/// </summary>
public record InputResult(
    bool Success,
    string? ErrorMessage = null,
    Exception? Exception = null
);
