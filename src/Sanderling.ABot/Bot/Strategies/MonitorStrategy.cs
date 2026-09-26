using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot.Strategies
{
	/// <summary>
	/// Read-only client strategy used while a fleet doctrine is being rehearsed. It deliberately emits
	/// no input motions: the fleet brain can be compared with manual flying on anomalies before a live
	/// actuator is allowed to control three clients.
	/// </summary>
	internal sealed class MonitorStrategy : IStrategy
	{
		public IEnumerable<IBotTask> GetTasks(Bot bot)
		{
			yield return new DiagnosticTask("monitor-only: fleet doctrine owns the plan; no client input emitted");
		}
	}
}
