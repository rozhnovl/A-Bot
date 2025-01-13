using WindowsInput.Native;
using Sanderling.ABot.Parse;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot.Task
{
	public class CombatTask : IBotTask
	{
		private readonly Bot bot;
		private readonly ShipFit shipFit;
		private readonly DronesContoller dronesController;

		public bool Completed { private set; get; }

		public CombatTask(Bot bot, ShipFit shipFit, DronesContoller dronesController)
		{
			this.bot = bot;
			this.shipFit = shipFit;
			this.dronesController = dronesController;
		}

		public IEnumerable<IBotTask> Component
		{
			get
			{
				var shipState = new ShipState(shipFit, bot);
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

				var listOverviewEntryToAttack =
					memoryMeasurement?.WindowOverview?.FirstOrDefault()?.Entries
						?.Where(entry => entry?.IconSpriteColorPercent?.IsRed() ?? false)
						?.Where(e => e.ObjectDistanceInMeters <= shipFit.MaxTargetingRange)
						?.OrderBy(entry => bot.AttackPriorityIndex(entry))
						?.ThenBy(entry => entry?.ObjectDistanceInMeters ?? int.MaxValue)
						?.ToArray()
					?? [];
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

				var overviewEntryLockTargets = listOverviewEntryToAttack?
					.Where(entry =>
						!(entry?.CommonIndications.Targeting == true ||
						  entry?.CommonIndications.TargetedByMe == true));

				if (overviewEntryLockTargets.Any() &&
				    (memoryMeasurement?.Target?.Length ?? 0) < shipFit.MaxTargets)
				{
					yield return overviewEntryLockTargets
						.Take(shipFit.MaxTargets - (memoryMeasurement?.Target?.Length ?? 0))
						.Select(e => e.UiElement).ClickWithModifier(VirtualKeyCode.CONTROL);
				}

				if (listOverviewEntryToAttack?.Length == 0)
				{
					Completed = true;
				}

				if (!(0 < listOverviewEntryToAttack?.Length))
					if (dronesController.droneInLocalSpaceCount == 0)
						Completed = true;
					else
					{
						foreach (var t in dronesController.GetDronesReturnTasks())
							yield return t;
					}
			}
		}

		public IEnumerable<MotionRecommendation> ClientActions => null;
	}
}