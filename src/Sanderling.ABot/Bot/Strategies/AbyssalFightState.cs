using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WindowsInput.Native;
using Bib3;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class AbyssalFightState : IStragegyState
	{
		private const int maxTargetDistance = 55000;

		[NotNull] private static StreamWriter sw;
		[NotNull] private Stopwatch StateStopwatch;
		private bool enteredAbyss;

		static AbyssalFightState()
		{
			sw = new StreamWriter(
				$"R:\\AbyssalRun_{DateTime.Now.ToString("dd_hh_mm_ss")}_{Process.GetCurrentProcess().Id}.log");

		}

		public AbyssalFightState()
		{
			StateStopwatch = new Stopwatch();
			StateStopwatch.Start();
		}

		private readonly Dictionary<string, double> DpsPerEntry = new Dictionary<string, double>()
		{
			{"Sparkneedle Tessella", 25},
			{"Emberneedle Tessella", 25},
			{"Strikeneedle Tessella", 25},
			{"Blastneedle Tessella", 25},
			{"Snarecaster Tessella", 10},
			{"Spotlighter Tessella", 10},
			{"Fogcaster Tessella", 10},
			{"Gazedimmer Tessella", 10},
			{"Sparklance Tessella", 50},
			{"Emberlance Tessella", 50},
			{"Strikelance Tessella", 50},
			{"Blastlance Tessella", 50},
			{"Fieldweaver Tessella", 0},
			{"Plateforger Tessella", 0},
			{"Sparkgrip Tessera", 191},
			{"Embergrip Tessera", 191},
			{"Strikegrip Tessera", 191},
			{"Blastgrip Tessera", 191},
			{"Photic Abyssal Overmind", 108.6419753},
			{"Twilit Abyssal Overmind", 264},
			{"Bathyic Abyssal Overmind", 375.3084112},
			{"Hadal Abyssal Overmind", 457.5575221},
			{"Benthic Abyssal Overmind", 594.861461},
			{"Drifter Foothold Battleship", 100},
			{"Drifter Rearguard Battleship", 200},
			{"Drifter Frontline Battleship", 300},
			{"Drifter Vanguard Battleship", 400},
			{"Drifter Assault Battleship", 500},
			{"Drifter Entanglement Cruiser", 40},
			{"Drifter Nullwarp Cruiser", 40},
			{"Drifter Nullcharge Cruiser", 40},
			{"Ghosting Damavik", 36},
			{"Tangling Damavik", 36},
			{"Anchoring Damavik", 36},
			{"Starving Damavik", 36},
			{"Striking Damavik", 36},
			{"Striking Vila Damavik", 9 + 40},
			{"Tangling Vila Damavik", 9 + 40},
			{"Anchoring Vila Damavik", 9 + 40},
			{"Shining Vila Damavik", 9 + 40},
			{"Blinding Vila Damavik", 9 + 40},
			{"Ghosting Vila Damavik", 9 + 40},
			{"Starving Vedmak", 237.6},
			{"Harrowing Vedmak", 237.6},
			{"Harrowing Vila Vedmak", 118.8 + 40},
			{"Striking Leshak", 147.84},
			{"Renewing Leshak", 147.84},
			{"Tangling Leshak", 147.84},
			{"Starving Leshak", 147.84},
			{"Warding Leshak", 147.84},
			{"Blinding Leshak", 147.84},
			{"Lucid Escort", 24},
			{"Lucid Warden", 20},
			{"Lucid Aegis", 36},
			{"Lucid Firewatcher", 30},
			{"Lucid Preserver", 0},
			{"Lucid Watchman", 48},
			{"Lucid Upholder", 40},
			{"Lucid Sentinel", 40},
			{"Lucid Deepwatcher", 160},
			{"Ephialtes Lancer", 30},
			{"Ephialtes Entangler", 20},
			{"Ephialtes Spearfisher", 20},
			{"Ephialtes Illuminator", 20},
			{"Ephialtes Dissipator", 20},
			{"Ephialtes Obfuscator", 20},
			{"Ephialtes Confuser", 20},
			{"Vila Swarmer", 0},
			{"Triglavian Bioadaptive Cache", 0},
		};

		private double CalculateApproximateDps(IList<IOverviewEntry> entries)
		{
			return entries.Select(e =>
					DpsPerEntry.TryGetValue(e.Type.Trim(), out var dpsValue)
						? dpsValue
						: throw new KeyNotFoundException(e.Type))
				.Sum();
		}

		private int CalcTargetPriority(IOverviewEntry entry)
		{
			if (entry.Name.Contains("Anchoring"))
				return 1;

			if (entry.Name.Contains("Firewatcher"))
				return 2;

			if (entry.Name.Contains("Renewing"))
				return 3;
			if (entry.Name.Contains("Entangler") || entry.Name.Contains("Snarecaster"))
				return 6;
			if (entry.Name.Contains("Scylla"))
				return 8;
			if (entry.Name.Contains("Tyrannos"))
				return 8;
			if (entry.Name.Contains("Extraction"))
				return 10;
			if (entry.Name.Contains("Bioadaptive"))
				return 1000;
			if (entry.Type.Contains("Battleship") && entry.Type.Contains("Drifter"))
				return 9000;

			return 800 - (int) (CalculateApproximateDps(new List<IOverviewEntry>() {entry}));
		}

		private bool IsOrbitBeacon(IOverviewEntry entry)
		{
			if (entry.Name.Contains("Leshak"))
				return true;

			if (entry.Name.Contains("Overmind"))
				return true;

			if (entry.Name.Contains("Battleship"))
				return true;
			return false;
		}

		private int MwdLastTurnOnAttempt = 0;

		public IBotTask GetStateActions(Bot bot)
		{
			var shipFit = new ShipFit(bot.MemoryMeasurementAccu?.ShipUiModule,
				new[]
				{
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon, VirtualKeyCode.F1),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster, VirtualKeyCode.F2),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster, VirtualKeyCode.F3),
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc)
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener, VirtualKeyCode.CONTROL,
							VirtualKeyCode.F1),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener, VirtualKeyCode.CONTROL,
							VirtualKeyCode.F2),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.MWD, VirtualKeyCode.CONTROL, VirtualKeyCode.F3),
					}
				});

			var shipState = new ShipState(shipFit, bot);

			var overviewProvider = new MemoryProxyOverviewProvider(bot);
			var inventoryProvider = new MemoryProxyInventoryProvider(bot);
			sw.WriteLine(
				$"InputStates[{StateStopwatch.Elapsed}]: {JsonConvert.SerializeObject(new Object[] {shipState, overviewProvider, inventoryProvider})}");
			try
			{
				var task = GetActions(shipState, overviewProvider, inventoryProvider);

				sw.WriteLine(
					$"OutputTask[{StateStopwatch.Elapsed}]: {task?.ToJson()}");
				return task;
			}
			catch (Exception e)
			{
				sw.WriteLine(
					$"Exception: {e}");
				throw;
			}
		}

		private int stepIndex = 0;

		private Dictionary<long, int> lastTargetingAttemptSteps = new Dictionary<long, int>();
		private TimeSpan? LeavingAbyssTimestamp;

		public ISerializableBotTask GetActions([NotNull] ShipState shipState,
			[NotNull] MemoryProxyOverviewProvider overviewProvider,
			[NotNull] MemoryProxyInventoryProvider inventoryProvider)
		{
			stepIndex++;
			var task = new DynamicTask();
			if (!shipState.ManeuverStartPossible)
				return null;

			if (LeavingAbyssTimestamp.HasValue &&
			    LeavingAbyssTimestamp.Value.Add(TimeSpan.FromMinutes(1.2)) > StateStopwatch.Elapsed)
				return task.With(
					$"Left abyss. Waiting {LeavingAbyssTimestamp.Value.Add(TimeSpan.FromMinutes(1.2)) - StateStopwatch.Elapsed} to leave invulnerability");

			if (!shipState.IsInAbyss)
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

			var overviewEntries = overviewProvider.Entries;
			var offensiveOverviewEntries = overviewEntries
				?.Where(entry => entry.IsEnemy)
				?.Where(entry => !entry.Name.Contains("Extraction"))
				?.Where(e => e.Type != "Vila Swarmer")
				?.ToList();
			var listOverviewEntryToAttack = offensiveOverviewEntries
				?.Where(entry => entry.Distance <= maxTargetDistance)
				?.Where(entry => entry.Name != "Vila Swarmer")
				?.OrderBy(CalcTargetPriority)
				?.ThenBy(entry => entry?.Distance ?? int.MaxValue)
				?.ToArray();

			var estimatedIncomingDps = CalculateApproximateDps(offensiveOverviewEntries);
			var conduit = overviewEntries
				?.Where(entry => entry.Name.Contains("Conduit") && !entry.Name.Contains("Proving"))
				?.SingleOrDefault();
			var orbitBeacon = overviewEntries?.Where(IsOrbitBeacon)?.OrderBy(CalcTargetPriority)?.FirstOrDefault() ??
			                  conduit;
			task.With($"Current maneuver is {shipState.Maneuver}." +
			          Environment.NewLine +
			          $"Incoming DPS is {estimatedIncomingDps}.");


			//goto targetProcessing;
			if (estimatedIncomingDps > 200)
			{
				if (shipState.Maneuver != ShipManeuverTypeEnum.Orbit)
				{
					task.With($"Orbiting {conduit}");
					return task.With(orbitBeacon.ClickMenuEntryByRegexPattern("Orbit.*", "5,000 m"));
				}

				var mwdTask = shipState.GetSetModuleActiveTask(ShipFit.ModuleType.MWD,
					shipState.HitpointsAndEnergy.Capacitor > 200);
				if (mwdTask != null && MwdLastTurnOnAttempt < stepIndex - 10)
				{
					MwdLastTurnOnAttempt = stepIndex;
					return task.With(mwdTask);
				}
			}
			else
			{
				task.With($"Incoming DPS is {estimatedIncomingDps}. No need to avoid");
				if (shipState.Maneuver != ShipManeuverTypeEnum.Approach &&
				    shipState.Maneuver != ShipManeuverTypeEnum.KeepAtRange)
					return task.With(conduit.ClickMenuEntryByRegexPattern("Keep at range", "500 m"));

				task.With($"Distance to target is {conduit.Distance}.");
				var mwdTask = shipState.GetSetModuleActiveTask(ShipFit.ModuleType.MWD, conduit.Distance > 5000);

				if (mwdTask != null)
					return task.With(mwdTask);
			}

			var tankingTask = shipState.GetNextTankingModulesTask(estimatedIncomingDps);

			if (tankingTask != null)
			{
				return task.With(tankingTask);
			}

			targetProcessing:

			#region drones

			var drones = shipState.Drones;

			#endregion

			//task.With($"Weapon status: IsActive = {weaponGroup.UiModule.IsActive(bot)}. IsReloading = {weaponGroup.UiModule.IsReloading(bot)}. Ramp = {weaponGroup.UiModule.RampRotationMilli}");
			if (shipState.ActiveTargets.Count > 0)
			{
				task.With($"Targets selected: {shipState.ActiveTargets.Count}");
				var focusedTarget = shipState.ActiveTargets.List.Last();

				ISerializableBotTask weaponTask = shipState.GetSetModuleActiveTask(ShipFit.ModuleType.Weapon,
					focusedTarget.Distance < 25000);
				if (weaponTask != null)
					return task.With(weaponTask);
				var shouldDronesAttack =
					!focusedTarget.DroneAssigned && focusedTarget.Distance <= 55000;

				if (shouldDronesAttack)
				{
					task.With($"No target attacked. Launching drones.");
					if (drones.ShouldLaunch)
						return task.With(HotkeyRegistry.LaunchDrones);

					return task.With(HotkeyRegistry.EngageDrones);
				}
			}

			task.With(
				$"Spare enemies to attack: {listOverviewEntryToAttack.Length}: {string.Concat(listOverviewEntryToAttack.Select(oe => Environment.NewLine + '\t' + (oe.MeTargeted == true ? "[Targeted]" : string.Empty) + oe.Name))}");

			if (shipState.ActiveTargets == null || shipState.ActiveTargets.Count < 5)
			{
				foreach (var overviewEntry in listOverviewEntryToAttack.Where(e => e.MeTargeted != true))
				{
					if (lastTargetingAttemptSteps.ContainsKey(overviewEntry.Id))
						if (lastTargetingAttemptSteps[overviewEntry.Id] > stepIndex - 3)
							continue;
					lastTargetingAttemptSteps[overviewEntry.Id] = stepIndex;
					return task.With(overviewEntry.GetSelectTask());
				}
			}

			var wrongTargetedEntries =
				shipState.ActiveTargets?.List?.Where(t =>
					((!t.Name.Contains("Extraction") && !offensiveOverviewEntries.Any(oe => oe.Name == t.Name))
					 || (t.Name.Contains("Extraction") && t.Distance > 50000)));
			if (wrongTargetedEntries?.Any() ?? false)
			{
				task.With(
					$"Wrong targeted entries: {string.Join(",", wrongTargetedEntries.Select(wte => wte.Name + "@" + wte.Distance + "[" + offensiveOverviewEntries.Any(oe => oe.Name == wte.Name) + "]"))}");
				return task.With(wrongTargetedEntries.FirstOrDefault().GetUnlockTask());
			}

			if (!(0 < listOverviewEntryToAttack?.Length))
			{
				var reloadTask = shipState.GetReloadTask();
				if (drones.HasNonReturningDronesInSpace)
					return task.With(HotkeyRegistry.ReturnDrones);
				else if (reloadTask != null)
					return task.With(reloadTask);
			}

			looting:
			if (overviewEntries.Any(e => e.Name.Contains("Bioadaptive")))
			{
				if (conduit.Distance < 2000 && !overviewEntries.Any(e => e.Name.Contains("Tractor")))
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
			else if (overviewEntries.Any(e => e.Name.Contains("Tractor")))
			{
				var tractorEntry = overviewEntries.Single(e => e.Name.Contains("Tractor"));

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
				if (shipState.Maneuver != ShipManeuverTypeEnum.Approach)
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
				return task.With(conduit.ClickMenuEntryByRegexPattern("Activate Gate"));
			}

			return null;
		}

		private bool enteringAbyss;

		private ISerializableBotTask EnterAbyssIfNeeded([NotNull] ShipState shipState,
			[NotNull] MemoryProxyInventoryProvider inventoryProvider)
		{
			var activateTask = shipState.GetPopupButtonTask("Activate");
			if (activateTask != null)
			{
				enteringAbyss = true;
				return activateTask;
			}

			if (enteringAbyss)
				return null;
			if (inventoryProvider.GetOpenWindowTask() != null)
				return inventoryProvider.GetOpenWindowTask();

			return inventoryProvider.GetActvateItemIfPresentTask("Raging Exotic Filament", "Use .*");
		}


		public IBotTask GetStateExitActions(Bot bot)
		{
			throw new NotImplementedException();
		}

		public bool MoveToNext { get; }
	}
}