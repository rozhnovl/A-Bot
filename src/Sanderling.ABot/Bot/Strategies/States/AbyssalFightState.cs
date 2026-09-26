using WindowsInput.Native;
﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Bib3;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sanderling.ABot.Bot.Configuration;
using Sanderling.ABot.Bot.Task;
using Sanderling.ABot.Parse;
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

		private readonly RunProfile profile;
		private readonly RoomStatsRecorder stats;

		private bool enteredAbyss;


		public AbyssalFightState(ILogger logger, RunProfile profile, RoomStatsRecorder stats)
		{
			this.logger = logger;
			this.profile = profile;
			this.stats = stats;
			StateStopwatch = new Stopwatch();
			StateStopwatch.Start();
		}


		private int MwdLastTurnOnAttempt = 0;

		public IBotTask GetStateActions(Bot bot)
		{
			var measurement = bot.MemoryMeasurementAtTime?.Value;

			// Guard: this state needs a ship in space with a readable module layout. When docked,
			// on a loading/character screen, or before the ship UI is parsed, building the fit
			// would throw (NRE / "3 module groups"). Wait quietly instead of crashing the loop.
			if (measurement?.ShipUi == null)
				return new DynamicTask().With("Waiting: no ship UI in space (docked / loading?).");

			ShipFit shipFit;
			try
			{
				shipFit = profile.BuildFit(bot);
			}
			catch (Exception e)
			{
				return new DynamicTask().With(
					$"Waiting: cannot map fit '{profile.Name}' to the ship's modules yet ({e.Message}).");
			}

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
			// Ammo stock: read the hold whenever it is open (docked readiness check, looting) and the
			// launcher count every tick, so the charge planner knows what is left before the next room.
			AmmoController.Observe(bot, shipState.Fit);
			if (!shipState.ManeuverStartPossible)
				return null;

			// Record what this room contains for later fit/strategy analysis.
			stats?.Observe(overviewProvider.Entries?.Select(e =>
				new RoomStatsRecorder.OverviewSnapshotEntry(e.Name, e.Type, e.IsEnemy)));
			// Loot ledger: whenever the ship cargo is open and readable, diff it against the last
			// reading and append the changes (what we picked up / burned) for ISK-per-run stats.
			stats?.ObserveCargo(MemoryProxyInventoryProvider.TryReadShipCargoItems(bot?.MemoryMeasurementAtTime?.Value));

			var coreCache = overviewProvider.Entries?.FirstOrDefault(e => e.Name.Contains("Bioadaptive")|| e.Name.Contains("Biocombinative"));
			// A room can show TWO conduits at once (Origin = the way in, Transfer = the gate onward),
			// so SingleOrDefault threw "more than one element". Prefer the Transfer/exit conduit; the
			// Origin one is only the entrance we arrived through.
			var conduits = overviewProvider.Entries
				?.Where(entry =>
					(entry.Name ?? entry.Type).Contains("Conduit") && !(entry.Name ?? entry.Type).Contains("Proving"))
				?.ToList() ?? new List<IOverviewEntry>();
			var conduit = conduits.FirstOrDefault(e => (e.Name ?? e.Type).Contains("Transfer"))
			              ?? conduits.FirstOrDefault(e => !(e.Name ?? e.Type).Contains("Origin"))
			              ?? conduits.FirstOrDefault();
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
					stats?.EndRun();
					//TODO check need for refill;
					return task.With("Left abyss. Waiting a minute to leave invulnerability");
				}

				var enterAbyssTask = EnterAbyssIfNeeded(bot, shipState, inventoryProvider);
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
			{
				// A spawn on grid = a room that still has to be cleared: reset the end-of-room
				// bookkeeping here rather than on our own gate count, which a hand-pressed gate skips.
				changingRoom = false;
				gateActivatedAt = null;
				cacheLooted = false;
				reloadIssued = false;
				gateWaitStartedAt = null;
			}
			var estimatedIncomingDps = npcInfoProvider.CalculateApproximateDps(overviewProvider);
			// What is left on grid cannot break the tank: the room is winding down. Everything that
			// follows may start the end-of-room work (cache, conduit) while the last rats die.
			var roomIsHarmless = shipState.Fit.SustainableIncomingDps > 0 &&
			                     estimatedIncomingDps <= shipState.Fit.SustainableIncomingDps;
			var orbitBeacon = overviewProvider.Entries?.Where(npcInfoProvider.IsOrbitBeacon)
				                  ?.OrderBy(npcInfoProvider.CalcTargetPriority)?.FirstOrDefault() ??
			                  coreCache ?? conduit;
			task.With($"Current maneuver is {shipState.Maneuver}." +
			          Environment.NewLine +
			          $"Incoming DPS is {estimatedIncomingDps}.");

			// ENTERING A ROOM: get the ship moving on an anchor BEFORE opening fire. A Hawk that starts
			// shooting while sitting still eats the whole spawn's applied damage; the orbit is the tank
			// (operator's order: orbit first, then regroup, then fight).
			if (conduit != null && listOverviewEntryToAttack.Any() && !roomIsHarmless &&
			    shipState.Maneuver == ShipManeuverType.None && orbitBeacon != null)
			{
				task.With($"Entering room — orbiting {orbitBeacon} before engaging");
				return task.With(orbitBeacon.ClickMenuEntryByRegexPattern("Orbit.*"));
			}

			// REGROUP: the lead calls the fleet in once per room, right after the orbit is set and before
			// the shooting starts (operator bound fleet regroup to Alt+R on the tank client).
			if (conduit != null && listOverviewEntryToAttack.Any() && IsLooter(bot) &&
			    regroupCalledInRoom != roomIndex)
			{
				regroupCalledInRoom = roomIndex;
				task.With("Entering room — calling the fleet to regroup");
				return task.With(new HotkeyTask(VirtualKeyCode.VK_R, VirtualKeyCode.MENU));
			}

			// NO THREAT: when the room cannot out-damage the tank, stop trading volleys at range and go
			// collect the loot — the guns keep firing at whatever is locked while we fly (operator).
			var lootableWreck = overviewProvider.Entries?.FirstOrDefault(e =>
				NpcInfoProvider.IsWreckName(e.Name) && NpcInfoProvider.IsAbyssalCacheName(e.Name));
			if (roomIsHarmless && lootableWreck != null && !lootableWreck.IsEmptyWreck && IsLooter(bot) && !cacheLooted)
			{
				task.With($"Incoming {estimatedIncomingDps} DPS is within the tank — looting early");
				goto looting;
			}

			// Flying to the cache while the room is still being cleared saves the trip afterwards
			// (operator, 2026-09-26): when the spawn cannot break the tank and the cache is not popped
			// yet, the looter heads there now — the launchers keep firing on the way.
			if (roomIsHarmless && IsLooter(bot) && coreCache != null && !coreCache.Name.Contains("Wreck") &&
			    coreCache.Distance > 2500 && shipState.Maneuver != ShipManeuverType.Approach &&
			    listOverviewEntryToAttack.Any())
			{
				task.With($"Incoming {estimatedIncomingDps} DPS is within the tank — flying to the cache early");
				return task.With(coreCache.GetApproachTask());
			}

			// ROOM WINDING DOWN: the wings — and the looter once the cache is done — head for the
			// conduit now, so the fleet is already gathered when the last rat dies. The launchers keep
			// firing on the way (light missiles reach 59 km).
			if (roomIsHarmless && listOverviewEntryToAttack.Any() && conduit != null &&
			    conduit.Distance > GateGatherRangeM && shipState.Maneuver != ShipManeuverType.Approach &&
			    (!IsLooter(bot) || cacheLooted || coreCache == null))
			{
				task.With($"Incoming {estimatedIncomingDps} DPS is within the tank — heading for the conduit early");
				return task.With(conduit.GetApproachTask());
			}

			// COMBAT FIRST: tank / lock / shoot / drones take priority over repositioning and looting.
			// Return the first ACTIONABLE combat task — skip the log-only DiagnosticTasks, which were
			// being returned as the "action" and produced motion-less (no-op) ticks. Positioning and
			// looting below run only when combat has nothing to do this tick.
			var combatTask = new CombatTask(bot, shipState.Fit,
				new DronesContoller(bot.MemoryMeasurementAtTime.Value, shipState.Fit),
				new PriorityManager(shipState.Fit, npcInfoProvider));
			foreach (var ct in combatTask.Component)
			{
				if (ct is DiagnosticTask) continue;
				return task.With((ISerializableBotTask)ct);
			}

			//goto targetProcessing;
			if (orbitBeacon == null)
			{
				// Nothing to anchor on yet (freshly loaded room, or every beacon/cache/conduit gone) —
				// skip positioning and fall through to the combat task instead of NRE'ing on a null beacon.
				task.With("No orbit anchor (beacon/cache/conduit) on grid — skipping positioning.");
			}
			else if (estimatedIncomingDps > 200)
			{
				if (shipState.Maneuver != ShipManeuverType.Orbit)
				{
					task.With($"Orbiting {orbitBeacon}");
					// Click the top-level "Orbit" entry (no distance submenu): the client then uses the
					// player's default orbit distance, which doubles as the fit's kite range (e.g. 25 km
					// for the triplebox Hawks). A ship already orbiting (e.g. a wing hawk anchored on the
					// fleet lead) never reaches this branch, so a manually-set orbit is preserved.
					return task.With(orbitBeacon.ClickMenuEntryByRegexPattern("Orbit.*"));
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
				// A harmless room needs no anchor: a ship that has stopped at the cache or the conduit
				// must stay there, not fly back to the beacon.
				if (!roomIsHarmless &&
				    shipState.Maneuver != ShipManeuverType.Approach &&
				    shipState.Maneuver != ShipManeuverType.KeepAtRange
				    && shipState.Maneuver != ShipManeuverType.Orbit)
					//TODO HERE
					return task.With(orbitBeacon.ClickMenuEntryByRegexPattern("Keep at range", "500 m"));

				task.With($"Distance to target is {orbitBeacon.Distance}.");
				// MWD only to close a BIG gap — the fit isn't cap-stable, so we don't run it just to
				// hold position (that drains cap the active tank needs). Off once we're within range.
				var mwdTask = shipState.GetSetModuleActiveTask(ShipFit.ModuleType.MWD, orbitBeacon.Distance > 12000);

				if (mwdTask != null)
					return task.With(mwdTask);
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
					if (!IsLooter(bot))
						return task.With("Not the looter — holding while the lead empties the tractor");

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
						stats?.AdvanceRoom();
						return task.With(conduit.ClickMenuEntryByRegexPattern("Activate Gate"));
					}
				}
			}
			else
			{
				// END OF ROOM — the operator's protocol (2026-09-26): the looter empties the cache wreck,
				// everybody flies to the conduit, and the fleet takes the gate together once every ship
				// is within GateGatherRangeM. Wings no longer wait at the wreck: an emptied wreck stays on
				// grid, which left them holding forever on 2026-09-26. They go and wait at the conduit.
				var cacheWreck = coreCache != null && coreCache.Name.Contains("Wreck") ? coreCache : null;
				if (cacheWreck != null)
					task.With($"Cache wreck icon {cacheWreck.IconTexturePath?.Split('/').LastOrDefault()} empty={cacheWreck.IsEmptyWreck} looted={cacheLooted}");
				if (cacheWreck != null && cacheWreck.IsEmptyWreck && !cacheLooted)
				{
					// The client already draws the wreck as looted — nothing to fly to.
					cacheLooted = true;
					task.With("Cache wreck is empty — heading for the conduit");
				}
				if (cacheWreck != null && IsLooter(bot) && !cacheLooted)
				{
					var lootWindowProvider = inventoryProvider.GetLootableWindow();
					if (lootWindowProvider is { IsEmpty: false })
						return task.With(lootWindowProvider.GetClickLootButtonTask());
					if (lootWindowProvider is { IsEmpty: true })
					{
						// Loot All has been taken. The wreck stays on grid empty, so remember that for
						// this room instead of re-opening it forever.
						cacheLooted = true;
						task.With("Cache emptied — heading for the conduit");
					}
					else
					{
						if (cacheWreck.Distance < 2500)
							return task.With(cacheWreck.ClickMenuEntryByRegexPattern("Open Cargo"));
						if (shipState.Maneuver != ShipManeuverType.Approach)
							return task.With(cacheWreck.GetApproachTask());
						return task.With("Flying to the cache wreck");
					}
				}

				if (!offensiveOverviewEntries.IsNullOrEmpty())
					return task;

				// No conduit on grid means we are not standing in a cleared abyssal room at all —
				// e.g. still in known space next to the Abyssal Trace. Never dereference it blind.
				if (conduit == null)
					return task.With("No enemies and no conduit on grid — nothing to jump into");

				var closeInventoryTask = inventoryProvider.GetCloseWindowTask();
				if (closeInventoryTask != null)
					return task.With(closeInventoryTask);

				// The gate is the last quiet moment of the room: never carry a partial magazine into
				// the next spawn (operator's rule). Issued once per room; the reload runs while we fly.
				if (!reloadIssued)
				{
					reloadIssued = true;
					var topUpTask = shipState.GetReloadTask();
					if (topUpTask != null)
						return task.With("Topping the launchers up before taking the gate").With(topUpTask);
				}

				// "Activate Gate" is a one-shot command: the ship then flies to the conduit by itself.
				// Clicking it every tick only re-opens the menu and interrupts the approach, so after
				// the click we simply wait out the flight.
				if (gateActivatedAt.HasValue &&
				    StateStopwatch.Elapsed - gateActivatedAt.Value < TimeSpan.FromSeconds(GateApproachWaitSeconds))
					return task.With("Gate activated — flying to the conduit, waiting");

				// Gather on the conduit: approach until inside the gather range.
				if (conduit.Distance > GateGatherRangeM)
				{
					gateWaitStartedAt = null;
					FleetGateBoard.Report(bot.Pid, conduit.Distance, ready: false);
					if (shipState.Maneuver != ShipManeuverType.Approach)
						return task.With($"Room clear — flying to the conduit ({conduit.Distance} m)")
							.With(conduit.GetApproachTask());
					return task.With($"Room clear — approaching the conduit ({conduit.Distance} m)");
				}

				// Wait for the rest of the fleet, then everybody jumps at once. A ship that never gets
				// here (dead, autopilot off, solo run) cannot hold us forever: the wait times out.
				FleetGateBoard.Report(bot.Pid, conduit.Distance, ready: true);
				gateWaitStartedAt ??= StateStopwatch.Elapsed;
				var waited = StateStopwatch.Elapsed - gateWaitStartedAt.Value;
				var fleetReady = FleetGateBoard.AllReady(out var fleetDetail);
				if (!fleetReady && waited < TimeSpan.FromSeconds(GateSyncTimeoutSeconds))
					return task.With($"At the conduit — waiting for the fleet ({fleetDetail}; {waited.TotalSeconds:F0} s)");

				task.With(fleetReady
					? $"Fleet gathered at the conduit ({fleetDetail}) — taking the gate"
					: $"Fleet not complete after {waited.TotalSeconds:F0} s ({fleetDetail}) — taking the gate anyway");

				// Operator's path: left-click the conduit, then press Activate Gate on the Selected
				// Item panel — no right-click menu. The menu stays as a fallback for clients/panels
				// where the button is not exposed. The select click alone is not the activation, so
				// the room bookkeeping below only happens once the panel actually shows the conduit.
				var panelGateTask = shipState.GetSelectedItemActionTask(conduit, "activate|jump");
				if (panelGateTask != null && !shipState.SelectedItemPanelShows(conduit))
					return task.With("Selecting the conduit for the Activate Gate button").With(panelGateTask);

				if (!changingRoom)
				{
					changingRoom = true;
					stats?.AdvanceRoom();
				}
				FleetGateBoard.Forget(bot.Pid);
				gateWaitStartedAt = null;
				roomIndex++;
				gateActivatedAt = StateStopwatch.Elapsed;
				return task.With(panelGateTask ?? conduit.ClickMenuEntryByRegexPattern("Activate Gate"));
			}

			return task;
		}

		/// <summary>
		/// Whether this client is the one that picks loot up. The fleet role comes from the client
		/// config (tank / wing-1 / …); a solo runner has no role and always loots.
		/// </summary>
		internal static bool IsLooter(Bot bot) =>
			string.IsNullOrEmpty(bot.Role) ||
			bot.Role.Contains("tank", StringComparison.OrdinalIgnoreCase) ||
			bot.Role.Contains("lead", StringComparison.OrdinalIgnoreCase);

		/// <summary>How long we let the ship fly to the conduit after "Activate Gate" before re-issuing it.</summary>
		private const int GateApproachWaitSeconds = 25;

		/// <summary>Everybody must be this close to the conduit before the fleet takes the gate.</summary>
		private const int GateGatherRangeM = 3000;
		/// <summary>How long a ship at the conduit waits for the rest of the fleet before jumping alone.</summary>
		private const int GateSyncTimeoutSeconds = 60;

		private bool enteringAbyss;
		private bool changingRoom;
		private TimeSpan? gateActivatedAt;
		/// <summary>The looter has emptied this room's cache wreck (the empty wreck stays on grid).</summary>
		private bool cacheLooted;
		/// <summary>The pre-gate top-up reload was issued in this room.</summary>
		private bool reloadIssued;
		/// <summary>When we arrived at the conduit and started waiting for the fleet.</summary>
		private TimeSpan? gateWaitStartedAt;
		/// <summary>Which room we have already called the fleet regroup in (once per room).</summary>
		private int regroupCalledInRoom = -1;
		/// <summary>Rooms taken so far this run; bumped when the gate is activated.</summary>
		private int roomIndex;
		private TimeSpan? lastActivateClickAt;

		private ISerializableBotTask EnterAbyssIfNeeded([NotNull] Bot bot, [NotNull] IShipState shipState,
			[NotNull] IInventoryProvider inventoryProvider)
		{
			// Frigate filaments show "Activate for fleet"; cruisers show "Activate". Prefer the fleet
			// variant, fall back to the solo one.
			var activateTask = shipState.GetPopupButtonTask("Activate for fleet")
			                   ?? shipState.GetPopupButtonTask("Activate");
			if (activateTask != null)
			{
				// Throttle: click the activation button at most once every few seconds. Re-clicking it
				// every tick — while the ship is still transitioning into the pocket, or the button is
				// briefly disabled or covered by another window — is exactly how stray clicks land and
				// break things. One click, then wait for the client to consume it.
				if (lastActivateClickAt is { } prev && StateStopwatch.Elapsed - prev < TimeSpan.FromSeconds(4))
					return new DynamicTask().With("Filament activation clicked — waiting for it to take.");
				lastActivateClickAt = StateStopwatch.Elapsed;
				enteringAbyss = true;
				return activateTask;
			}

			if (enteringAbyss)
				return null;

			// Bullet-proof pre-entry: pull any drones still in space back to the bay first, so we
			// never enter (or leave a room) with drones out. Once recalled they count toward readiness.
			var recallTask = shipState.Drones.GetDronesReturnTasks().FirstOrDefault();
			if (recallTask != null)
				return new DynamicTask()
					.With("Recalling drones to bay before entering abyss.")
					.With(recallTask);

			var openTask = inventoryProvider.GetOpenWindowTask();
			if (openTask != null)
				return openTask; // open the inventory (also lets us read cargo for the readiness gate)

			// Bullet-proof: if cargo is in icon view (names unreadable), switch it to List view first.
			if (inventoryProvider is MemoryProxyInventoryProvider invForView)
			{
				var switchTask = invForView.GetSwitchToListViewTaskIfNeeded();
				if (switchTask != null)
					return new DynamicTask()
						.With("Switching inventory to List view so cargo names are readable.")
						.With(switchTask);
			}

			// Readiness gate: do NOT commit to a run under-equipped. Verify ammo/consumables in cargo
			// and a full drone complement all in the bay before activating the filament.
			var problems = AbyssReadinessProblems(bot, inventoryProvider);
			if (problems.Count > 0)
				return new DynamicTask().With("NOT READY — not entering abyss: " + string.Join("; ", problems));

			return inventoryProvider.GetActvateItemIfPresentTask(profile.FilamentName, "Use .*");
		}

		/// <summary>
		/// Pre-entry readiness: full drone complement all in the bay (none still in space), and each
		/// required cargo item (ammo/paste/filament) present in the cargo hold. Returns the list of
		/// problems — empty means ready to enter.
		/// </summary>
		private List<string> AbyssReadinessProblems([NotNull] Bot bot, [NotNull] IInventoryProvider inventoryProvider)
		{
			var problems = new List<string>();
			var mem = bot.MemoryMeasurementAtTime?.Value;

			// Drones: full complement AND all in the bay (none in space). A droneless fit (Hawk) has no
			// drone bay at all, so demanding the window would deadlock the gate.
			var dv = mem?.WindowDroneView;
			if (dv == null)
			{
				if (profile.DronesNeeded > 0)
					problems.Add("drone window closed — open it to verify drones");
			}
			else
			{
				var bay = dv.DroneGroupInBay?.Header?.MainText?.CountFromDroneGroupCaption()
				          ?? dv.DroneGroupInBay?.Children?.Count ?? 0;
				var inSpace = dv.DroneGroupInSpace?.Header?.MainText?.CountFromDroneGroupCaption()
				              ?? dv.DroneGroupInSpace?.Children?.Count ?? 0;
				if (profile.DronesNeeded > 0 && bay < profile.DronesNeeded)
					problems.Add($"drones {bay}/{profile.DronesNeeded} in bay");
				if (inSpace > 0)
					problems.Add($"{inSpace} drone(s) still in space — recall before entering");
			}

			// Cargo: required ammo/consumables present (drone-bay items are checked above).
			if (inventoryProvider is MemoryProxyInventoryProvider inv)
				foreach (var c in inv.CheckCargo(profile.RequiredCargo))
					if (!c.Ok)
						problems.Add($"{c.Item}: {c.Detail}");

			return problems;
		}


		public IBotTask GetStateExitActions(Bot bot)
		{
			throw new NotImplementedException();
		}

		public bool MoveToNext { get; }
	}
}