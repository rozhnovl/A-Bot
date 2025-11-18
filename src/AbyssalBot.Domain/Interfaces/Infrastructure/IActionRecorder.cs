namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Records bot actions for debugging, analysis, and replay
/// </summary>
public interface IActionRecorder
{
    /// <summary>
    /// Records a bot action
    /// </summary>
    /// <param name="action">Action to record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordActionAsync(
        RecordedAction action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a sequence of actions
    /// </summary>
    /// <param name="actions">Actions to record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordActionsAsync(
        IEnumerable<RecordedAction> actions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recorded actions for a specific session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<RecordedAction>> GetSessionActionsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recorded actions within a time range
    /// </summary>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<RecordedAction>> GetActionsByTimeRangeAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recorded actions filtered by type
    /// </summary>
    /// <param name="actionType">Action type filter</param>
    /// <param name="sessionId">Optional session ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<RecordedAction>> GetActionsByTypeAsync(
        ActionType actionType,
        Guid? sessionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a decision made by the bot
    /// </summary>
    /// <param name="decision">Decision record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordDecisionAsync(
        DecisionRecord decision,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets decision history
    /// </summary>
    /// <param name="sessionId">Optional session ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<DecisionRecord>> GetDecisionHistoryAsync(
        Guid? sessionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes action patterns and generates insights
    /// </summary>
    /// <param name="sessionId">Session ID to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ActionAnalysis> AnalyzeActionsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports actions to a file for replay or debugging
    /// </summary>
    /// <param name="sessionId">Session ID to export</param>
    /// <param name="filePath">Export file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExportActionsAsync(
        Guid sessionId,
        string filePath,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a recorded bot action
/// </summary>
public record RecordedAction(
    Guid Id,
    Guid SessionId,
    DateTimeOffset Timestamp,
    ActionType Type,
    string Description,
    ActionResult Result,
    TimeSpan Duration,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// Represents different action types
/// </summary>
public enum ActionType
{
    Navigation,
    Combat,
    ModuleActivation,
    Targeting,
    DroneControl,
    Inventory,
    Trading,
    UIInteraction,
    StateTransition,
    Other
}

/// <summary>
/// Represents the result of an action
/// </summary>
public enum ActionResult
{
    Success,
    Failure,
    Partial,
    Skipped,
    Cancelled
}

/// <summary>
/// Represents a decision made by the bot
/// </summary>
public record DecisionRecord(
    Guid Id,
    Guid SessionId,
    DateTimeOffset Timestamp,
    string DecisionPoint,
    string ChosenOption,
    IReadOnlyList<string> AvailableOptions,
    Dictionary<string, double> OptionScores,
    string Reasoning,
    Dictionary<string, object>? Context = null
);

/// <summary>
/// Represents analysis of recorded actions
/// </summary>
public record ActionAnalysis(
    Guid SessionId,
    TimeSpan TotalDuration,
    int TotalActions,
    Dictionary<ActionType, int> ActionCounts,
    Dictionary<ActionType, TimeSpan> ActionDurations,
    double SuccessRate,
    IReadOnlyList<string> Insights,
    IReadOnlyList<string> Recommendations
);
