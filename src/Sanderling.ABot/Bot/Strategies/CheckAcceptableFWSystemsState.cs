using System.Linq;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot.Strategies
{
	class CheckAcceptableFWSystemsState : IStragegyState
	{
		public string[] SystemsToSkip = null;
		public IBotTask GetStateActions(Bot bot)
		{
			var memory = bot?.MemoryMeasurementAtTime?.Value;

			return null;
		}

		public IBotTask GetStateExitActions(Bot bot)
		{
			var fwWindow =
				bot.MemoryMeasurementAtTime?.Value?.WindowOther?.FirstOrDefault(w => w.Caption == "Factional Warfare");
			if (fwWindow != null)
				return fwWindow.ClickMenuEntryByRegexPattern(bot, ".*Close");
			return null;
		}

		public bool MoveToNext => SystemsToSkip!=null;
	}
}