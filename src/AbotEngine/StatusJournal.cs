namespace AbotEngine;

/// <summary>One meaningful strategy/status transition retained for the admin page.</summary>
public sealed record AgentStatusEvent
{
    public long Sequence { get; init; }
    public long AtUnixMs { get; init; }
    public int Pid { get; init; }
    public string Role { get; init; } = "";
    public string Strategy { get; init; } = "";
    public string State { get; init; } = "";
    public string Summary { get; init; } = "";
    public string Action { get; init; } = "";
}

public sealed record AdminStatusOverview(
    string State,
    string Summary,
    int Agents,
    int Acting,
    int Warnings,
    int Errors);

/// <summary>
/// Thread-safe in-memory ring buffer shared by the web dashboard and runner loop. It records only
/// meaningful changes (state/summary/action), so a 1.5-second bot tick does not flood the admin page.
/// </summary>
public sealed class StatusJournal
{
    private readonly object gate = new();
    private readonly int capacity;
    private readonly Queue<AgentStatusEvent> events = new();
    private readonly Dictionary<int, string> lastSignatureByPid = new();
    private long sequence;

    public StatusJournal(int capacity = 120) => this.capacity = Math.Max(10, capacity);

    public void Observe(IEnumerable<AgentSnapshot> snapshots)
    {
        lock (gate)
        {
            foreach (var snapshot in snapshots)
            {
                var status = snapshot.StrategyStatus;
                var state = EffectiveState(snapshot);
                var summary = EffectiveSummary(snapshot);
                var action = status.Action ?? "";
                var signature = $"{state}\u001f{summary}\u001f{action}";
                if (lastSignatureByPid.TryGetValue(snapshot.Pid, out var previous) && previous == signature)
                    continue;
                lastSignatureByPid[snapshot.Pid] = signature;

                events.Enqueue(new AgentStatusEvent
                {
                    Sequence = ++sequence,
                    AtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Pid = snapshot.Pid,
                    Role = snapshot.Role,
                    Strategy = status.Strategy,
                    State = state,
                    Summary = summary,
                    Action = action,
                });
                while (events.Count > capacity)
                    events.Dequeue();
            }
        }
    }

    public AgentStatusEvent[] Recent(int count = 40)
    {
        lock (gate)
            return events.Reverse().Take(Math.Clamp(count, 1, capacity)).ToArray();
    }

    public static AdminStatusOverview Overview(IReadOnlyList<AgentSnapshot> snapshots)
    {
        var states = snapshots.Select(EffectiveState).ToArray();
        var errors = states.Count(s => s == "Error");
        var warnings = states.Count(s => s == "Warning");
        var acting = states.Count(s => s == "Acting");
        var state = errors > 0 ? "Error" : warnings > 0 ? "Warning" : acting > 0 ? "Acting" : "OK";
        var summary = snapshots.Count == 0
            ? "No agents connected"
            : errors > 0 ? $"{errors} agent(s) need intervention"
            : warnings > 0 ? $"{warnings} warning(s)"
            : acting > 0 ? $"{acting}/{snapshots.Count} agent(s) acting"
            : "All agents stable";
        return new AdminStatusOverview(state, summary, snapshots.Count, acting, warnings, errors);
    }

    private static string EffectiveState(AgentSnapshot snapshot)
    {
        if (!snapshot.ParseOk || snapshot.LastError is not null || snapshot.MotionProblems.Length > 0)
            return "Error";
        if (!snapshot.ReadyOk || string.Equals(snapshot.StrategyStatus.State, "Warning", StringComparison.OrdinalIgnoreCase))
            return "Warning";
        return snapshot.StrategyStatus.State;
    }

    private static string EffectiveSummary(AgentSnapshot snapshot)
    {
        if (!snapshot.ParseOk) return snapshot.LastError ?? "UI read/parse failed";
        if (snapshot.LastError is not null) return snapshot.LastError;
        if (snapshot.MotionProblems.Length > 0) return snapshot.MotionProblems[0];
        return string.IsNullOrWhiteSpace(snapshot.StrategyStatus.Summary)
            ? "No strategy summary"
            : snapshot.StrategyStatus.Summary;
    }
}
