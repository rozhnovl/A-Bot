using System.Collections.Generic;
using System.Linq;
using Sanderling.ABot.Bot.Configuration;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Strategies
{
	/// <summary>
	/// A minimal, non-abyss combat behavior for validating ship control on a belt with a
	/// single weak rat. It runs the exact requested sequence, one action per step:
	///   1. lock the nearest enemy,
	///   2. orbit it at 10 km,
	///   3. once the range has opened to >= 5 km, launch and engage drones,
	///   4. keep the missile launcher active.
	/// When no enemy remains, it recalls the drones. It is intentionally independent of the
	/// abyss state machine (no filament/conduit logic), so it can be driven on any grid.
	/// </summary>
	internal class BeltTestStrategy : IStrategy
	{
		private const int OrbitRangeMeters = 10000;
		private const int DroneDeployRangeMeters = 5000;

		private readonly RunProfile profile;

		public BeltTestStrategy(RunProfile profile)
		{
			this.profile = profile;
		}

		public IEnumerable<IBotTask> GetTasks(Bot bot)
		{
			var fit = profile.BuildFit(bot);
			var shipState = new ShipState(fit, bot);

			if (!shipState.ManeuverStartPossible)
			{
				yield return new DiagnosticTask("Belt test: maneuver not possible (docked / aligning / no ship in space).");
				yield break;
			}

			var overview = new MemoryProxyOverviewProvider(bot);
			var enemies = overview.Entries?.Where(e => e.IsEnemy).OrderBy(e => e.Distance).ToList()
			              ?? new List<IOverviewEntry>();
			var enemy = enemies.FirstOrDefault();
			var target = shipState.ActiveTargets.ActiveTarget ?? shipState.ActiveTargets.List?.FirstOrDefault();

			yield return new DiagnosticTask(
				$"Belt test: enemies={enemies.Count}, locked={shipState.ActiveTargets.Count}, maneuver={shipState.Maneuver}" +
				(target != null ? $", target '{target.Name}' @ {target.Distance}m" : string.Empty));

			// Nothing to fight: bring drones home and idle.
			if (enemy == null && target == null)
			{
				foreach (var t in shipState.Drones.GetDronesReturnTasks())
				{
					yield return new DiagnosticTask("No enemy — recalling drones.");
					yield return t;
					yield break;
				}
				yield return new DiagnosticTask("No enemy on overview — idle.");
				yield break;
			}

			// 1) Lock the nearest enemy.
			if (target == null)
			{
				if (enemy != null && !enemy.MeTargeted && enemy.CommonIndications?.Targeting != true)
				{
					yield return new DiagnosticTask($"Locking '{enemy.Name}' @ {enemy.Distance}m.");
					yield return enemy.GetSelectTask();
				}
				else
				{
					yield return new DiagnosticTask("Lock in progress — waiting.");
				}
				yield break;
			}

			// 2) Establish a 10 km orbit on the enemy.
			if (shipState.Maneuver != ShipManeuverType.Orbit && enemy != null)
			{
				yield return new DiagnosticTask($"Orbiting '{enemy.Name}' at {OrbitRangeMeters:N0} m.");
				yield return enemy.ClickMenuEntryByRegexPattern("Orbit.*", "10,000 m");
				yield break;
			}

			// 3) Once we've opened to >= 5 km, launch and engage drones.
			if (target.Distance >= DroneDeployRangeMeters)
			{
				var droneTasks = shipState.Drones.GetDronesAttackTasks(target).ToList();
				if (droneTasks.Count > 0)
				{
					yield return new DiagnosticTask($"Range {target.Distance}m >= {DroneDeployRangeMeters}m — deploying/engaging drones.");
					foreach (var t in droneTasks)
						yield return t;
					yield break;
				}
			}
			else
			{
				yield return new DiagnosticTask($"Range {target.Distance}m < {DroneDeployRangeMeters}m — holding drones until 5 km.");
			}

			// 4) Keep the missile launcher firing.
			var weaponTask = shipState.GetSetModuleActiveTask(ShipFit.ModuleType.Weapon, true);
			if (weaponTask != null)
			{
				yield return new DiagnosticTask("Activating missile launcher.");
				yield return weaponTask;
			}
		}
	}
}
