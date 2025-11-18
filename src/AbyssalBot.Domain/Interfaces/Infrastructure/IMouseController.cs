namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides mouse control operations including clicks, drags, and movements
/// </summary>
public interface IMouseController
{
    /// <summary>
    /// Moves the mouse to a specific position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MoveToAsync(int x, int y, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clicks at a specific position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="button">Mouse button to click</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClickAtAsync(int x, int y, MouseButton button = MouseButton.Left, CancellationToken cancellationToken = default);

    /// <summary>
    /// Double-clicks at a specific position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="button">Mouse button to double-click</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DoubleClickAtAsync(int x, int y, MouseButton button = MouseButton.Left, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a drag operation from one position to another
    /// </summary>
    /// <param name="fromX">Starting X coordinate</param>
    /// <param name="fromY">Starting Y coordinate</param>
    /// <param name="toX">Ending X coordinate</param>
    /// <param name="toY">Ending Y coordinate</param>
    /// <param name="button">Mouse button to use for dragging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DragAsync(
        int fromX,
        int fromY,
        int toX,
        int toY,
        MouseButton button = MouseButton.Left,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current mouse position
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<(int X, int Y)> GetPositionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Scrolls the mouse wheel
    /// </summary>
    /// <param name="amount">Amount to scroll (positive = up, negative = down)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ScrollAsync(int amount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a mouse down action
    /// </summary>
    /// <param name="button">Mouse button</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MouseDownAsync(MouseButton button = MouseButton.Left, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a mouse up action
    /// </summary>
    /// <param name="button">Mouse button</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MouseUpAsync(MouseButton button = MouseButton.Left, CancellationToken cancellationToken = default);
}
