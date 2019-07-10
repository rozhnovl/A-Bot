using System;
using System.Collections.Generic;
using System.Linq;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot.Strategies
{
	class CorporationMissionTaker : IStrategy
	{
		private IStragegyState currentState;
		IStragegyState nextState;
		private bool isFinalizingTask;
		private int currentDestinationId = 1;
		private string[] SkippedFwSystems = null;

		public class MissionDestination
		{
			public string AgentName;
			public string MissionName;
		}

		private Queue<MissionDestination> MissionsToBookmark = new Queue<MissionDestination>();

		public CorporationMissionTaker()
		{
			currentState = new CheckAcceptableFWSystemsState(); // new ShipCheckingState();
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
							if (setDestinationState.Result == SetDestinationTask.SetDestinationTaskResult.RouteSet)
								nextState = new TravelState();
							else
							{
								if (MissionsToBookmark.Any())
									nextState = null; //TODO new CreateDynamicRouteState();
								nextState = new WaitForCommandState();
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