namespace AbotEngine.Fleet;

/// <summary>Dependency-free checks for the high-level activity selector.</summary>
public static class FleetActivitySupervisorScenarios
{
    public static void Run(Action<string> log)
    {
        PrefersAnomalies(log);
        FallsBackToBelts(log);
        UnknownDoesNotMeanEmpty(log);
        AbyssRequiresEveryGate(log);
        CommittedWorkIsAtomic(log);
        SafetyAndFailureCircuitHoldImmediately(log);
        FlappingCandidateCannotSwitch(log);
        log("Activity supervisor scenarios: PASS");
    }

    private static void PrefersAnomalies(Action<string> log)
    {
        var supervisor = TestSupervisor();
        var decision = Stabilize(supervisor, Signals(
            ActivityAvailability.Available, ActivityAvailability.Available));
        Check(decision.Activity == FleetWorkKind.Anomalies, "anomalies must outrank belts");
        log("Activity supervisor: anomalies > belts");
    }

    private static void FallsBackToBelts(Action<string> log)
    {
        var supervisor = TestSupervisor();
        var decision = Stabilize(supervisor, Signals(
            ActivityAvailability.Exhausted, ActivityAvailability.Available));
        Check(decision.Activity == FleetWorkKind.Belts, "exhausted anomalies must fall back to belts");
        log("Activity supervisor: exhausted anomalies -> belts");
    }

    private static void UnknownDoesNotMeanEmpty(Action<string> log)
    {
        var supervisor = TestSupervisor();
        var decision = Stabilize(supervisor, Signals(
            ActivityAvailability.Unknown, ActivityAvailability.Available));
        Check(decision.Activity == FleetWorkKind.Hold &&
              decision.Blockers.Contains("anomaly availability unknown"),
            "unknown anomaly state must hold instead of falling through");
        log("Activity supervisor: unknown availability -> HOLD");
    }

    private static void AbyssRequiresEveryGate(Action<string> log)
    {
        var disabled = TestSupervisor();
        var blocked = Stabilize(disabled, Signals(
            ActivityAvailability.Exhausted, ActivityAvailability.Exhausted,
            local: 1, abyss: AbyssReadiness.Disabled));
        Check(blocked.Activity == FleetWorkKind.Hold && blocked.Blockers.Count > 0,
            "low local alone must never authorize Abyss");

        var enabled = TestSupervisor();
        var ready = new AbyssReadiness(true, true, true, true, true, true, true, true, true);
        var selected = Stabilize(enabled, Signals(
            ActivityAvailability.Exhausted, ActivityAvailability.Exhausted,
            local: 1, abyss: ready));
        Check(selected.Activity == FleetWorkKind.Abyss, "complete preflight should authorize Abyss");
        log("Activity supervisor: Abyss requires local + full preflight");
    }

    private static void CommittedWorkIsAtomic(Action<string> log)
    {
        var supervisor = TestSupervisor();
        Stabilize(supervisor, Signals(ActivityAvailability.Available, ActivityAvailability.Available));
        var decision = supervisor.Observe(Signals(
            ActivityAvailability.Exhausted, ActivityAvailability.Available) with
            { CurrentActivityCommitted = true }, T(10));
        Check(decision.Activity == FleetWorkKind.Anomalies,
            "committed activity must survive transient/terminal availability changes");
        log("Activity supervisor: committed warp/site is atomic");
    }

    private static void SafetyAndFailureCircuitHoldImmediately(Action<string> log)
    {
        var unsafeSupervisor = TestSupervisor();
        Stabilize(unsafeSupervisor, Signals(ActivityAvailability.Available, ActivityAvailability.Available));
        var unsafeDecision = unsafeSupervisor.Observe(
            Signals(ActivityAvailability.Available, ActivityAvailability.Available) with { CriticalDamage = true },
            T(10));
        Check(unsafeDecision.Activity == FleetWorkKind.Hold && unsafeDecision.Switched,
            "critical damage must hold immediately");

        var failedSupervisor = TestSupervisor();
        Stabilize(failedSupervisor, Signals(ActivityAvailability.Available, ActivityAvailability.Available));
        var failedDecision = failedSupervisor.Observe(
            Signals(ActivityAvailability.Available, ActivityAvailability.Available) with
            { ConsecutiveActivityFailures = 3 }, T(10));
        Check(failedDecision.Activity == FleetWorkKind.Hold &&
              failedDecision.Blockers.Any(b => b.Contains("circuit", StringComparison.OrdinalIgnoreCase)),
            "three failures must open the activity circuit");
        log("Activity supervisor: safety/failure circuit -> immediate HOLD");
    }

    private static void FlappingCandidateCannotSwitch(Action<string> log)
    {
        var supervisor = TestSupervisor();
        supervisor.Observe(Signals(ActivityAvailability.Available, ActivityAvailability.Available), T(1));
        supervisor.Observe(Signals(ActivityAvailability.Exhausted, ActivityAvailability.Available), T(2));
        var decision = supervisor.Observe(
            Signals(ActivityAvailability.Available, ActivityAvailability.Available), T(3));
        Check(decision.Activity == FleetWorkKind.Hold && decision.Candidate == FleetWorkKind.Anomalies,
            "flapping observations must restart stabilization instead of switching");
        log("Activity supervisor: hysteresis rejects flapping signals");
    }

    private static FleetActivitySupervisor TestSupervisor() =>
        new(requiredStableObservations: 3, minimumSwitchInterval: TimeSpan.Zero);

    private static FleetActivityDecision Stabilize(
        FleetActivitySupervisor supervisor,
        FleetActivitySignals signals)
    {
        FleetActivityDecision? decision = null;
        for (var observation = 1; observation <= 3; observation++)
            decision = supervisor.Observe(signals, T(observation));
        return decision!;
    }

    private static FleetActivitySignals Signals(
        ActivityAvailability anomalies,
        ActivityAvailability belts,
        int? local = null,
        AbyssReadiness? abyss = null) => new(
            CompositionValid: true,
            AllClientsReadable: true,
            EmergencyStop: false,
            CriticalDamage: false,
            CurrentActivityCommitted: false,
            Anomalies: anomalies,
            Belts: belts,
            LocalPilots: local,
            MaxLocalPilotsForAbyss: 3,
            Abyss: abyss ?? AbyssReadiness.Disabled);

    private static DateTimeOffset T(int seconds) => DateTimeOffset.UnixEpoch.AddSeconds(seconds);

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
