using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class AbyssalRunner(IStateFactory stateFactory) : IStrategy
	{
		[NotNull] private IStragegyState currentState = stateFactory.CreateAbyssalFightState();
		private IStragegyState nextState;
		private bool isFinalizingTask;

		private readonly (string, int)[] requiredCargoContent = {
			("Mobile Tractor Unit", 1),
			("Nanite Repair Paste", 100),
			("Raging Exotic Filament", 3),
			("Scourge Fury Light Missile", 1000),
			("Scourge Precision Light Missile", 1000),
		};

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
							nextState = stateFactory.CreateReloadAtStationState(requiredCargoContent);
							break;
						case ReloadAtStationState _:
							nextState = stateFactory.CreateWarpToBookmarkInSystemState("abyssal spot");
							break;
						case WarpToBookmarkInSystemState _:
							nextState = stateFactory.CreateAbyssalFightState();
							break;
						case TakeMissionsState takeMissionsState:
						{
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