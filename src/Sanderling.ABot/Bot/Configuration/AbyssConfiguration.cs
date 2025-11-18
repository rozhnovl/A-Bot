using System.Collections.Generic;

namespace Sanderling.ABot.Bot.Configuration;

/// <summary>
/// Configuration for Abyssal Deadspace operations
/// </summary>
public record AbyssConfiguration
{
	/// <summary>
	/// Filament type to use for entering the abyss
	/// </summary>
	public string FilamentType { get; init; } = "Raging Exotic Filament";

	/// <summary>
	/// Time to wait after leaving abyss before taking action (in seconds)
	/// </summary>
	public int InvulnerabilityWaitTimeSeconds { get; init; } = 46;

	/// <summary>
	/// Total invulnerability duration after leaving (in seconds)
	/// </summary>
	public int TotalInvulnerabilityDurationSeconds { get; init; } = 72;

	/// <summary>
	/// Whether to use Mobile Tractor Unit for looting
	/// </summary>
	public bool UseTractorForLooting { get; init; } = true;

	/// <summary>
	/// Distance to conduit before deploying tractor (in meters)
	/// </summary>
	public int TractorDeploymentDistance { get; init; } = 2000;

	/// <summary>
	/// Distance to tractor/wreck before opening cargo (in meters)
	/// </summary>
	public int LootingDistance { get; init; } = 2500;

	/// <summary>
	/// NPC types to ignore in combat
	/// </summary>
	public List<string> IgnoredNpcTypes { get; init; } = new()
	{
		"Vila Swarmer",
		"Extraction"
	};

	/// <summary>
	/// Objects that indicate cache/loot containers
	/// </summary>
	public List<string> CacheNames { get; init; } = new()
	{
		"Bioadaptive",
		"Biocombinative"
	};

	/// <summary>
	/// Objects that indicate conduits (gates to next room)
	/// </summary>
	public List<string> ConduitNames { get; init; } = new()
	{
		"Conduit"
	};

	/// <summary>
	/// Objects that should be excluded from conduit search
	/// </summary>
	public List<string> ConduitExclusions { get; init; } = new()
	{
		"Proving"
	};
}
