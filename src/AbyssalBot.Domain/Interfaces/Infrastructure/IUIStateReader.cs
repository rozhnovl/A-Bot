namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides UI window state information from memory
/// </summary>
public interface IUIStateReader
{
    /// <summary>
    /// Checks if a specific window is visible
    /// </summary>
    /// <param name="windowType">Type of window to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> IsWindowVisibleAsync(UIWindowType windowType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the bounds of a specific UI window
    /// </summary>
    /// <param name="windowType">Type of window</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<WindowBounds?> GetWindowBoundsAsync(UIWindowType windowType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all visible notification messages
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<Notification>> ReadNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any modal dialog is currently open
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> HasModalDialogAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current modal dialog information
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ModalDialog?> GetModalDialogAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the current route information from the UI
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<RouteInfo?> ReadRouteInfoAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents different UI window types in EVE
/// </summary>
public enum UIWindowType
{
    Overview,
    SelectedItem,
    Inventory,
    DroneBay,
    FleetWindow,
    LocalChat,
    StationServices,
    AgencyWindow,
    Neocom
}

/// <summary>
/// Represents window bounds in screen coordinates
/// </summary>
public record WindowBounds(
    int X,
    int Y,
    int Width,
    int Height
)
{
    /// <summary>
    /// Gets the center point of the window
    /// </summary>
    public (int X, int Y) Center => (X + Width / 2, Y + Height / 2);

    /// <summary>
    /// Checks if a point is within the window bounds
    /// </summary>
    public bool Contains(int x, int y) =>
        x >= X && x <= X + Width && y >= Y && y <= Y + Height;
}

/// <summary>
/// Represents a notification message
/// </summary>
public record Notification(
    string Message,
    NotificationType Type,
    DateTimeOffset Timestamp
);

/// <summary>
/// Represents notification types
/// </summary>
public enum NotificationType
{
    Info,
    Warning,
    Error,
    Combat,
    System
}

/// <summary>
/// Represents a modal dialog window
/// </summary>
public record ModalDialog(
    string Title,
    string Message,
    IReadOnlyList<string> ButtonLabels
);

/// <summary>
/// Represents route information
/// </summary>
public record RouteInfo(
    int JumpsRemaining,
    string? DestinationSystem,
    bool IsAutopilotActive
);
