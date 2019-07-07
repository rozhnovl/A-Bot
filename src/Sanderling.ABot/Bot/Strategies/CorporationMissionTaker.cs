using System;
using System.Collections.Generic;

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
}