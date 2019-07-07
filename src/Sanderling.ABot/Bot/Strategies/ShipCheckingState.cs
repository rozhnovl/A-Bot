using System.Linq;
using BotEngine.Motor;
using Sanderling.ABot.Bot.Task;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Strategies
{
	class ShipCheckingState : IStragegyState
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
				return new BotTask()
				{
					ClientActions = new[]
					{
						bot.MemoryMeasurementAtTime?.Value?.Neocom?.FittingButton?.MouseClick(
							MouseButtonIdEnum
								.Left)
					}
				};
			return null;
		}

		public bool MoveToNext => task?.ShipNameFound ?? false;
	}
}