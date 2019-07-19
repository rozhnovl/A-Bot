using System;
using System.Collections.Generic;
using System.Linq;
using BotEngine.Motor;
using Sanderling.ABot.Bot.Task;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Strategies
{
	class CorporationMissionTaker : IStrategy
	{
		private IStragegyState currentState;
		IStragegyState nextState;
		private bool isFinalizingTask;
		private int currentDestinationId = 2;
		private string[] SkippedFwSystems = null;

		public class MissionDestination
		{
			public string AgentName;
			public string MissionName;
		}

		private Queue<MissionDestination> MissionsToBookmark = new Queue<MissionDestination>();

		public CorporationMissionTaker()
		{
			currentState =
				new TravelState(
					true); // new CheckAcceptableFWSystemsState();//new WaitForCommandState("Integration test");// new ShipCheckingState();
		}

		public IEnumerable<IBotTask> GetTasks(Bot bot)
		{
			if (!isFinalizingTask)
			{
				yield return new DiagnosticTask($"Current state is {currentState.GetType().Name}");
				yield return currentState.GetStateActions(bot);
				if (currentState.MoveToNext)
				{
					switch (currentState)
					{
						case ShipCheckingState _:
							nextState = SkippedFwSystems == null
								? (IStragegyState) new CheckAcceptableFWSystemsState()
								: new SetDestinationState(currentDestinationId, SkippedFwSystems);
							break;
						case CheckAcceptableFWSystemsState checkSystemsState:
						{
							SkippedFwSystems = checkSystemsState.SystemsToSkip;
							nextState = new SetDestinationState(currentDestinationId, SkippedFwSystems);
							break;
						}

						case SetDestinationState setDestinationState:
							switch (setDestinationState.Result)
							{
								case SetDestinationTask.SetDestinationTaskResult.RouteSet:
									nextState = new TravelState(false);
									break;
								case SetDestinationTask.SetDestinationTaskResult.NoSuitableBookmark:
								{
									if (MissionsToBookmark.Any())
										nextState = new CreateDynamicRouteState();
									nextState = new WaitForCommandState("No destination left to set");
								}
									break;
								case SetDestinationTask.SetDestinationTaskResult.NextBookmarkIsEnemySystem:
									nextState = new SetDestinationState(++currentDestinationId, SkippedFwSystems);
									break;
								case null:
									break;
								default:
									throw new ArgumentOutOfRangeException();
							}
							break;
						case TravelState _:
							nextState = new TakeMissionsState();
							break;
						case TakeMissionsState takeMissionsState:
						{
							foreach (var mission in takeMissionsState.AcceptedMissions)
								MissionsToBookmark.Enqueue(mission);
							nextState = new SetDestinationState(++currentDestinationId, SkippedFwSystems);
						}
							break;
						case WaitForCommandState waitForCommand:
						{
							nextState = waitForCommand.NextState;
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

		public SetDestinationTask.SetDestinationTaskResult? Result => task?.Result;
		public bool MoveToNext => task?.Result != null;
	}
}