namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides keyboard control operations including hotkeys and text entry
/// </summary>
public interface IKeyboardController
{
    /// <summary>
    /// Sends a single key press (down and up)
    /// </summary>
    /// <param name="key">Key to press</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PressKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a hotkey combination
    /// </summary>
    /// <param name="hotkey">Hotkey to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendHotkeyAsync(Hotkey hotkey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a key down action
    /// </summary>
    /// <param name="key">Key to press down</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task KeyDownAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a key up action
    /// </summary>
    /// <param name="key">Key to release</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task KeyUpAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enters text at the current cursor position
    /// </summary>
    /// <param name="text">Text to enter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task EnterTextAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a sequence of keys
    /// </summary>
    /// <param name="keys">Keys to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendKeysAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets the delay between keystrokes in milliseconds
    /// </summary>
    int KeystrokeDelayMilliseconds { get; set; }
}
