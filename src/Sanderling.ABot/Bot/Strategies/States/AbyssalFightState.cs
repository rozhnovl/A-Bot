using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Bib3;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sanderling.ABot.Bot.Configuration;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class AbyssalFightState : IStragegyState
	{
		private readonly ILogger logger;
		private const int MaxTargetDistance = 55000;

		//[NotNull] private static StreamWriter sw;
		[NotNull] private readonly Stopwatch StateStopwatch;
		private readonly NpcInfoProvider npcInfoProvider = new NpcInfoProvider();

		AbyssEnemySpawnContext dbContext = new AbyssEnemySpawnContext();

		private bool enteredAbyss;


		public AbyssalFightState(ILogger logger)
		{
			this.logger = logger;
			StateStopwatch = new Stopwatch();
			StateStopwatch.Start();
		}


		private int MwdLastTurnOnAttempt = 0;

		public IBotTask GetStateActions(Bot bot)
		{
			var shipFit = FitsRegistry.Hawk(bot);
			var shipState = new ShipState(shipFit, bot);

			var overviewProvider = new MemoryProxyOverviewProvider(bot);
			var inventoryProvider = new MemoryProxyInventoryProvider(bot);
			logger.LogInformation(
				$"InputStates[{StateStopwatch.Elapsed}]: {JsonConvert.SerializeObject(new StateInput(shipState, overviewProvider, inventoryProvider))}");
			try
			{
				var task = GetActions(bot, shipState, overviewProvider, inventoryProvider, new PriorityManager(shipFit, npcInfoProvider));

				logger.LogInformation(
					$"OutputTask[{StateStopwatch.Elapsed}]: {task?.ToJson()}");
				return task;
			}
			catch (Exception e)
			{
				logger.LogInformation(
					$"Exception: {e}");
				throw;
			}
		}

		private int stepIndex = 0;

		private Dictionary<long, int> lastTargetingAttemptSteps = new Dictionary<long, int>();
		private TimeSpan? LeavingAbyssTimestamp;

		public class StateInput
		{
			public StateInput(ShipState shipState, MemoryProxyOverviewProvider overviewProvider,
				MemoryProxyInventoryProvider inventoryProvider)
			{
				ShipState = shipState;
				OverviewProvider = overviewProvider;
				InventoryProvider = inventoryProvider;
			}

			public IShipState ShipState { get; set; }
			public IOverviewProvider OverviewProvider { get; set; }
			public IInventoryProvider InventoryProvider { get; set; }
		}

		public ISerializableBotTask GetActions(Bot bot, [NotNull] IShipState shipState,
			[NotNull] IOverviewProvider overviewProvider,
			[NotNull] IInventoryProvider inventoryProvider,
			PriorityManager priorityManager)
		{
			stepIndex++;
			var task = new DynamicTask();
			if (!shipState.ManeuverStartPossible)
				return null;

			var coreCache = overviewProvider.Entries?.FirstOrDefault(e => e.Name.Contains("Bioadaptive")|| e.Name.Contains("Biocombinative"));
			var conduit = overviewProvider.Entries
				?.Where(entry =>
					(entry.Name ?? entry.Type).Contains("Conduit") && !(entry.Name ?? entry.Type).Contains("Proving"))
				?.SingleOrDefault(); //!!!!
			if (conduit == null && LeavingAbyssTimestamp.HasValue &&
			    LeavingAbyssTimestamp.Value.Add(TimeSpan.FromSeconds(46)) > StateStopwatch.Elapsed)
				return task.With(
					$"Left abyss. Waiting {LeavingAbyssTimestamp.Value.Add(TimeSpan.FromMinutes(1.2)) - StateStopwatch.Elapsed} to leave invulnerability");
			/*dbContext.Spawns.Add(new AbyssEnemySpawn()
				{Enemies = overviewProvider.Entries.Select(e => e.Name).ToArray(), Id = Guid.NewGuid(), Time = DateTime.Now});*/
			if (conduit == null)
			{
				if (enteredAbyss)
				{
					LeavingAbyssTimestamp = StateStopwatch.Elapsed;
					enteredAbyss = false;
					//TODO check need for refill;
					return task.With("Left abyss. Waiting a minute to leave invulnerability");
				}

				var enterAbyssTask = EnterAbyssIfNeeded(shipState, inventoryProvider);
				if (enterAbyssTask != null)
					return enterAbyssTask;
			}
			else
			{
				enteringAbyss = false;
				enteredAbyss = true;
			}

			var turnOnAlwaysActiveModulesTask = shipState.GetTurnOnAlwaysActiveModulesTask();
			if (turnOnAlwaysActiveModulesTask != null)
				return task.With(turnOnAlwaysActiveModulesTask);

			var offensiveOverviewEntries = overviewProvider.Entries
				?.Where(entry => entry.IsEnemy)
				?.Where(entry => !entry.Name.Contains("Extraction"))
				?.Where(e => e.Type != "Vila Swarmer")
				?.ToList();
			var listOverviewEntryToAttack = priorityManager.GetEnemies(overviewProvider);
			if (listOverviewEntryToAttack.Any())
				changingRoom = false;
			var estimatedIncomingDps = npcInfoProvider.CalculateApproximateDps(overviewProvider);
			var orbitBeacon = overviewProvider.Entries?.Where(npcInfoProvider.IsOrbitBeacon)
				                  ?.OrderBy(npcInfoProvider.CalcTargetPriority)?.FirstOrDefault() ??
			                  coreCache ?? conduit;
			task.With($"Current maneuver is {shipState.Maneuver}." +
			          Environment.NewLine +
			          $"Incoming DPS is {estimatedIncomingDps}.");


			//goto targetProcessing;
			if (estimatedIncomingDps > 200)
			{
				if (shipState.Maneuver != ShipManeuverType.Orbit)
				{
					task.With($"Orbiting {orbitBeacon}");
					return task.With(orbitBeacon.ClickMenuEntryByRegexPattern("Orbit.*", "5,000 m"));
				}

				//var mwdTask = shipState.GetSetModuleActiveTask(ShipFit.ModuleType.MWD,
				//	shipState.HitpointsAndEnergy.Capacitor > 200);
				//if (mwdTask != null && MwdLastTurnOnAttempt < stepIndex - 10)
				//{
				//	MwdLastTurnOnAttempt = stepIndex;
				//	return task.With(mwdTask);
				//}
			}
			else
			{
				task.With($"Incoming DPS is {estimatedIncomingDps}. No need to avoid");
				//TODO should not be switching from target obit
				if (shipState.Maneuver != ShipManeuverType.Approach &&
				    shipState.Maneuver != ShipManeuverType.KeepAtRange
				    && shipState.Maneuver != ShipManeuverType.Orbit)
					//TODO HERE
					return task.With(orbitBeacon.ClickMenuEntryByRegexPattern("Keep at range", "500 m"));

				task.With($"Distance to target is {orbitBeacon.Distance}.");
				var mwdTask = shipState.GetSetModuleActiveTask(ShipFit.ModuleType.MWD, orbitBeacon.Distance > 2000);

				if (mwdTask != null)
					return task.With(mwdTask);
			}


			//TODO nested task?
			var combatTask = new CombatTask(bot, shipState.Fit,
				new DronesContoller(bot.MemoryMeasurementAtTime.Value, shipState.Fit),
				new PriorityManager(shipState.Fit, npcInfoProvider));
			foreach (var ct in combatTask.Component)
			{
				return task.With((ISerializableBotTask)ct);
			}

			looting:
			if (shipState.ShouldUseTractorForLooting)
			{
				if (coreCache!=null)
				{
					if (conduit.Distance < 2000 && !overviewProvider.Entries.Any(e => e.Name.Contains("Tractor")))
					{
						var openInventoryTask = inventoryProvider.GetOpenWindowTask();
						if (openInventoryTask != null)
							return task.With(openInventoryTask);

						task.With("We are near conduit and have nothing to do. Time to deploy tractor");
						var launchTractorTask =
							inventoryProvider.GetActvateItemIfPresentTask("Mobile Tractor Unit", ".*Launch.*");
						if (launchTractorTask != null)
							return task.With(launchTractorTask);
					}
				}
				else if (overviewProvider.Entries.Any(e => e.Name.Contains("Tractor")))
				{
					var tractorEntry = overviewProvider.Entries.Single(e => e.Name.Contains("Tractor"));

					var lootWindowProvider = inventoryProvider.GetLootableWindow();

					if (lootWindowProvider != null)
					{
						return task.With(
							lootWindowProvider.IsEmpty
								? tractorEntry.ClickMenuEntryByRegexPattern("Scoop.*")
								: lootWindowProvider.GetClickLootButtonTask());
					}

					if (tractorEntry.Distance < 2500)
						return task.With(tractorEntry.ClickMenuEntryByRegexPattern("Open Cargo"));
					if (shipState.Maneuver != ShipManeuverType.Approach)
					{
						return task.With("Approach");
					}
				}
				else if (offensiveOverviewEntries.IsNullOrEmpty())
				{
					var closeInventoryTask = inventoryProvider.GetCloseWindowTask();
					if (closeInventoryTask != null)
						return task.With(closeInventoryTask);
					task.With("Room finished, time to jump");
					if (!changingRoom)
					{
						changingRoom = true;
						return task.With(conduit.ClickMenuEntryByRegexPattern("Activate Gate"));
					}
				}
			}
			else
			{
				if (coreCache != null)
				{
					if (coreCache.Name.Contains("Wreck"))
					{
						var lootWindowProvider = inventoryProvider.GetLootableWindow();

						if (lootWindowProvider is { IsEmpty: false })
						{
							return task.With(lootWindowProvider.GetClickLootButtonTask());
						}

						if (coreCache.Distance < 2500)
							return task.With(coreCache.ClickMenuEntryByRegexPattern("Open Cargo"));
						if (shipState.Maneuver != ShipManeuverType.Approach)
						{
							return task.With(coreCache.GetApproachTask());
						}
					}
				}
				else if (offensiveOverviewEntries.IsNullOrEmpty())
				{
					//var closeInventoryTask = inventoryProvider.GetCloseWindowTask();
					//if (closeInventoryTask != null)
					//	return task.With(closeInventoryTask);
					task.With("Room finished, time to jump");
					return task.With(conduit.ClickMenuEntryByRegexPattern("Activate Gate"));
				}
			}

			return task;
		}

		private bool enteringAbyss;
		private bool changingRoom;

		private ISerializableBotTask EnterAbyssIfNeeded([NotNull] IShipState shipState,
			[NotNull] IInventoryProvider inventoryProvider)
		{
			var activateTask = shipState.GetPopupButtonTask("Activate");
			if (activateTask != null)
			{
				enteringAbyss = true;
				return activateTask;
			}

			if (enteringAbyss)
				return null;
			var openTask = inventoryProvider.GetOpenWindowTask();
			if (openTask != null)
				return openTask;

			return inventoryProvider.GetActvateItemIfPresentTask("Raging Exotic Filament", "Use .*");
		}


		public IBotTask GetStateExitActions(Bot bot)
		{
			throw new NotImplementedException();
		}

		public bool MoveToNext { get; }
	}
}