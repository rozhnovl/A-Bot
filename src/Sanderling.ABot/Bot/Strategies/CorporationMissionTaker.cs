using System;
using System.Collections.Generic;
using System.Linq;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class CorporationMissionTaker : IStrategy
	{
		private IStragegyState currentState;
		private IStragegyState nextState;
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
			var startState = Environment.GetCommandLineArgs().Skip(2).FirstOrDefault();

			switch (startState)
			{
				case "takeMissions":
				case null:
					currentState = new CheckAcceptableFWSystemsState();
					break;
				case "bookmark":
					currentState = new TravelState(true);
					break;
			}
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
}