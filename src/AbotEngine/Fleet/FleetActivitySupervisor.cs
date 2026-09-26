namespace AbotEngine.Fleet;

/// <summary>Three-state observation: absence must be positively established before falling through.</summary>
public enum ActivityAvailability
{
    Unknown,
    Available,
    Exhausted,
}

public enum FleetWorkKind
{
    Hold,
    Anomalies,
    Belts,
    Abyss,
}

/// <summary>
/// Abyss is never selected from local population alone. Every preflight item must be positively known.
/// </summary>
public sealed record AbyssReadiness(
    bool ExplicitlyEnabled,
    bool ActuatorCalibrated,
    bool FleetAtFullHealth,
    bool CapacitorReady,
    bool FilamentsReady,
    bool CargoReady,
    bool LocalRosterStable,
    bool NoHostileOrSuspectFlags,
    bool SessionTimeReady)
{
    public static AbyssReadiness Disabled { get; } = new(
        false, false, false, false, false, false, false, false, false);

    public IReadOnlyList<string> Missing()
    {
        var missing = new List<string>();
        if (!ExplicitlyEnabled) missing.Add("Abyss not explicitly enabled");
        if (!ActuatorCalibrated) missing.Add("Abyss actuator not calibrated");
        if (!FleetAtFullHealth) missing.Add("fleet not at full health");
        if (!CapacitorReady) missing.Add("capacitor not ready");
        if (!FilamentsReady) missing.Add("filaments not confirmed");
        if (!CargoReady) missing.Add("cargo/preflight not confirmed");
        if (!LocalRosterStable) missing.Add("local roster not stable");
        if (!NoHostileOrSuspectFlags) missing.Add("hostile/suspect safety unknown");
        if (!SessionTimeReady) missing.Add("safe session time not confirmed");
        return missing;
    }
}

/// <summary>One complete, immutable supervisor observation.</summary>
public sealed record FleetActivitySignals(
    bool CompositionValid,
    bool AllClientsReadable,
    bool EmergencyStop,
    bool CriticalDamage,
    bool CurrentActivityCommitted,
    ActivityAvailability Anomalies,
    ActivityAvailability Belts,
    int? LocalPilots,
    int MaxLocalPilotsForAbyss,
    AbyssReadiness Abyss,
    int ConsecutiveActivityFailures = 0);

public sealed record FleetActivityDecision(
    FleetWorkKind Activity,
    FleetWorkKind Candidate,
    bool Switched,
    string Summary,
    IReadOnlyList<string> Blockers);

/// <summary>
/// High-level work selector. It is intentionally independent of UI actuation and room tactics:
/// strategies report authoritative availability/safety signals, and this layer selects which one
/// may run. Unknown never means empty, committed work is not interrupted, unstable candidates are
/// held for several observations, and repeated failures open a circuit breaker.
/// </summary>
public sealed class FleetActivitySupervisor
{
    private readonly int requiredStableObservations;
    private readonly int failureLimit;
    private readonly TimeSpan minimumSwitchInterval;
    private FleetWorkKind selected = FleetWorkKind.Hold;
    private FleetWorkKind pending = FleetWorkKind.Hold;
    private int pendingObservations;
    private DateTimeOffset lastSwitchAt = DateTimeOffset.MinValue;

    public FleetActivitySupervisor(
        int requiredStableObservations = 3,
        int failureLimit = 3,
        TimeSpan? minimumSwitchInterval = null)
    {
        if (requiredStableObservations < 1) throw new ArgumentOutOfRangeException(nameof(requiredStableObservations));
        if (failureLimit < 1) throw new ArgumentOutOfRangeException(nameof(failureLimit));
        this.requiredStableObservations = requiredStableObservations;
        this.failureLimit = failureLimit;
        this.minimumSwitchInterval = minimumSwitchInterval ?? TimeSpan.FromSeconds(15);
    }

    public FleetWorkKind Selected => selected;

    public FleetActivityDecision Observe(FleetActivitySignals signals, DateTimeOffset now)
    {
        var immediateBlockers = ImmediateBlockers(signals);
        if (immediateBlockers.Count > 0)
        {
            var switched = selected != FleetWorkKind.Hold;
            selected = FleetWorkKind.Hold;
            ResetPending();
            if (switched) lastSwitchAt = now;
            return new(FleetWorkKind.Hold, FleetWorkKind.Hold, switched,
                $"HOLD: {string.Join("; ", immediateBlockers)}", immediateBlockers);
        }

        // A site/warp/combat commitment is atomic. Availability can disappear because a menu closed
        // or a site despawned; neither is permission to abandon ships in transit or mid-fight.
        if (signals.CurrentActivityCommitted && selected != FleetWorkKind.Hold)
        {
            ResetPending();
            return new(selected, selected, false, $"continue committed {selected}", Array.Empty<string>());
        }

        var (candidate, blockers) = ChooseCandidate(signals);
        if (candidate == selected)
        {
            ResetPending();
            return new(selected, candidate, false, $"continue {selected}", blockers);
        }

        if (pending != candidate)
        {
            pending = candidate;
            pendingObservations = 1;
        }
        else
        {
            pendingObservations++;
        }

        var cooldownRemaining = minimumSwitchInterval - (now - lastSwitchAt);
        if (pendingObservations < requiredStableObservations || cooldownRemaining > TimeSpan.Zero)
        {
            var reason = pendingObservations < requiredStableObservations
                ? $"candidate {candidate} stable {pendingObservations}/{requiredStableObservations}"
                : $"switch cooldown {Math.Ceiling(cooldownRemaining.TotalSeconds)}s";
            return new(FleetWorkKind.Hold, candidate, false, $"HOLD: {reason}", blockers);
        }

        selected = candidate;
        lastSwitchAt = now;
        ResetPending();
        return new(selected, selected, true, $"switch -> {selected}", blockers);
    }

    private List<string> ImmediateBlockers(FleetActivitySignals signals)
    {
        var blockers = new List<string>();
        if (signals.EmergencyStop) blockers.Add("emergency stop");
        if (!signals.CompositionValid) blockers.Add("invalid fleet composition");
        if (!signals.AllClientsReadable) blockers.Add("one or more clients unreadable");
        if (signals.CriticalDamage) blockers.Add("critical fleet damage");
        if (signals.ConsecutiveActivityFailures >= failureLimit)
            blockers.Add($"activity circuit open after {signals.ConsecutiveActivityFailures} failures");
        return blockers;
    }

    private static (FleetWorkKind candidate, IReadOnlyList<string> blockers) ChooseCandidate(
        FleetActivitySignals signals)
    {
        if (signals.Anomalies == ActivityAvailability.Available)
            return (FleetWorkKind.Anomalies, Array.Empty<string>());
        if (signals.Anomalies == ActivityAvailability.Unknown)
            return (FleetWorkKind.Hold, new[] { "anomaly availability unknown" });

        if (signals.Belts == ActivityAvailability.Available)
            return (FleetWorkKind.Belts, Array.Empty<string>());
        if (signals.Belts == ActivityAvailability.Unknown)
            return (FleetWorkKind.Hold, new[] { "belt availability unknown" });

        var abyssBlockers = signals.Abyss.Missing().ToList();
        if (signals.LocalPilots is null)
            abyssBlockers.Add("local population unknown");
        else if (signals.LocalPilots > signals.MaxLocalPilotsForAbyss)
            abyssBlockers.Add($"local population {signals.LocalPilots} > {signals.MaxLocalPilotsForAbyss}");

        return abyssBlockers.Count == 0
            ? (FleetWorkKind.Abyss, Array.Empty<string>())
            : (FleetWorkKind.Hold, abyssBlockers);
    }

    private void ResetPending()
    {
        pending = selected;
        pendingObservations = 0;
    }
}
