using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;
using Bib3;
using BotEngine.Common;
using BotEngine.Motor;
using Sanderling.ABot.Parse;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot.Task
{
	public class CombatTask : IBotTask
	{
		private const int TargetCountMax = 4;

		public Bot bot;

		public bool Completed { private set; get; }

		public IEnumerable<IBotTask> Component
		{
			get
			{
				var memoryMeasurementAtTime = bot?.MemoryMeasurementAtTime;
				var memoryMeasurementAccu = bot?.MemoryMeasurementAccu;

				var memoryMeasurement = memoryMeasurementAtTime?.Value;

				if (!memoryMeasurement.ManeuverStartPossible())
					yield break;
				throw new NotImplementedException();
				/*
				var OverviewTabActive = memoryMeasurement?.WindowOverview?.FirstOrDefault()?.PresetTab
					?.OrderByDescending(tab => tab?.LabelColorOpacityMilli ?? 1500)?.FirstOrDefault();
				var OverviewTabCombat = memoryMeasurement?.WindowOverview?.FirstOrDefault()?.PresetTab
					?.Where(tab => tab?.Label.Text.RegexMatchSuccess(Config.CombatTabName) ?? false)
					.FirstOrDefault();

				// switch tabs for wrecks
				if (OverviewTabCombat != OverviewTabActive)
					yield return OverviewTabCombat.ClickTask();


				var listOverviewEntryToAttack =
					memoryMeasurement?.WindowOverview?.FirstOrDefault()?.Entries?.Where(entry => entry?.MainIcon?.Color?.IsRed() ?? false)
						?.OrderBy(entry => bot.AttackPriorityIndex(entry))
						?.ThenBy(entry => entry?.DistanceMax ?? int.MaxValue)
						?.ToArray();
				//TODO if (listOverviewEntryToAttack.Any())
				//Bot.currentAnomalyLooted = false;
				var targetSelected =
					memoryMeasurement?.Target?.FirstOrDefault(target => target?.IsSelected ?? false);

				var shouldAttackTarget =
					listOverviewEntryToAttack?.Any(entry => entry?.MeActiveTarget ?? false) ?? false;

				var setModuleWeapon =
					memoryMeasurementAccu?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? false);

				if (null != targetSelected)
					if (shouldAttackTarget)
						//yield return new HotkeyTask(VirtualKeyCode.F1);
						yield return bot.EnsureIsActive(setModuleWeapon);
					else
						yield return targetSelected.ClickMenuEntryByRegexPattern(bot, "unlock");

				var droneListView = memoryMeasurement?.WindowDroneView?.FirstOrDefault()?.ListView;

				var droneGroupWithNameMatchingPattern = new Func<string, DroneViewEntryGroup>(namePattern =>
					droneListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(group => group?.LabelTextLargest()?.Text?.RegexMatchSuccessIgnoreCase(namePattern) ?? false));

				var droneGroupInBay = droneGroupWithNameMatchingPattern("bay");
				var droneGroupInLocalSpace = droneGroupWithNameMatchingPattern("local space");

				var droneInBayCount = droneGroupInBay?.Caption?.Text?.CountFromDroneGroupCaption();
				var droneInLocalSpaceCount = droneGroupInLocalSpace?.Caption?.Text?.CountFromDroneGroupCaption();

				//	assuming that local space is bottommost group.
				var setDroneInLocalSpace =
					droneListView?.Entry?.OfType<DroneViewEntryItem>()
						?.Where(drone => droneGroupInLocalSpace?.RegionCenter()?.B < drone?.RegionCenter()?.B)
						?.ToArray();

				var droneInLocalSpaceSetStatus =
					setDroneInLocalSpace?.Select(drone => drone?.LabelText?.Select(label => label?.Text?.StatusStringFromDroneEntryText()))?.ConcatNullable()?.WhereNotDefault()?.Distinct()?.ToArray();

				var droneInLocalSpaceIdle =
					droneInLocalSpaceSetStatus?.Any(droneStatus => droneStatus.RegexMatchSuccessIgnoreCase("idle")) ?? false;

				if (shouldAttackTarget)
				{
					if (0 < droneInBayCount && droneInLocalSpaceCount < 4)
						yield return HotkeyRegistry.LaunchDrones;

					if (droneInLocalSpaceIdle)
						yield return HotkeyRegistry.EngageDrones;
				}

				var overviewEntryLockTarget =
					listOverviewEntryToAttack?.FirstOrDefault(entry => !((entry?.MeTargeted ?? false) || (entry?.MeTargeting ?? false)));

				if (null != overviewEntryLockTarget && !(TargetCountMax <= memoryMeasurement?.Target?.Length))
					yield return overviewEntryLockTarget.ClickWithModifier(bot, VirtualKeyCode.CONTROL);


				if (!(0 < listOverviewEntryToAttack?.Length))
					if (0 < droneInLocalSpaceCount)
					{
						if (!(droneInLocalSpaceSetStatus?.Any(droneStatus => droneStatus.RegexMatchSuccessIgnoreCase("Returning")) ?? false))
							yield return HotkeyRegistry.ReturnDrones;
					}
					else
						Completed = true;
			}*/
			}
		}

		public IEnumerable<MotionRecommendation> ClientActions => null;
	}
}