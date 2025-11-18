using System.Collections.Generic;

namespace Sanderling.ABot.Bot.Configuration;

/// <summary>
/// Configuration for ship fits and module keybindings
/// </summary>
public record ShipFitConfiguration
{
	public Dictionary<string, ShipConfiguration> Ships { get; init; } = new();
}

public record ShipConfiguration
{
	public string ShipName { get; init; } = string.Empty;
	public int MaxTargetingRange { get; init; }
	public int MaxTargets { get; init; }
	public int MaxDronesInSpace { get; init; }
	public List<ModuleSlotConfiguration> HighSlots { get; init; } = new();
	public List<ModuleSlotConfiguration> MidSlots { get; init; } = new();
	public List<ModuleSlotConfiguration> LowSlots { get; init; } = new();
}

public record ModuleSlotConfiguration
{
	/// <summary>
	/// The type of module: Hardener, Weapon, ShieldBooster, MWD, Etc
	/// </summary>
	public string ModuleType { get; init; } = "Etc";

	/// <summary>
	/// Hotkey for this module (e.g., "F1", "F2", "CONTROL+F1")
	/// </summary>
	public string? Hotkey { get; init; }

	/// <summary>
	/// Optimal range for this module in meters
	/// </summary>
	public int OptimalRange { get; init; } = 4000;
}
