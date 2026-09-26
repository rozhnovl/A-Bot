using WindowsInput.Native;
using Sanderling.ABot.Parse;
using Sanderling.Parse;
using Sanderling.ABot.Bot.Strategies;
using System.Threading.Tasks;

namespace Sanderling.ABot.Bot.Task
{
	public class PriorityManager(ShipFit shipFit, NpcInfoProvider npcInfoProvider)
	{
		public IOverviewEntry[] GetEnemies(IOverviewProvider overview)
		{
			var offensiveOverviewEntries = overview.Entries
				?.Where(entry => entry.IsEnemy)
				?.Where(entry => !entry.Name.Contains("Extraction"))
				?.Where(e => e.Type != "Vila Swarmer")
				?.Where(e => !NpcInfoProvider.IsAbyssalCache(e))
				?.ToList();
			// Room-aware ordering: how valuable a webber/neuter is depends on the room's total DPS.
			var priorityOf = npcInfoProvider.TargetPriorityScorer(overview);
			var listOverviewEntryToAttack = offensiveOverviewEntries
				?.Where(entry => entry.Distance <= shipFit.MaxTargetingRange)
				?.Where(entry => entry.Name != "Vila Swarmer")
				?.OrderBy(priorityOf)
				?.ThenBy(entry => entry?.Distance ?? int.MaxValue)
				?.ToArray()
				??[];
			return listOverviewEntryToAttack;
		}
	}
	public class CombatTask(Bot bot, ShipFit shipFit, DronesContoller dronesController, PriorityManager priorityManager) : IBotTask
	{
		// NOTE: do NOT re-declare bot/shipFit/dronesController as fields here — that shadows the primary
		// constructor parameters with uninitialized (null) fields, which NRE'd on the first combat tick.
		private readonly NpcInfoProvider npcInfoProvider = new NpcInfoProvider();

		public bool Completed { private set; get; }

		private Dictionary<long, int> lastTargetingAttemptSteps = new Dictionary<long, int>();
		private int stepIndex = 0;

		public IEnumerable<IBotTask> Component
		{
			get
			{
				stepIndex++;
				var shipState = new ShipState(shipFit, bot);
				var overviewProvider = new MemoryProxyOverviewProvider(bot);
				var inventoryProvider = new MemoryProxyInventoryProvider(bot);
				var memoryMeasurementAtTime = bot?.MemoryMeasurementAtTime;

				var memoryMeasurement = memoryMeasurementAtTime?.Value;

				if (!memoryMeasurement.ManeuverStartPossible())
					yield break;

				/*var OverviewTabActive = memoryMeasurement?.WindowOverview?.FirstOrDefault()?.PresetTab
					?.OrderByDescending(tab => tab?.LabelColorOpacityMilli ?? 1500)?.FirstOrDefault();
				var OverviewTabCombat = memoryMeasurement?.WindowOverview?.FirstOrDefault()?.PresetTab
					?.Where(tab => tab?.Label.Text.RegexMatchSuccess(Config.CombatTabName) ?? false)
					.FirstOrDefault();

				// switch tabs for wrecks
				if (OverviewTabCombat != OverviewTabActive)
					yield return OverviewTabCombat.ClickTask();
				*/

				var listOverviewEntryToAttack = priorityManager.GetEnemies(overviewProvider);
				var estimatedIncomingDps = npcInfoProvider.CalculateApproximateDps(overviewProvider);
				var enemyNeutGjPerSec = npcInfoProvider.CalculateNeutPressure(overviewProvider);
				yield return new DiagnosticTask($"Current maneuver is {shipState.Maneuver}." +
				                                Environment.NewLine +
				                                $"Incoming DPS is {estimatedIncomingDps}, neut pressure {enemyNeutGjPerSec} GJ/s.");
				var tankingTask = shipState.GetNextTankingModulesTask(estimatedIncomingDps, enemyNeutGjPerSec);

				if (tankingTask != null)
				{
					yield return tankingTask;
				}

				// Ammo comes before target selection: a reload costs ~10 s of DPS, so it must happen while
				// the kill queue says so, not after we have committed to a target. The queue is the
				// priority-ordered enemy list, so the planner sees the same order we will shoot in.
				var ammoTask = AmmoController.GetSwitchTask(bot, shipFit, listOverviewEntryToAttack, out var ammoReason);
				if (ammoTask != null)
				{
					yield return new DiagnosticTask($"Ammo swap — {ammoReason}.");
					yield return ammoTask;
				}

				var targetSelected = shipState.ActiveTargets.ActiveTarget;
				var selectedName = targetSelected?.Name ?? "";
				var cache = overviewProvider.Entries?.FirstOrDefault(NpcInfoProvider.IsAbyssalCache);
				var candidates = listOverviewEntryToAttack.AsEnumerable();
				if (cache != null && cache.Distance <= shipFit.MaxTargetingRange)
					candidates = candidates.Append(cache);
				var candidateList = candidates.ToList();

				var allowed = new List<IOverviewEntry>();
				foreach (var e in candidateList)
				{
					if (allowed.Count >= shipState.Fit.MaxTargets) break;
					// Loot wrecks are never combat targets — they are cargo (operator, 2026-09-18).
					if (NpcInfoProvider.IsWreckName(e.Name) || NpcInfoProvider.IsWreckName(e.Type))
						continue;
					var key = FleetFireBoard.TargetKey(e, candidateList);
					var max = FleetFireBoard.MaxShooters(e, overviewProvider, shipFit.VolleyDamage, npcInfoProvider, key);
					if (FleetFireBoard.ShouldShoot(bot.Pid, key, max))
						allowed.Add(e);
				}

				// Publish what WE can see of the active target's remaining HP, so the other windows know it
				// is nearly dead and go spend their volleys elsewhere instead of overkilling it.
				if (targetSelected != null)
				{
					var onGrid = candidateList.FirstOrDefault(e => FleetFireBoard.NamesMatch(selectedName, e.Name));
					var remainingFraction = targetSelected.RemainingHitpointsFraction;
					if (onGrid != null && remainingFraction is double fraction)
					{
						var fullEhp = npcInfoProvider.EhpFor(onGrid);
						if (fullEhp is long full)
							FleetFireBoard.ReportRemainingEhp(
								FleetFireBoard.TargetKey(onGrid, candidateList), (long)(full * fraction));
					}
				}

				var selectedAllowed = targetSelected != null &&
					allowed.Any(e => FleetFireBoard.NamesMatch(selectedName, e.Name));

				// FRIENDLY FIRE GUARD (observed live 2026-09-18: a wing opened up on a fleet mate).
				// Locks are clicks on overview ROWS, and the overview re-sorts by distance every tick,
				// so a row can move under the cursor between perceive and click — and fleet mates sit
				// in the top rows while the rats are 50 km out. Whatever ends up locked, we only ever
				// shoot something the overview currently calls an enemy: positive identification, not
				// "not on the blocked list".
				var selectedIsFriendly = targetSelected != null &&
					(overviewProvider.Entries ?? new List<IOverviewEntry>())
						.Any(e => !e.IsEnemy && FleetFireBoard.NamesMatch(selectedName, e.Name));
				var selectedIsEnemy = targetSelected != null && !selectedIsFriendly &&
					(overviewProvider.Entries ?? new List<IOverviewEntry>())
						.Any(e => e.IsEnemy && FleetFireBoard.NamesMatch(selectedName, e.Name));

				if (selectedIsFriendly)
					yield return new DiagnosticTask($"FRIENDLY LOCKED: {selectedName} — unlocking, holding fire.");

				var selectedBlocked = targetSelected != null && !selectedAllowed &&
					(selectedIsFriendly ||
					 NpcInfoProvider.IsWreckName(selectedName) ||
					 candidateList.Any(e => FleetFireBoard.NamesMatch(selectedName, e.Name)));
				if (selectedBlocked)
					yield return targetSelected!.GetUnlockTask();

				// Fire only at a target the overview positively calls an enemy (or one the fire board
				// already handed us): never at a friendly, never at something we cannot identify.
				if (targetSelected != null && !selectedBlocked && (selectedAllowed || selectedIsEnemy))
				{
					foreach (var droneTask in dronesController.GetDronesAttackTasks(targetSelected))
						yield return droneTask;

					var attackTask = shipState.GetAttackTasks();
					if (attackTask != null)
						yield return attackTask;

					// The secondary gun is short-ranged (the Hawk's rail reaches ~4 km): run it only while
					// the target is actually inside that envelope, and switch it off again beyond it.
					var secondaryRange = shipFit.GetAllByType(ShipFit.ModuleType.SecondaryWeapon)
						.Select(m => m.OptimalRange).DefaultIfEmpty(0).Max();
					if (secondaryRange > 0)
					{
						var secondaryTask = shipState.GetSetModuleActiveTask(
							ShipFit.ModuleType.SecondaryWeapon, targetSelected.Distance <= secondaryRange);
						if (secondaryTask != null)
							yield return secondaryTask;
					}
				}

				yield return
					new DiagnosticTask(
				$"Spare enemies to attack: {allowed.Count}/{candidateList.Count}: {string.Concat(allowed.Select(oe => Environment.NewLine + '\t' + (oe.MeTargeted == true ? "[Targeted]" : string.Empty) + oe.Name))}");

				var toLock = new List<IOverviewEntry>();
				foreach (var overviewEntry in allowed.Where(e =>
						         e.CommonIndications.Targeting != true && e.CommonIndications.TargetedByMe != true)
					         .Take(shipState.Fit.MaxTargets - shipState.ActiveTargets.Count))
				{
					if (lastTargetingAttemptSteps.ContainsKey(overviewEntry.Id))
						if (lastTargetingAttemptSteps[overviewEntry.Id] > stepIndex - 3)
							continue;
					lastTargetingAttemptSteps[overviewEntry.Id] = stepIndex;
					toLock.Add(overviewEntry);
				}

				if (toLock.Count > 0)
				{
					// Lock the whole batch in one ctrl-held pass (ctrl down → row, row, row → ctrl up)
					// instead of one lock per tick: fewer mouse trips and a much shorter window in which
					// the distance-sorted overview can shuffle a row under the cursor.
					var lockElements = toLock.Select(e => e.SelectElement).Where(e => e != null).ToArray();
					if (lockElements.Length == toLock.Count && lockElements.Length > 1)
						yield return lockElements.ClickWithModifier(VirtualKeyCode.CONTROL);
					else
						foreach (var overviewEntry in toLock)
							yield return overviewEntry.GetSelectTask();
				}

				if (listOverviewEntryToAttack.Length == 0 && cache == null)
				{
					var reloadTask = shipState.GetReloadTask(); 
					if (reloadTask != null)
						yield return reloadTask;
					foreach (var t in dronesController.GetDronesReturnTasks())
						yield return t;
					if (dronesController.droneInLocalSpaceCount == 0)
						Completed = true;
				}
			}
		}

		public IEnumerable<MotionRecommendation> ClientActions => null;
	}
}