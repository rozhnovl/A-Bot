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
				?.ToList();
			var listOverviewEntryToAttack = offensiveOverviewEntries
				?.Where(entry => entry.Distance <= shipFit.MaxTargetingRange)
				?.Where(entry => entry.Name != "Vila Swarmer")
				?.OrderBy(npcInfoProvider.CalcTargetPriority)
				?.ThenBy(entry => entry?.Distance ?? int.MaxValue)
				?.ToArray()
				??[];
			return listOverviewEntryToAttack;
		}
	}
	public class CombatTask(Bot bot, ShipFit shipFit, DronesContoller dronesController, PriorityManager priorityManager) : IBotTask
	{
		private readonly Bot bot;
		private readonly ShipFit shipFit;
		private readonly DronesContoller dronesController;
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
				yield return new DiagnosticTask($"Current maneuver is {shipState.Maneuver}." +
				                                Environment.NewLine +
				                                $"Incoming DPS is {estimatedIncomingDps}.");
				var tankingTask = shipState.GetNextTankingModulesTask(estimatedIncomingDps);

				if (tankingTask != null)
				{
					yield return tankingTask;
				}
				//TODO if (listOverviewEntryToAttack.Any())
				//Bot.currentAnomalyLooted = false;
				var targetSelected = shipState.ActiveTargets.ActiveTarget;

				var shouldAttackTarget =
					true; //TODO listOverviewEntryToAttack?.Any(entry => entry?.CommonIndications.TargetedByMe ?? false) ?? false;

				if (null != targetSelected)
					if (shouldAttackTarget)
					{
						var attackTask = shipState.GetAttackTasks();
						if (attackTask != null)
						{
							yield return attackTask;
						}
					}
					else
						yield return targetSelected.GetUnlockTask();

				yield return
					new DiagnosticTask(
				$"Spare enemies to attack: {listOverviewEntryToAttack.Length}: {string.Concat(listOverviewEntryToAttack.Select(oe => Environment.NewLine + '\t' + (oe.MeTargeted == true ? "[Targeted]" : string.Empty) + oe.Name))}");

				foreach (var overviewEntry in listOverviewEntryToAttack.Where(e =>
						         e.CommonIndications.Targeting != true && e.CommonIndications.TargetedByMe != true)
					         .Take(shipState.Fit.MaxTargets - shipState.ActiveTargets.Count))
				{
					if (lastTargetingAttemptSteps.ContainsKey(overviewEntry.Id))
						if (lastTargetingAttemptSteps[overviewEntry.Id] > stepIndex - 3)
							continue;
					lastTargetingAttemptSteps[overviewEntry.Id] = stepIndex;
					yield return overviewEntry.GetSelectTask();
				}

				if (listOverviewEntryToAttack.Length == 0)
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