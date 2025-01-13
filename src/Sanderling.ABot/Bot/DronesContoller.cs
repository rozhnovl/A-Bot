using Bib3;
using BotEngine.Common;
using Sanderling.ABot.Bot.Task;
using Sanderling.ABot.Parse;
using IMemoryMeasurement = Sanderling.Parse.IMemoryMeasurement;

namespace Sanderling.ABot.Bot
{
	public class DronesContoller
	{
		private int droneInBayCount;
		public int droneInLocalSpaceCount { get; private set; }
		private string[] droneInLocalSpaceSetStatus;
		private ShipFit shipFit;

		public DronesContoller(IMemoryMeasurement memoryMeasurement, ShipFit shipFit)
		{
			this.shipFit = shipFit;
			var droneListView = memoryMeasurement?.WindowDroneView?.DroneGroups;

			droneInBayCount = memoryMeasurement?.WindowDroneView?.DroneGroupInBay?.Header?.MainText
				?.CountFromDroneGroupCaption() ?? 0;
			droneInLocalSpaceCount = memoryMeasurement?.WindowDroneView?.DroneGroupInSpace?.Header?.MainText
				?.CountFromDroneGroupCaption() ?? 0;

			droneInLocalSpaceSetStatus =
				memoryMeasurement?.WindowDroneView?.DroneGroupInSpace?.Children
					//TODO double check
					?.Select(drone => drone?.Entry?.MainText?.StatusStringFromDroneEntryText())
					?.WhereNotDefault()?.Distinct()?.ToArray();
		}

		public bool ShouldLaunch => 0 < droneInBayCount && droneInLocalSpaceCount < shipFit.MaxDronesInSpace;

		public bool HasNonReturningDronesInSpace => 0 < droneInLocalSpaceCount
		                                            && !(droneInLocalSpaceSetStatus?.Any(
			                                                 droneStatus =>
				                                                 droneStatus.RegexMatchSuccessIgnoreCase(
					                                                 "Returning")) ??
		                                                 false);

		public bool HasIdleDronesInSpace => 0 < droneInLocalSpaceCount
		                                    && !(droneInLocalSpaceSetStatus?.Any(
			                                         droneStatus =>
				                                         droneStatus.RegexMatchSuccessIgnoreCase(
					                                         "Idle")) ??
		                                         false);

		public IEnumerable<ISerializableBotTask> GetDronesAttackTasks(ITarget target)
		{

			var shouldDronesAttack =
				!target.DroneAssigned && target.Distance <= 55000;
			if (!shouldDronesAttack)
				yield break;

			if (ShouldLaunch)
				yield return HotkeyRegistry.LaunchDrones;

			if (HasIdleDronesInSpace)
				yield return HotkeyRegistry.EngageDrones;
		}

		public IEnumerable<ISerializableBotTask> GetDronesReturnTasks()
		{
			if (HasNonReturningDronesInSpace)
				yield return HotkeyRegistry.ReturnDrones;
		}
	}
}