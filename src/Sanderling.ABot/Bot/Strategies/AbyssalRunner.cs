using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Sanderling.ABot.Bot.Configuration;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class AbyssalRunner : IStrategy
	{
		[NotNull] private IStragegyState currentState;
		private IStragegyState nextState;
		private bool isFinalizingTask;

		private readonly RunProfile profile;
		private readonly RoomStatsRecorder statsRecorder;
		private readonly (string, int)[] requiredCargoContent;

		public AbyssalRunner() : this(ProfilesRegistry.Default) { }

		public AbyssalRunner(RunProfile profile)
		{
			this.profile = profile;
			statsRecorder = new RoomStatsRecorder(profile.Name);
			requiredCargoContent = profile.RequiredCargo.Select(c => (c.Item, c.Quantity)).ToArray();

			currentState = new AbyssalFightState(
				LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(AbyssalFightState)),
				profile, statsRecorder);
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
							nextState = new ReloadAtStationState(requiredCargoContent);
							break;
						case ReloadAtStationState _:
							nextState = new WarpToBookmarkInSystemState("abyssal spot");
							break;
						case WarpToBookmarkInSystemState _:
							nextState = new AbyssalFightState(
								LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(AbyssalFightState)),
								profile, statsRecorder);
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