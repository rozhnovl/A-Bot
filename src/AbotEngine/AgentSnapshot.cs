namespace AbotEngine;

/// <summary>One readiness-check outcome, surfaced to observers/dashboards.</summary>
public sealed record ReadyItem(string Name, bool Ok, string Detail);

/// <summary>Stable strategy summary rendered by both the single-client and fleet admin pages.</summary>
public sealed record AgentStrategyStatus
{
    public string Strategy { get; init; } = "";
    public string Stage { get; init; } = "";
    public string State { get; init; } = "Starting";
    public string Summary { get; init; } = "";
    public string Action { get; init; } = "";
    public string[] Details { get; init; } = Array.Empty<string>();
    public long UpdatedAtUnixMs { get; init; }
}

/// <summary>One raw NPC tank layer and the fraction of EM/thermal damage that passes its resists.</summary>
public sealed record NpcDamageLayer
{
    public double Hitpoints { get; init; }
    public double EmResonance { get; init; } = 1;
    public double ThermalResonance { get; init; } = 1;
    /// <summary>Observed remaining layer percentage; 100 until target bars are available.</summary>
    public double RemainingPct { get; init; } = 100;
}

/// <summary>Raw shield, armor and hull data used for mixed-damage volley calculation.</summary>
public sealed record NpcDefenseProfile
{
    public NpcDamageLayer Shield { get; init; } = new();
    public NpcDamageLayer Armor { get; init; } = new();
    public NpcDamageLayer Hull { get; init; } = new();
    public bool RemainingObserved { get; init; }
    public bool IsKnown => Shield.Hitpoints > 0 || Armor.Hitpoints > 0 || Hull.Hitpoints > 0;
}

/// <summary>Compact overview entity used by a fleet doctrine; no UI tree leaves the client agent.</summary>
public sealed record AgentOverviewEntity
{
    public long Id { get; init; }
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public int DistanceMeters { get; init; }
    public bool IsEnemy { get; init; }
    public bool TargetingMe { get; init; }
    public bool AttackingMe { get; init; }
    public bool TargetedByMe { get; init; }
    public long EstimatedEhp { get; init; }
    public NpcDefenseProfile Defense { get; init; } = new();
    public double ApproxDps { get; init; }
    public string[] Ewar { get; init; } = Array.Empty<string>();
}

/// <summary>
/// A compact, serializable snapshot of one client agent's state, produced every step.
/// Small on purpose — safe to ship to a dashboard, a fleet view, or a log line, and
/// (unlike the full UI tree) cheap to move between processes/machines if ever needed.
/// </summary>
public sealed record AgentSnapshot
{
    public long StepIndex { get; init; }
    public string Mode { get; init; } = "dry-run";
    public string Profile { get; init; } = "";
    public string Role { get; init; } = "";
    public int Pid { get; init; }
    public string Title { get; init; } = "";

    public bool ParseOk { get; init; }
    public bool ReadyOk { get; init; }
    public ReadyItem[] Readiness { get; init; } = Array.Empty<ReadyItem>();

    public long UpdatedAtMs { get; init; }
    public long LastGoodParseMs { get; init; }
    public double UptimeSec { get; init; }

    public string? System { get; init; }
    public int? Armor { get; init; }
    public int? Shield { get; init; }
    public int? Struct { get; init; }
    public int? Capacitor { get; init; }
    public int IncomingDps { get; init; }
    public int Attackers { get; init; }
    public int TargetsLocked { get; init; }
    /// <summary>Loaded charge in the grouped Small Focused Beam Laser II, when the UI exposes it.</summary>
    public int? LaserChargeTypeId { get; init; }
    public int OverviewEntries { get; init; }
    public bool InSpace { get; init; }
    public string? Maneuver { get; init; }
    public AgentOverviewEntity[] Overview { get; init; } = Array.Empty<AgentOverviewEntity>();

    public AgentStrategyStatus StrategyStatus { get; init; } = new();
    public string[] Intents { get; init; } = Array.Empty<string>();
    public int MotionCount { get; init; }
    public bool Executed { get; init; }
    public string? LastError { get; init; }

    /// <summary>Per-motion execution problems this step (empty when all fine).</summary>
    public string[] MotionProblems { get; init; } = Array.Empty<string>();
}
