using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3.Geometrik;
using BotEngine.Motor;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Strategies
{
	class CorporationMissionTaker : IStrategy
	{
		private IStragegyState currentState;
		IStragegyState nextState;
		private bool isFinalizingTask;
		private int currentDestinationId = 1;
		public CorporationMissionTaker()
		{
			currentState = new ShipCheckingState();// new ShipCheckingState();
		}

		public IEnumerable<IBotTask> GetTasks(Bot bot)
		{
			if (!isFinalizingTask)
			{
				yield return currentState.GetStateActions(bot);
				if (currentState.MoveToNext)
				{
					switch (currentState)
					{
						case ShipCheckingState _:
							nextState = new SetDestinationState();
							break;
						case SetDestinationState _:
							nextState = new TravelState();
							break;
						case TravelState _:
							nextState = new TakeMissionsState();
							break;
						case TakeMissionsState _:
						{
							nextState = new SetDestinationState();
							currentDestinationId++;
						}
							break;
						default:
							throw new ArgumentOutOfRangeException(
								$"No way found to leave {currentState.GetType().Name} state");
					}

					isFinalizingTask = true;
				}
			}
			else
			{
				var finalizingActions = currentState.GetStateExitActions(bot);
				if (finalizingActions != null)
					yield return finalizingActions;
				else
				{
					isFinalizingTask = false;
					currentState = nextState;
				}
			}
		}
	}

	interface IStragegyState
	{
		IBotTask GetStateActions(Bot bot);
		IBotTask GetStateExitActions(Bot bot);
		bool MoveToNext { get; }
	}

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


	class CheckAcceptableFWSystemsState : IStragegyState
	{
		public string[] SystemsToSkip = null;
		public IBotTask GetStateActions(Bot bot)
		{
			var memory = bot?.MemoryMeasurementAtTime?.Value;

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
	class TakeMissionsState : IStragegyState
	{
		private ISet<string> AcceptedMissionLevels = new HashSet<string>() { "Level 3" , "Level 4" };
		private ISet<string> IgnoredMissions = new HashSet<string>(){ "Gate Blitz", "Uproot" };
		private ISet<string> CheckedAgents = new HashSet<string>();
        private Regex MissionNameRegex = new Regex("<span id=subheader>([a-zA-Z0-9\\s]*)</span>");
		private bool allMissionsProcessed;
		public IBotTask GetStateActions(Bot bot)
		{
			var memory = bot?.MemoryMeasurementAtTime?.Value;

			if (memory.WindowAgentDialogue?.Any() ?? false)
			{
				var agentDialogue = memory.WindowAgentDialogue.FirstOrDefault();
				if (agentDialogue.ButtonText.Any(bt => bt.Text == "Request Mission" || bt.Text == "View Mission"))
					return agentDialogue.ButtonText
						.First(bt => bt.Text == "Request Mission" || bt.Text == "View Mission")
						.ClickTask();
				var missionDescription = ((WindowAgentDialogue) agentDialogue.RegionInteraction).LeftPane.Html;
				var missionTarget = ((WindowAgentDialogue) agentDialogue.RegionInteraction).RightPane.Html;
				if (!MissionNameRegex.IsMatch(missionDescription))
				{
					throw new InvalidOperationException("Failed to parse mission name");
				}

				var acceptButton = agentDialogue?.ButtonText?.FirstOrDefault(b => b.Text == "Accept");
				if (acceptButton != null)
				{
					var missionName = MissionNameRegex.Match(missionDescription).Groups[1].Value;
					if (!IgnoredMissions.Contains(missionName))
					{
						return acceptButton.ClickTask();
					}
					else if (!missionDescription.Contains("Declining a mission from this agent within the next"))
						return agentDialogue.ButtonText
							.First(bt => bt.Text == "Decline")
							.ClickTask();
				}

				var agentNameWithLevel = agentDialogue.Caption.Substring(agentDialogue.Caption.IndexOf('-'));
				CheckedAgents.Add(agentNameWithLevel.Substring(2, agentNameWithLevel.IndexOf('<') - 3));
				return agentDialogue.ClickMenuEntryByRegexPattern(bot, ".*Close");
			}

			//check if can speak to another
			var agentsToCheck = memory.WindowStation.Single().AgentEntry.Where(a =>
				a.LabelText.Any(lt => AcceptedMissionLevels.Any(aml => lt.Text.Contains(aml))));
			var notCheckedYetAgents =
				agentsToCheck
					.Where(a => a.LabelText.Any(t => t.Text == "Security"))
					.Where(a => !a.LabelText.Any(t => CheckedAgents.Any(ca => t.Text.Contains(ca))));
			var agentToTalk = notCheckedYetAgents.FirstOrDefault();
			if (agentToTalk != null)
				return agentToTalk.DoubleClickTask();
			allMissionsProcessed = true;
			return null;
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

		public bool MoveToNext => allMissionsProcessed;
	}
	class TravelState : IStragegyState
	{
		private bool arrived;

		public IBotTask GetStateActions(Bot bot)
		{
			var memory = bot.MemoryMeasurementAtTime.Value;
			var ManeuverType = memory?.ShipUi?.Indication?.ManeuverType;

			if (ShipManeuverTypeEnum.Warp == ManeuverType ||
			    ShipManeuverTypeEnum.Jump == ManeuverType)
				return null;  //	do nothing while warping or jumping.

			//	from the set of route element markers in the Info Panel pick the one that represents the next Waypoint/System.
			//	We assume this is the one which is nearest to the topleft corner of the Screen which is at (0,0)
			var RouteElementMarkerNext =
				memory?.InfoPanelRoute?.RouteElementMarker
					?.OrderByCenterDistanceToPoint(new Vektor2DInt(0, 0))?.FirstOrDefault();

			if (RouteElementMarkerNext!=null)
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
			
			return RouteElementMarkerNext.ClickMenuEntryByRegexPattern(bot, "Dock|Jump.*");
		}

		public IBotTask GetStateExitActions(Bot bot)
		{
			return null;
		}

		public bool MoveToNext => arrived;
	}
}