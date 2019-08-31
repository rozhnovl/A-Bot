using System.Linq;
using BotEngine.Motor;
using Sanderling.ABot.Bot.Task;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class SetDestinationState : IStragegyState
	{
		private SetDestinationTask task;
		private readonly int currentDestinationId;
		private readonly string[] enemySystems;
		private SetDestinationTask.SetDestinationTaskResult? result;
		public SetDestinationState(int currentDestinationId, string[] enemySystems)
		{
			this.currentDestinationId = currentDestinationId;
			this.enemySystems = enemySystems;
		}

		public IBotTask GetStateActions(Bot bot)
		{
			var hasRoute =
				(bot.MemoryMeasurementAtTime.Value?.InfoPanelRoute?.HeaderText?.Contains("Current Destination") ??
				 false)
				|| (bot.MemoryMeasurementAtTime.Value?.InfoPanelRoute?.HeaderText?.Contains("Jump") ?? false);
			if (hasRoute)
			{
				result = SetDestinationTask.SetDestinationTaskResult.RouteSet;
				return null;
			}

			var openFoldersTask = new OpenBookmarksFolderTask(bot.MemoryMeasurementAtTime.Value,
				new[] {"Corporation Locations", "FW Route"});

			if (openFoldersTask.ClientActions.Any())
				return new OpenBookmarksFolderTask(bot.MemoryMeasurementAtTime.Value,
					new[] {"Corporation Locations", "FW Route"});

			task = new SetDestinationTask(bot, currentDestinationId, bot.MemoryMeasurementAtTime?.Value, enemySystems);
			return task;
		}

		public IBotTask GetStateExitActions(Bot bot)
		{
			var fittingWindow = bot.MemoryMeasurementAtTime?.Value?.WindowPeopleAndPlaces?.FirstOrDefault();
			if (fittingWindow != null)
				return bot.MemoryMeasurementAtTime?.Value?.Neocom?.PeopleAndPlacesButton?.ClickTask();
			return null;
		}

		public SetDestinationTask.SetDestinationTaskResult? Result => result ?? task?.Result;
		public bool MoveToNext => (result ?? task?.Result) != null;
	}
}