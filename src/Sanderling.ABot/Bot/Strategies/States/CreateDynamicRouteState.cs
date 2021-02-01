using System.Collections.Generic;
using System.Linq;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class CreateDynamicRouteState : IStragegyState
	{
		private SetDestinationTask task;

		private List<string> takenMissions = new List<string>();

		public CreateDynamicRouteState()
		{
		}

		public IBotTask GetStateActions(Bot bot)
		{
			return new SetMissionDestinationTask(bot, takenMissions.ToArray());
			return null;

		}

		public IBotTask GetStateExitActions(Bot bot)
		{
			var fittingWindow = bot.MemoryMeasurementAtTime?.Value?.WindowPeopleAndPlaces?.FirstOrDefault();
			if (fittingWindow != null)
				return bot.MemoryMeasurementAtTime?.Value?.Neocom?.PeopleAndPlacesButton.ClickTask();
			return null;
		}

		public SetDestinationTask.SetDestinationTaskResult? Result => task?.Result;
		public bool MoveToNext => task?.Result != null;
	}
}