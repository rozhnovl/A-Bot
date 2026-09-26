namespace Sanderling.ABot.Bot
{
	/// <summary>Small, UI-friendly state shared by every strategy implementation.</summary>
	public enum StrategyRunState
	{
		Starting,
		Waiting,
		Acting,
		Idle,
		Warning,
		Error,
	}

	/// <summary>
	/// Structured status produced centrally after every <see cref="Bot.Step"/>. Strategies continue to
	/// describe their reasoning with <c>DiagnosticTask</c>; the common bot loop turns those diagnostics
	/// and the selected actionable task into this stable contract for logs and dashboards.
	/// </summary>
	public sealed record StrategyStatus
	{
		public string Strategy { get; init; } = "";
		public string Stage { get; init; } = "";
		public StrategyRunState State { get; init; } = StrategyRunState.Starting;
		public string Summary { get; init; } = "";
		public string Action { get; init; } = "";
		public string[] Details { get; init; } = Array.Empty<string>();
	}
}
