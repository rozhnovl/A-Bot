namespace AbotEngine;

/// <summary>Dependency-free smoke checks for the status aggregation used by both dashboards.</summary>
public static class StatusJournalScenarios
{
    public static void Run(Action<string>? log = null)
    {
        var journal = new StatusJournal(10);
        var acting = Snapshot(101, "Acting", "Locking primary", "Lock Starving Damavik");

        journal.Observe(new[] { acting });
        journal.Observe(new[] { acting });
        Require(journal.Recent().Length == 1, "unchanged ticks must be de-duplicated");

        var waiting = Snapshot(101, "Waiting", "Holding at the gate", "Wait for fleet ready");
        journal.Observe(new[] { waiting });
        var events = journal.Recent();
        Require(events.Length == 2, "a meaningful strategy transition must be retained");
        Require(events[0].State == "Waiting", "history must be newest-first");

        var overview = StatusJournal.Overview(new[]
        {
            waiting,
            Snapshot(102, "Warning", "Deacon capacitor low", "Stop MWD"),
        });
        Require(overview.State == "Warning" && overview.Warnings == 1,
            "strategy warnings must reach fleet health");

        overview = StatusJournal.Overview(new[]
        {
            waiting,
            Snapshot(103, "Acting", "Firing", "Apply DPS") with
            {
                ParseOk = false,
                LastError = "UI parse failed",
            },
        });
        Require(overview.State == "Error" && overview.Errors == 1,
            "agent failures must override normal strategy state");

        log?.Invoke("Status journal scenarios: PASS");
    }

    private static AgentSnapshot Snapshot(int pid, string state, string summary, string action) => new()
    {
        Pid = pid,
        Role = pid == 101 ? "tank-retri" : "wing-retri",
        ParseOk = true,
        ReadyOk = true,
        StrategyStatus = new AgentStrategyStatus
        {
            Strategy = "ScenarioStrategy",
            Stage = "room-1",
            State = state,
            Summary = summary,
            Action = action,
        },
    };

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException($"Status journal scenario failed: {message}");
    }
}
