using AbyssalBot.Domain.Interfaces.Infrastructure;
using Serilog;
using Serilog.Context;

namespace AbyssalBot.Infrastructure.Logging;

/// <summary>
/// Implementation of IBotLogger using Serilog
/// </summary>
public class SerilogBotLogger : IBotLogger
{
    private readonly ILogger _logger;

    public SerilogBotLogger(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogDebug(string message, LogContext? context = null)
    {
        using var _ = CreateContext(context);
        _logger.Debug(message);
    }

    public void LogInformation(string message, LogContext? context = null)
    {
        using var _ = CreateContext(context);
        _logger.Information(message);
    }

    public void LogWarning(string message, LogContext? context = null)
    {
        using var _ = CreateContext(context);
        _logger.Warning(message);
    }

    public void LogError(string message, Exception? exception = null, LogContext? context = null)
    {
        using var _ = CreateContext(context);
        if (exception != null)
        {
            _logger.Error(exception, message);
        }
        else
        {
            _logger.Error(message);
        }
    }

    public void LogCritical(string message, Exception? exception = null, LogContext? context = null)
    {
        using var _ = CreateContext(context);
        if (exception != null)
        {
            _logger.Fatal(exception, message);
        }
        else
        {
            _logger.Fatal(message);
        }
    }

    public void LogCombatEvent(string eventType, Dictionary<string, object> details)
    {
        _logger.Information("Combat Event: {EventType} {@Details}", eventType, details);
    }

    public void LogNavigationEvent(string eventType, Dictionary<string, object> details)
    {
        _logger.Information("Navigation Event: {EventType} {@Details}", eventType, details);
    }

    public void LogStateTransition(string fromState, string toState, string? reason = null)
    {
        if (!string.IsNullOrEmpty(reason))
        {
            _logger.Information("State Transition: {FromState} -> {ToState} (Reason: {Reason})",
                fromState, toState, reason);
        }
        else
        {
            _logger.Information("State Transition: {FromState} -> {ToState}", fromState, toState);
        }
    }

    public void LogMetric(string metricName, double value, string? unit = null)
    {
        if (!string.IsNullOrEmpty(unit))
        {
            _logger.Information("Metric: {MetricName} = {Value} {Unit}", metricName, value, unit);
        }
        else
        {
            _logger.Information("Metric: {MetricName} = {Value}", metricName, value);
        }
    }

    public IDisposable BeginScope(string scopeName)
    {
        return LogContext.PushProperty("Scope", scopeName);
    }

    public IDisposable BeginScope(string scopeName, Dictionary<string, object> properties)
    {
        var disposables = new List<IDisposable>
        {
            LogContext.PushProperty("Scope", scopeName)
        };

        disposables.AddRange(
            properties.Select(kvp => LogContext.PushProperty(kvp.Key, kvp.Value))
        );

        return new CompositeDisposable(disposables);
    }

    private IDisposable? CreateContext(LogContext? context)
    {
        if (context == null)
        {
            return null;
        }

        var disposables = new List<IDisposable>();

        if (context.SessionId != null)
        {
            disposables.Add(LogContext.PushProperty("SessionId", context.SessionId));
        }

        if (context.CharacterName != null)
        {
            disposables.Add(LogContext.PushProperty("CharacterName", context.CharacterName));
        }

        if (context.CurrentState != null)
        {
            disposables.Add(LogContext.PushProperty("CurrentState", context.CurrentState));
        }

        if (context.Location != null)
        {
            disposables.Add(LogContext.PushProperty("Location", context.Location));
        }

        if (context.AdditionalProperties != null)
        {
            foreach (var prop in context.AdditionalProperties)
            {
                disposables.Add(LogContext.PushProperty(prop.Key, prop.Value));
            }
        }

        return disposables.Count > 0 ? new CompositeDisposable(disposables) : null;
    }

    private class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables;

        public CompositeDisposable(List<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
