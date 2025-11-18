namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Collects telemetry data including metrics, traces, and events
/// </summary>
public interface ITelemetryCollector
{
    /// <summary>
    /// Records a metric value
    /// </summary>
    /// <param name="name">Metric name</param>
    /// <param name="value">Metric value</param>
    /// <param name="tags">Optional tags</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordMetricAsync(
        string name,
        double value,
        Dictionary<string, string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a counter increment
    /// </summary>
    /// <param name="name">Counter name</param>
    /// <param name="increment">Increment value (default 1)</param>
    /// <param name="tags">Optional tags</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task IncrementCounterAsync(
        string name,
        long increment = 1,
        Dictionary<string, string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a timing/duration measurement
    /// </summary>
    /// <param name="name">Timer name</param>
    /// <param name="duration">Duration to record</param>
    /// <param name="tags">Optional tags</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordTimingAsync(
        string name,
        TimeSpan duration,
        Dictionary<string, string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a trace span for an operation
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="tags">Optional tags</param>
    /// <returns>Disposable trace span</returns>
    ITraceSpan StartTrace(
        string operationName,
        Dictionary<string, string>? tags = null);

    /// <summary>
    /// Records a custom event
    /// </summary>
    /// <param name="eventName">Event name</param>
    /// <param name="properties">Event properties</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordEventAsync(
        string eventName,
        Dictionary<string, object> properties,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a bot decision event
    /// </summary>
    /// <param name="decision">Decision type</param>
    /// <param name="outcome">Outcome of the decision</param>
    /// <param name="details">Additional details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordDecisionAsync(
        string decision,
        string outcome,
        Dictionary<string, object>? details = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an error or exception
    /// </summary>
    /// <param name="exception">Exception to record</param>
    /// <param name="context">Additional context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordExceptionAsync(
        Exception exception,
        Dictionary<string, object>? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Flushes all pending telemetry data
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task FlushAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current telemetry statistics
    /// </summary>
    TelemetryStatistics GetStatistics();
}

/// <summary>
/// Represents a trace span for measuring operation duration
/// </summary>
public interface ITraceSpan : IDisposable
{
    /// <summary>
    /// Adds a tag to the trace span
    /// </summary>
    void AddTag(string key, string value);

    /// <summary>
    /// Adds an event to the trace span
    /// </summary>
    void AddEvent(string eventName, Dictionary<string, object>? attributes = null);

    /// <summary>
    /// Marks the span as having an error
    /// </summary>
    void SetError(Exception exception);

    /// <summary>
    /// Gets the span duration
    /// </summary>
    TimeSpan Duration { get; }
}

/// <summary>
/// Represents telemetry statistics
/// </summary>
public record TelemetryStatistics(
    long TotalMetrics,
    long TotalEvents,
    long TotalTraces,
    long TotalErrors,
    DateTimeOffset LastFlush
);
