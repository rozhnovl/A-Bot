namespace Sanderling.ABot.Bot.Configuration;

/// <summary>
/// General bot configuration settings
/// </summary>
public record BotConfiguration
{
	/// <summary>
	/// Default ship fit to use
	/// </summary>
	public string DefaultShipFit { get; init; } = "Hawk";

	/// <summary>
	/// Whether to enable diagnostic logging
	/// </summary>
	public bool EnableDiagnostics { get; init; } = true;

	/// <summary>
	/// Whether to enable performance monitoring
	/// </summary>
	public bool EnablePerformanceMonitoring { get; init; } = false;

	/// <summary>
	/// Default delay between actions in milliseconds
	/// </summary>
	public int DefaultActionDelayMs { get; init; } = 100;

	/// <summary>
	/// Whether to automatically refill after leaving abyss
	/// </summary>
	public bool AutoRefillAfterAbyss { get; init; } = false;
}
