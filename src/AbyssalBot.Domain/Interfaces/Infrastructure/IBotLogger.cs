namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides structured logging for bot operations
/// </summary>
public interface IBotLogger
{
    /// <summary>
    /// Logs a debug message
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="context">Additional context</param>
    void LogDebug(string message, LogContext? context = null);

    /// <summary>
    /// Logs an informational message
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="context">Additional context</param>
    void LogInformation(string message, LogContext? context = null);

    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="context">Additional context</param>
    void LogWarning(string message, LogContext? context = null);

    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="exception">Optional exception</param>
    /// <param name="context">Additional context</param>
    void LogError(string message, Exception? exception = null, LogContext? context = null);

    /// <summary>
    /// Logs a critical error message
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="exception">Optional exception</param>
    /// <param name="context">Additional context</param>
    void LogCritical(string message, Exception? exception = null, LogContext? context = null);

    /// <summary>
    /// Logs a combat event
    /// </summary>
    /// <param name="eventType">Type of combat event</param>
    /// <param name="details">Event details</param>
    void LogCombatEvent(string eventType, Dictionary<string, object> details);

    /// <summary>
    /// Logs a navigation event
    /// </summary>
    /// <param name="eventType">Type of navigation event</param>
    /// <param name="details">Event details</param>
    void LogNavigationEvent(string eventType, Dictionary<string, object> details);

    /// <summary>
    /// Logs a bot state transition
    /// </summary>
    /// <param name="fromState">Previous state</param>
    /// <param name="toState">New state</param>
    /// <param name="reason">Reason for transition</param>
    void LogStateTransition(string fromState, string toState, string? reason = null);

    /// <summary>
    /// Logs a performance metric
    /// </summary>
    /// <param name="metricName">Name of the metric</param>
    /// <param name="value">Metric value</param>
    /// <param name="unit">Unit of measurement</param>
    void LogMetric(string metricName, double value, string? unit = null);

    /// <summary>
    /// Creates a scoped logger for a specific operation
    /// </summary>
    /// <param name="scopeName">Name of the scope</param>
    /// <returns>Disposable scope</returns>
    IDisposable BeginScope(string scopeName);

    /// <summary>
    /// Creates a scoped logger with properties
    /// </summary>
    /// <param name="scopeName">Name of the scope</param>
    /// <param name="properties">Scope properties</param>
    /// <returns>Disposable scope</returns>
    IDisposable BeginScope(string scopeName, Dictionary<string, object> properties);
}

/// <summary>
/// Represents additional logging context
/// </summary>
public record LogContext(
    string? SessionId = null,
    string? CharacterName = null,
    string? CurrentState = null,
    string? Location = null,
    Dictionary<string, object>? AdditionalProperties = null
)
{
    /// <summary>
    /// Creates a LogContext from the current bot state
    /// </summary>
    public static LogContext FromCurrentState(
        string sessionId,
        string characterName,
        string currentState,
        string? location = null)
    {
        return new LogContext(
            SessionId: sessionId,
            CharacterName: characterName,
            CurrentState: currentState,
            Location: location
        );
    }

    /// <summary>
    /// Adds additional properties to the context
    /// </summary>
    public LogContext WithProperty(string key, object value)
    {
        var props = AdditionalProperties != null
            ? new Dictionary<string, object>(AdditionalProperties)
            : new Dictionary<string, object>();

        props[key] = value;

        return this with { AdditionalProperties = props };
    }
}

/// <summary>
/// Represents log levels
/// </summary>
public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error,
    Critical
}
