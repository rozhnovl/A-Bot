using System.Linq;
using BotEngine.Motor;
using Sanderling.ABot.Bot.Task;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class ShipCheckingState : IStragegyState
	{
		private CheckShipTask task;

		public IBotTask GetStateActions(Bot bot)
		{
			task = new CheckShipTask(bot.MemoryMeasurementAtTime?.Value, "Puller");
			return task;
		}

		public IBotTask GetStateExitActions(Bot bot)
		{
			var fittingWindow = bot.MemoryMeasurementAtTime?.Value?.WindowShipFitting?.FirstOrDefault();
			if (fittingWindow != null)
				return bot.MemoryMeasurementAtTime?.Value?.Neocom?.FittingButton?.ClickTask();
					
			return null;
		}

		public bool MoveToNext => task?.ShipNameFound ?? false;
	}
}