using System.Collections.Generic;

namespace Sanderling.ABot.Bot.Configuration;

/// <summary>
/// Configuration for combat behavior including NPC priorities, DPS values, and engagement parameters
/// </summary>
public record CombatConfiguration
{
	/// <summary>
	/// DPS values for different NPC types
	/// </summary>
	public Dictionary<string, double> NpcDpsValues { get; init; } = new();

	/// <summary>
	/// Priority values for targeting different NPCs (lower = higher priority)
	/// </summary>
	public NpcPriorityConfiguration Priorities { get; init; } = new();

	/// <summary>
	/// Combat engagement parameters
	/// </summary>
	public EngagementConfiguration Engagement { get; init; } = new();

	/// <summary>
	/// Orbit beacon identification settings
	/// </summary>
	public OrbitBeaconConfiguration OrbitBeacons { get; init; } = new();
}

public record NpcPriorityConfiguration
{
	public int Guristas { get; init; } = 20;
	public int Anchoring { get; init; } = 1;
	public int Firewatcher { get; init; } = 2;
	public int Renewing { get; init; } = 3;
	public int Plateforger { get; init; } = 3;
	public int Fieldweaver { get; init; } = 3;
	public int Entangler { get; init; } = 6;
	public int Snarecaster { get; init; } = 6;
	public int Scylla { get; init; } = 8;
	public int Tyrannos { get; init; } = 8;
	public int Extraction { get; init; } = 10;
	public int Bioadaptive { get; init; } = 1000;
	public int DrifterBattleship { get; init; } = 9000;
	public int DefaultBasePriority { get; init; } = 800;
}

public record EngagementConfiguration
{
	/// <summary>
	/// Maximum distance to target enemies (in meters)
	/// </summary>
	public int MaxTargetDistance { get; init; } = 55000;

	/// <summary>
	/// DPS threshold for defensive maneuvers (orbit vs approach)
	/// </summary>
	public double DefensiveManeuverDpsThreshold { get; init; } = 200.0;

	/// <summary>
	/// Distance for orbit maneuver (in meters)
	/// </summary>
	public int OrbitDistance { get; init; } = 5000;

	/// <summary>
	/// Distance for keep at range maneuver (in meters)
	/// </summary>
	public int KeepAtRangeDistance { get; init; } = 500;

	/// <summary>
	/// Distance threshold for activating MWD when approaching (in meters)
	/// </summary>
	public int MwdActivationDistance { get; init; } = 2000;

	/// <summary>
	/// Minimum capacitor level for MWD activation
	/// </summary>
	public int MinimumCapacitorForMwd { get; init; } = 200;

	/// <summary>
	/// Default optimal range for modules (in meters)
	/// </summary>
	public int DefaultModuleOptimalRange { get; init; } = 4000;

	/// <summary>
	/// Minimum steps between targeting attempts for the same target
	/// </summary>
	public int MinStepsBetweenTargetingAttempts { get; init; } = 10;
}

public record OrbitBeaconConfiguration
{
	public List<string> BeaconNames { get; init; } = new()
	{
		"Leshak",
		"Overmind",
		"Battleship"
	};
}
