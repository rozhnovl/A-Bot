using System;
using System.Linq;
using Bib3;
using BotEngine.Common;
using Sanderling.ABot.Parse;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Parse;
using IMemoryMeasurement = Sanderling.Parse.IMemoryMeasurement;

namespace Sanderling.ABot.Bot
{
	public class DronesContoller
	{
		private int droneInBayCount;
		private int droneInLocalSpaceCount;
		private string[] droneInLocalSpaceSetStatus;

		public DronesContoller(IMemoryMeasurement memoryMeasurement)
		{
			var droneListView = memoryMeasurement?.WindowDroneView?.ListView;

			var droneGroupWithNameMatchingPattern = new Func<string, DroneViewEntryGroup>(namePattern =>
				droneListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(group =>
					group?.LabelTextLargest()?.Text?.RegexMatchSuccessIgnoreCase(namePattern) ?? false));

			var droneGroupInBay = droneGroupWithNameMatchingPattern("bay");
			var droneGroupInLocalSpace = droneGroupWithNameMatchingPattern("local space");

			droneInBayCount = droneGroupInBay?.Caption?.Text?.CountFromDroneGroupCaption() ?? 0;
			droneInLocalSpaceCount = droneGroupInLocalSpace?.Caption?.Text?.CountFromDroneGroupCaption() ?? 0;

			//	assuming that local space is bottommost group.
			var setDroneInLocalSpace =
				droneListView?.Entry?.OfType<DroneViewEntryItem>()
					?.Where(drone => droneGroupInLocalSpace?.RegionCenter()?.B < drone?.RegionCenter()?.B)
					?.ToArray();

			droneInLocalSpaceSetStatus =
				setDroneInLocalSpace
					?.Select(drone => drone?.LabelText?.Select(label => label?.Text?.StatusStringFromDroneEntryText()))
					?.ConcatNullable()?.WhereNotDefault()?.Distinct()?.ToArray();
		}

		public bool ShouldLaunch => 0 < droneInBayCount && droneInLocalSpaceCount < 2;

		public bool HasNonReturningDronesInSpace => 0 < droneInLocalSpaceCount
		                                            && !(droneInLocalSpaceSetStatus?.Any(
			                                                 droneStatus =>
				                                                 droneStatus.RegexMatchSuccessIgnoreCase(
					                                                 "Returning")) ??
		                                                 false);

	}
}