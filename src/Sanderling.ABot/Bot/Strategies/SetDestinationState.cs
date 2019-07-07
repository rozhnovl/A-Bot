using System.Linq;
using BotEngine.Motor;
using Sanderling.ABot.Bot.Task;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Strategies
{
	class SetDestinationState : IStragegyState
	{
		private SetDestinationTask task;

		public IBotTask GetStateActions(Bot bot)
		{
			task = new SetDestinationTask(bot, new[] { "FW Route" }, 1, bot.MemoryMeasurementAtTime?.Value);
			return task;
		}

		public IBotTask GetStateExitActions(Bot bot)
		{
			var fittingWindow = bot.MemoryMeasurementAtTime?.Value?.WindowPeopleAndPlaces?.FirstOrDefault();
			if (fittingWindow != null)
				return new BotTask()
				{
					ClientActions = new[]
					{
						bot.MemoryMeasurementAtTime?.Value?.Neocom?.PeopleAndPlacesButton?.MouseClick(
							MouseButtonIdEnum
								.Left)
					}
				};
			return null;
		}

		public bool MoveToNext => task?.HasRoute ?? false;
	}
}