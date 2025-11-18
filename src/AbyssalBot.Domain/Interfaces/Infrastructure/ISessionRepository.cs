namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Repository for bot session tracking and history
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// Creates a new bot session
    /// </summary>
    /// <param name="session">Session to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Guid> CreateSessionAsync(
        BotSession session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing session
    /// </summary>
    /// <param name="session">Session to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateSessionAsync(
        BotSession session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends a bot session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="endReason">Reason for ending</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task EndSessionAsync(
        Guid sessionId,
        string endReason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a session by ID
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<BotSession?> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current active session
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<BotSession?> GetActiveSessionAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets session history with filtering
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<BotSession>> GetSessionHistoryAsync(
        SessionHistoryFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a bot action in the current session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="action">Action to record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordActionAsync(
        Guid sessionId,
        BotAction action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets actions for a session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<BotAction>> GetSessionActionsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets session statistics
    /// </summary>
    /// <param name="filter">Optional filter criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<SessionStatistics> GetSessionStatisticsAsync(
        SessionHistoryFilter? filter = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a bot session
/// </summary>
public record BotSession(
    Guid Id,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    string BotVersion,
    string CharacterName,
    SessionStatus Status,
    string? EndReason = null,
    Dictionary<string, object>? Metadata = null
)
{
    /// <summary>
    /// Gets the duration of the session
    /// </summary>
    public TimeSpan Duration => (EndTime ?? DateTimeOffset.UtcNow) - StartTime;

    /// <summary>
    /// Checks if the session is currently active
    /// </summary>
    public bool IsActive => Status == SessionStatus.Active;
}

/// <summary>
/// Represents session status
/// </summary>
public enum SessionStatus
{
    Active,
    Paused,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Represents a bot action
/// </summary>
public record BotAction(
    Guid Id,
    Guid SessionId,
    DateTimeOffset Timestamp,
    string ActionType,
    string Description,
    bool Success,
    Dictionary<string, object>? Details = null
);

/// <summary>
/// Represents session history filter criteria
/// </summary>
public record SessionHistoryFilter(
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null,
    SessionStatus? Status = null,
    string? CharacterName = null,
    int Skip = 0,
    int Take = 100
);

/// <summary>
/// Represents session statistics
/// </summary>
public record SessionStatistics(
    int TotalSessions,
    int CompletedSessions,
    int FailedSessions,
    TimeSpan TotalRunTime,
    TimeSpan AverageSessionDuration,
    int TotalActions,
    double ActionsPerMinute
);
