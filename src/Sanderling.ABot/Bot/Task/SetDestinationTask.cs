using System.Collections.Generic;
using System.Linq;
using BotEngine.Motor;
using Sanderling.Motor;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot.Task
{
	public class SetDestinationTask : IBotTask
	{
		private readonly Bot bot;
		private readonly string[] FoldersToOpen;
		private readonly int currentDestinationId;
		private readonly IMemoryMeasurement MemoryMeasurement;

		public SetDestinationTask(Bot bot, string[] foldersToOpen, int currentDestinationId, IMemoryMeasurement memoryMeasurement)
		{
			this.bot = bot;
			FoldersToOpen = foldersToOpen;
			this.currentDestinationId = currentDestinationId;
			MemoryMeasurement = memoryMeasurement;
		}

		public IEnumerable<IBotTask> Component => null;
		public SetDestinationTaskResult? Result { get; private set; }

		public enum SetDestinationTaskResult
		{
			RouteSet, NoSuitableBookmark,
		}
		public IEnumerable<MotionParam> ClientActions
		{
			get
			{
				//var toogleButton = MemoryMeasurement?.InfoPanelRoute?.ExpandToggleButton;
				//if (MemoryMeasurement?.InfoPanelRoute?.HeaderText!= "No Destination" && toogleButton != null)
				//yield return toogleButton.MouseClick(MouseButtonIdEnum.Left);

				var hasRoute = (MemoryMeasurement?.InfoPanelRoute?.HeaderText?.Contains("Current Destination") ?? false)
				           || (MemoryMeasurement?.InfoPanelRoute?.HeaderText?.Contains("Jump") ?? false);
				if (hasRoute)
				{
					Result = SetDestinationTaskResult.RouteSet;
					yield break;
				}

				if (MemoryMeasurement?.WindowPeopleAndPlaces?.FirstOrDefault() == null)
					yield return MemoryMeasurement?.Neocom?.PeopleAndPlacesButton.MouseClick(MouseButtonIdEnum.Left);

				if (MemoryMeasurement?.WindowPeopleAndPlaces
					    ?.FirstOrDefault()
					    ?.LabelText
					    ?.FirstOrDefault(text => text.Text.StartsWith("Corporation Locations")) == null)
				{
					var places = MemoryMeasurement?.WindowPeopleAndPlaces
						?.FirstOrDefault()
						?.LabelText
						?.FirstOrDefault(text => text.Text == "Places");
					yield return places.MouseClick(MouseButtonIdEnum.Left);
				}

				var waypointsFolder = MemoryMeasurement
					?.WindowPeopleAndPlaces
					?.FirstOrDefault()
					?.LabelText
					?.FirstOrDefault(text => text.Text.StartsWith(FoldersToOpen.Single()));
				var waypointsGroup = MemoryMeasurement
					?.WindowPeopleAndPlaces
					?.FirstOrDefault()
					?.ListView
					?.Entry
					?.FirstOrDefault(e => e.LabelText.SingleOrDefault().Text.StartsWith(FoldersToOpen.Single()));
				if (waypointsGroup != null && !waypointsGroup.IsExpanded.Value)
					yield return waypointsFolder.MouseClick(MouseButtonIdEnum.Left);
				var nextWaypoint = MemoryMeasurement
					?.WindowPeopleAndPlaces
					?.FirstOrDefault()
					?.LabelText
					?.FirstOrDefault(text => text.Text.StartsWith(currentDestinationId + ". "));

				if (nextWaypoint != null)
				{
					foreach (var action in nextWaypoint.ClickMenuEntryByRegexPattern(bot, "Set Destination")
						.ClientActions
					)
					{
						yield return action;
					}
				}
				else
				{
					Result = SetDestinationTaskResult.NoSuitableBookmark;
				}
			}
		}
	}
}