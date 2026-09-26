using System;
using System.Collections.Generic;
using System.Linq;
using Sanderling.ABot.Bot.Configuration;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Strategies
{
	/// <summary>
	/// Content-agnostic combat core for on-grid PvE (combat anomalies, belts), split into the three
	/// cooperating modules the operator asked for and run in PRIORITY order each tick:
	///   1) SelfState   — survive: always-on hardeners, active tank vs. incoming DPS, DRONE PRESERVATION,
	///                    reload/recall when the grid is clear. (highest priority)
	///   2) Targeting   — who &amp; how to hit: lock priority enemies, focus-fire the active target with
	///                    drones (the drone-boat's main DPS) + weapons.
	///   3) Positioning — orbit / approach / keep-range; MWD only to close a big gap (not cap-stable).
	/// The first module with something to do wins the tick (one deliberate action per tick). This is the
	/// reusable core that later wraps into the abyss (which only adds filament entry + gate + the timer).
	///
	/// MVP: assumes the ship is already on an anomaly grid (operator warps in). Auto-find/warp via the
	/// probe scanner is a later addition.
	/// </summary>
	internal class AnomalyStrategy : IStrategy
	{
		private const int OrbitRangeMeters = 12000;      // drone/missile cruiser stand-off
		private const int MwdCloseGapMeters = 20000;     // MWD only to close big gaps (cap-hungry)

		private readonly RunProfile profile;
		private readonly NpcInfoProvider npc = new();
		private readonly Dictionary<long, int> lastLockAttemptStep = new();
		private int step;

		public AnomalyStrategy(RunProfile profile) => this.profile = profile;

		public IEnumerable<IBotTask> GetTasks(Bot bot)
		{
			step++;

			var measurement = bot.MemoryMeasurementAtTime?.Value;
			if (measurement?.ShipUi == null)
			{
				// Transient null (warp/loading) — wait, don't crash.
				yield return new DiagnosticTask("Waiting: no ship UI (docked / warping / loading).");
				yield break;
			}

			ShipFit fit = null;
			string fitError = null;
			try { fit = profile.BuildFit(bot); }
			catch (Exception e) { fitError = e.Message; }
			if (fit == null)
			{
				yield return new DiagnosticTask($"Waiting: fit '{profile.Name}' not mappable yet ({fitError}).");
				yield break;
			}

			var ship = new ShipState(fit, bot);
			if (!ship.ManeuverStartPossible)
			{
				yield return new DiagnosticTask("Waiting: maneuver not possible (aligning / warping / docked).");
				yield break;
			}

			var overview = new MemoryProxyOverviewProvider(bot);
			var enemies = new PriorityManager(fit, npc).GetEnemies(overview);
			var incomingDps = npc.CalculateApproximateDps(overview);
			var primary = enemies.FirstOrDefault();

			// Per-enemy bracket breakdown: R=attacking me (red), Y=locked me but not attacking (yellow ~ on
			// my drones in a solo run), P=warp-disrupting me, J=jamming me. This is the ground truth we need
			// to nail drone-preservation (pull drones off strong enemies that yellow-box them) — capture it
			// live before wiring the auto-recall.
			string Short(string n) => (n ?? "?").Length > 14 ? n.Substring(0, 14) : n ?? "?";
			var brackets = string.Join(" ", enemies.Take(6).Select(e =>
			{
				var ci = e.CommonIndications;
				var tag = ci?.AttackingMe == true ? "R" : ci?.Targeting == true ? "Y" : "-";
				var ew = (ci?.IsWarpDisruptingMe == true ? "P" : "") + (ci?.IsJammingMe == true ? "J" : "");
				return $"{Short(e.Name)}[{tag}{ew}]@{e.Distance}";
			}));

			yield return new DiagnosticTask(
				$"Anomaly[{step}]: enemies={enemies.Length} locked={ship.ActiveTargets.Count} " +
				$"maneuver={ship.Maneuver} inDps={incomingDps:F0} hardeners={(incomingDps > fit.PassiveRegenDps ? "ON" : "off")}(>{fit.PassiveRegenDps}) dronesInSpace={ship.Drones.droneInLocalSpaceCount}" +
				(primary != null ? $" primary='{primary.Name}'@{primary.Distance}m arch={npc.RangeArchetypeFor(primary)}" : "") +
				(brackets.Length > 0 ? $" | {brackets}" : ""));

			// Priority order: survive -> deal damage -> move. First module with an action wins the tick.
			foreach (var t in SelfState(ship, enemies, incomingDps)) { yield return t; yield break; }
			foreach (var t in Targeting(ship, enemies)) { yield return t; yield break; }
			foreach (var t in Positioning(ship, primary)) { yield return t; yield break; }

			yield return new DiagnosticTask("Grid clear / nothing to do.");
		}

		// ---- 1) SELF-STATE: survive -------------------------------------------------------------
		private IEnumerable<IBotTask> SelfState(ShipState ship, IOverviewEntry[] enemies, double incomingDps)
		{
			// Keep always-on utility (e.g. sensor booster) running.
			var alwaysOn = ship.GetSetModuleActiveTask(ShipFit.ModuleType.AlwaysOn, true);
			if (alwaysOn != null) { yield return alwaysOn; yield break; }

			// Adaptive resist: turn the hardeners ON only when the estimated incoming DPS would beat the fit's
			// passive regen — trivial spawns are tanked passively, hardeners fire only when a spawn actually
			// threatens us (and drop again when the grid clears / incomingDps falls back under the threshold).
			var wantHardeners = incomingDps > ship.Fit.PassiveRegenDps;
			var hardener = ship.GetSetModuleActiveTask(ShipFit.ModuleType.Hardener, wantHardeners);
			if (hardener != null) { yield return hardener; yield break; }

			// Active tank vs. incoming DPS (shield/armor booster cycling).
			var tank = ship.GetNextTankingModulesTask(incomingDps);
			if (tank != null) { yield return tank; yield break; }

			// Grid clear: recall drones and reload so we're ready to move on.
			if (enemies.Length == 0)
			{
				foreach (var recall in ship.Drones.GetDronesReturnTasks()) { yield return recall; yield break; }
				var reload = ship.GetReloadTask();
				if (reload != null) { yield return reload; yield break; }
			}

			// TODO drone preservation: pull drones off when a dangerous enemy is shooting THEM (yellow
			// bracket in a solo run) or a drone is low — needs drone HP + per-enemy bracket color/aggro.
		}

		// ---- 2) TARGETING: who & how to hit -----------------------------------------------------
		private IEnumerable<IBotTask> Targeting(ShipState ship, IOverviewEntry[] enemies)
		{
			if (enemies.Length == 0) yield break;

			var target = ship.ActiveTargets.ActiveTarget ?? ship.ActiveTargets.List?.FirstOrDefault();

			// Fire on the active target: drones (main DPS) focus-fire first, then keep the weapon on.
			if (target != null)
			{
				foreach (var droneTask in ship.Drones.GetDronesAttackTasks(target)) { yield return droneTask; yield break; }
				var attack = ship.GetAttackTasks();
				if (attack != null) { yield return attack; yield break; }
			}

			// Lock priority enemies up to the ship's max targets (don't hammer the same one every tick).
			foreach (var enemy in enemies
				         .Where(e => e.CommonIndications?.Targeting != true && e.CommonIndications?.TargetedByMe != true)
				         .Take(Math.Max(0, ship.Fit.MaxTargets - ship.ActiveTargets.Count)))
			{
				if (lastLockAttemptStep.TryGetValue(enemy.Id, out var last) && last > step - 3)
					continue;
				lastLockAttemptStep[enemy.Id] = step;
				yield return enemy.GetSelectTask();
				yield break;
			}
		}

		// ---- 3) POSITIONING: hold the right range for this primary ------------------------------
		// Range archetype comes from the target type (brawl / kite / standoff, see ABYSS-INTEL.md §4);
		// MWD is a manual burst only — never held, because the fits are NOT cap-stable and a lit MWD
		// blooms our signature (easier to hit). So: MWD ON only to close a big gap, OFF once in range.
		private IEnumerable<IBotTask> Positioning(ShipState ship, IOverviewEntry primary)
		{
			if (primary == null) yield break;

			// Fixed-orbit mode: boats that just sit on the spawn (e.g. a chain-damage Vorton BC) skip the
			// kite/standoff archetype entirely — MWD only to close a big gap, otherwise a tight orbit.
			if (ship.Fit.PreferredOrbitMeters > 0)
			{
				if (primary.Distance > MwdCloseGapMeters)
				{
					var mwdOn = ship.GetSetModuleActiveTask(ShipFit.ModuleType.MWD, true);
					if (mwdOn != null) { yield return mwdOn; yield break; }
				}
				else
				{
					var mwdOff = ship.GetSetModuleActiveTask(ShipFit.ModuleType.MWD, false);
					if (mwdOff != null) { yield return mwdOff; yield break; }
				}

				if (ship.Maneuver != ShipManeuverType.Orbit)
					yield return primary.ClickMenuEntryByRegexPattern("Orbit.*", $"{ship.Fit.PreferredOrbitMeters:N0} m");
				yield break;
			}

			var archetype = npc.RangeArchetypeFor(primary);
			var farToTarget = primary.Distance > ship.Fit.MaxTargetingRange;

			// --- MWD discipline ---
			if (farToTarget && primary.Distance > MwdCloseGapMeters)
			{
				// Big gap: burn in with the MWD.
				var mwdOn = ship.GetSetModuleActiveTask(ShipFit.ModuleType.MWD, true);
				if (mwdOn != null) { yield return mwdOn; yield break; }
			}
			else
			{
				// In (or nearly in) range: kill the MWD — sig bloom + cap. (No-op if already off.)
				var mwdOff = ship.GetSetModuleActiveTask(ShipFit.ModuleType.MWD, false);
				if (mwdOff != null) { yield return mwdOff; yield break; }
			}

			// --- establish the archetype maneuver once (don't re-issue every tick) ---
			switch (archetype)
			{
				case NpcInfoProvider.EngageRange.Brawl:
					if (ship.Maneuver != ShipManeuverType.Orbit)
						yield return primary.ClickMenuEntryByRegexPattern("Orbit.*", "500 m");
					break;

				case NpcInfoProvider.EngageRange.Standoff:
					// Keep our distance from a long-range BS; orbit is unstable that far out.
					if (ship.Maneuver != ShipManeuverType.KeepAtRange && ship.Maneuver != ShipManeuverType.Orbit)
						yield return primary.ClickMenuEntryByRegexPattern("Keep at range", "30,000 m");
					break;

				default: // Kite
					if (ship.Maneuver != ShipManeuverType.Orbit)
						yield return primary.ClickMenuEntryByRegexPattern("Orbit.*", $"{OrbitRangeMeters:N0} m");
					break;
			}
		}
	}
}
