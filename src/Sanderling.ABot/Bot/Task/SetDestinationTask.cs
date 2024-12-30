using System.Collections.Generic;
using System.Linq;
using BotEngine.Motor;
using Sanderling.Motor;
using Sanderling.Parse;
using IMemoryMeasurement = Sanderling.Parse.IMemoryMeasurement;

namespace Sanderling.ABot.Bot.Task
{
	public class OpenBookmarksFolderTask : IBotTask
	{
		private readonly string[] FoldersToOpen;
		private readonly IMemoryMeasurement MemoryMeasurement;

		public OpenBookmarksFolderTask(IMemoryMeasurement memoryMeasurement, string[] foldersToOpen)
		{
			FoldersToOpen = foldersToOpen;
			MemoryMeasurement = memoryMeasurement;
		}

		public IEnumerable<IBotTask> Component { get; }

		public IEnumerable<MotionRecommendation> ClientActions
		{
			get
			{
				bool groupFound = false;
				if (MemoryMeasurement?.WindowPeopleAndPlaces?.FirstOrDefault() == null)
					yield return MemoryMeasurement?.Neocom?.PeopleAndPlacesButton
						.MouseClick(MouseButtonIdEnum.Left)
						.AsRecommendation();

				if (MemoryMeasurement?.WindowPeopleAndPlaces
					    ?.FirstOrDefault()
					    ?.LabelText
					    ?.FirstOrDefault(text => text.Text.StartsWith("Corporation Locations")) == null)
				{
					var places = MemoryMeasurement?.WindowPeopleAndPlaces
						?.FirstOrDefault()
						?.LabelText
						?.FirstOrDefault(text => text.Text == "Places");
					yield return places.MouseClick(MouseButtonIdEnum.Left)
						.AsRecommendation();
				}

				foreach (var folder in FoldersToOpen.Reverse())
				{
					var waypointsFolder = MemoryMeasurement
						?.WindowPeopleAndPlaces
						?.FirstOrDefault()
						?.LabelText
						?.FirstOrDefault(text => text.Text.StartsWith(folder));
					var waypointsGroup = MemoryMeasurement
						?.WindowPeopleAndPlaces
						?.FirstOrDefault()
						?.ListView
						?.Entry
						?.FirstOrDefault(e => e.LabelText.SingleOrDefault().Text.StartsWith(folder));
					if (waypointsGroup != null && !waypointsGroup.IsExpanded.Value)
						yield return waypointsFolder.MouseClick(MouseButtonIdEnum.Left)
							.AsRecommendation();
					if (waypointsGroup != null)
						groupFound = true;
				}

				if (!groupFound)
				{
					var waypointsGroup = MemoryMeasurement
						?.WindowPeopleAndPlaces
						?.FirstOrDefault()
						?.ListView
						?.Entry
						?.FirstOrDefault(e => e.IsExpanded.Value);
					if (waypointsGroup != null && waypointsGroup.IsExpanded.Value)
						yield return waypointsGroup.GroupExpander.MouseClick(MouseButtonIdEnum.Left)
							.AsRecommendation();
				}
			}
		}
	}

	public class SetDestinationTask : IBotTask
	{
		private readonly Bot bot;
		private readonly IMemoryMeasurement MemoryMeasurement;
		private readonly int currentDestinationId;
		private readonly string[] enemySystems;

		public SetDestinationTask(Bot bot, int currentDestinationId, IMemoryMeasurement memoryMeasurement,
			string[] enemySystems)
		{
			this.bot = bot;
			this.currentDestinationId = currentDestinationId;
			MemoryMeasurement = memoryMeasurement;
			this.enemySystems = enemySystems;
		}

		public IEnumerable<IBotTask> Component => null;
		public SetDestinationTaskResult? Result { get; private set; }

		public enum SetDestinationTaskResult
		{
			RouteSet,
			NoSuitableBookmark,
			NextBookmarkIsEnemySystem
		}

		public IEnumerable<MotionRecommendation> ClientActions
		{
			get
			{
				var nextWaypoint = MemoryMeasurement
					?.WindowPeopleAndPlaces
					?.FirstOrDefault()
					?.LabelText
					?.FirstOrDefault(text => text.Text.StartsWith(currentDestinationId + ". "));

				if (nextWaypoint != null)
				{
					if (enemySystems.Any(es => nextWaypoint.Text.Contains(es)))
						Result = SetDestinationTaskResult.NextBookmarkIsEnemySystem;
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
	public class SetMissionDestinationTask : IBotTask
	{
		private readonly Bot bot;
		private readonly string[] processedMissions;
		private readonly IMemoryMeasurement MemoryMeasurement;

		public SetMissionDestinationTask(Bot bot, string[] processedMissions)
		{
			this.bot = bot;
			this.processedMissions = processedMissions;
			MemoryMeasurement = bot.MemoryMeasurementAtTime.Value;
		}

		public IEnumerable<IBotTask> Component => null;
		public SetDestinationTaskResult? Result { get; private set; }

		public enum SetDestinationTaskResult
		{
			RouteSet,
			NoSuitableBookmark,
			NextBookmarkIsEnemySystem
		}
		public IEnumerable<MotionRecommendation> ClientActions
		{
			get
			{
				var activeMissionWindow =
					MemoryMeasurement.WindowOther.SingleOrDefault(w => w.Caption.Contains("Mission journal"));
				yield return activeMissionWindow.Sprite.ToArray()[12].MouseClick(MouseButtonIdEnum.Left)
					.AsRecommendation();
				yield break;
				var missionsWindow = MemoryMeasurement?.WindowOther?.FirstOrDefault(w => w.Caption == "Journal");
				if (missionsWindow == null)
					yield return MemoryMeasurement?.Neocom?.Button.Single(bt => bt.TexturePath.Contains("journal"))
						.MouseClick(MouseButtonIdEnum.Left)
						.AsRecommendation();

				var acceptedMissions = missionsWindow.LabelText.Where(lt => lt.Text.Contains(">Accepted<"));

				var missionToAddDestination =
					acceptedMissions.FirstOrDefault(am => processedMissions.All(pm => !am.Text.Contains(pm)));
				if (missionToAddDestination != null)
				{
					yield return missionToAddDestination.MouseDoubleClick(MouseButtonIdEnum.Left)
						.AsRecommendation();
				}
			}
		}
	}
}