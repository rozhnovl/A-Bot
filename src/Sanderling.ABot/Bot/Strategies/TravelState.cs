using System;
using System.Collections.Generic;
using System.Linq;
using Bib3.Geometrik;
using Sanderling.ABot.Bot.Task;
using Sanderling.ABot.Parse;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Strategies
{
	class TravelState : IStragegyState
	{
		private bool arrived;
		private readonly bool bookmarkMissionsOnWay;

		public TravelState(bool bookmarkMissionsOnWay)
		{
			this.bookmarkMissionsOnWay = bookmarkMissionsOnWay;
		}

		public IBotTask GetStateActions(Bot bot)
		{
			var memory = bot.MemoryMeasurementAtTime.Value;
			var ManeuverType = memory?.ShipUi?.Indication?.ManeuverType;

			if (ShipManeuverTypeEnum.Warp == ManeuverType ||
			    ShipManeuverTypeEnum.Jump == ManeuverType)
				return null;  //	do nothing while warping or jumping.

			if (bookmarkMissionsOnWay)
			{
				var bookmarkTask = GetBookmarkTask(bot);
				if (bookmarkTask != null)
					return bookmarkTask;
			}


			//	from the set of route element markers in the Info Panel pick the one that represents the next Waypoint/System.
			//	We assume this is the one which is nearest to the topleft corner of the Screen which is at (0,0)
			var RouteElementMarkerNext =
				memory?.InfoPanelRoute?.RouteElementMarker
					?.OrderByCenterDistanceToPoint(new Vektor2DInt(0, 0))?.FirstOrDefault();
			if (RouteElementMarkerNext != null)
			{
				var undockTask = new UndockTask(bot.MemoryMeasurementAtTime.Value);
				if (undockTask.ClientActions.Any())
					return undockTask;
			}
			else
			{
				if ((memory?.IsDocked ?? false))
					arrived = true;
				return null;
			}

			var nextSystemLabel = memory?.InfoPanelRoute?.NextLabel.Text;
			var nextSystemExists = nextSystemLabel.Contains("Next System in Route");
			if (nextSystemExists)
			{
				var ns1 = nextSystemLabel.Substring(nextSystemLabel.IndexOf("Next System"));
				var ns2 = ns1.Substring(ns1.IndexOf(">") + 1);
				var nextSystemName = ns2.Substring(0, ns2.IndexOf("<"));

				if (memory.WindowSelectedItemView.Single().Sprite != null
				    && memory.WindowSelectedItemView.Single().Sprite
					    .Any(a => a.TexturePath.Equals("res:/UI/Texture/icons/44_32_39.png"))
				    && memory.WindowSelectedItemView.Single().LabelText.Any(lt => lt.Text.Contains(nextSystemName)))
				{
					return memory.WindowSelectedItemView.Single().Sprite
						.Single(a => a.TexturePath.Equals("res:/UI/Texture/icons/44_32_39.png")).ClickTask();
				}
			}

			return RouteElementMarkerNext.ClickMenuEntryByRegexPattern(bot, "Dock|Jump.*");
		}

		private IList<string> missionsToBookmark;
		private string SystemForMissions = string.Empty;
		private bool missionBookingInProgress = false;
		private string currentMissionToBookmark;
		private IBotTask GetBookmarkTask(Bot bot)
		{ 
			var memory = bot.MemoryMeasurementAtTime.Value;

			if (memory.InfoPanelCurrentSystem.HeaderText != SystemForMissions)
			{
				SystemForMissions = memory.InfoPanelCurrentSystem.HeaderText;
				missionsToBookmark = null;
			}
			if (!missionBookingInProgress)
			{
				if (missionsToBookmark == null)
				{
					if (memory?.Menu == null)
						return memory.InfoPanelCurrentSystem.ListSurroundingsButton.ClickTask();
					missionsToBookmark = memory.Menu.FirstOrDefault()?.Entry
						.SkipWhile(me => !me.Text.StartsWith("Agent Missions"))
						.Skip(1)
						.TakeWhile(me => !me.Text.StartsWith("Configure"))
						.Select(me => me.Text)
						.ToList();
				}

				var nextMission = missionsToBookmark.FirstOrDefault();
				if (nextMission != null)
				{
					var menus = memory.Menu.ToArray();
					switch (menus.Length)
					{
						case 0:
							return memory.InfoPanelCurrentSystem.ListSurroundingsButton.ClickTask();
						case 1:
							return menus[0].Entry.FirstOrDefault(me => me.Text == nextMission).ClickTask();
						case 2:
						{
							var entry = menus[1].Entry.FirstOrDefault(me => me.Text.StartsWith("Encounter"));
							if (entry == null)
								missionsToBookmark.Remove(nextMission);
							return entry.ClickTask();
						}

						case 3:
						{
							var entry = menus[2].Entry.FirstOrDefault(me => me.Text.StartsWith("Warp"));
							if (entry == null)
								missionsToBookmark.Remove(nextMission);
							missionBookingInProgress = true;
							currentMissionToBookmark = nextMission;
							return entry.ClickTask();
						}
					}

					return new MenuPathTask
					{
						Bot = bot,
						RootUIElement = memory.InfoPanelCurrentSystem.ListSurroundingsButton,
						ListMenuListPriorityEntryRegexPattern = new[]
						{
							new[] {nextMission.Replace("(", "\\(").Replace(")", "\\)")}, new[] {"Encounter.*"},
							new[] {"Warp to Location"}
						},
					};
				}
				else return null;
			}
			else
			{
				if (memory.ManeuverStartPossible())
				{
					var newBookmarkWindow = memory.WindowOther?.FirstOrDefault(w=>w.Caption=="New Location");
					if (newBookmarkWindow != null)
					{
						if (newBookmarkWindow.InputText.SingleOrDefault(t => t.Text == "Warp Gate") != null)
							return new EnterTextTask(currentMissionToBookmark);
						else
						{
							missionBookingInProgress = false;
							missionsToBookmark.Remove(currentMissionToBookmark);
							return newBookmarkWindow.ButtonText.Single(bt => bt.Text == "Submit").ClickTask();
						}

					}
					var overviewGate = memory.WindowOverview[0].ListView.Entry
						.FirstOrDefault(e => e.Name == "Acceleration Gate");
					if (overviewGate != null)
						return overviewGate.ClickMenuEntryByRegexPattern(bot, "Save Location.*");
				}

				return new BotTask();
			}

			return null;
		}

		public IBotTask GetStateExitActions(Bot bot) => null;

		public bool MoveToNext => arrived;
	}
}