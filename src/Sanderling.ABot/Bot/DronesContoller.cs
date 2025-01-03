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
			var droneListView = memoryMeasurement?.WindowDroneView?.DroneGroups;


			droneInBayCount = memoryMeasurement?.WindowDroneView.DroneGroupInBay?.Header?.MainText
				?.CountFromDroneGroupCaption() ?? 0;
			droneInLocalSpaceCount = memoryMeasurement?.WindowDroneView.DroneGroupInSpace?.Header?.MainText
				?.CountFromDroneGroupCaption() ?? 0;

			droneInLocalSpaceSetStatus =
				memoryMeasurement?.WindowDroneView.DroneGroupInSpace?.Children
					//TODO double check
					?.Select(drone => drone?.Entry?.MainText?.StatusStringFromDroneEntryText())
					?.WhereNotDefault()?.Distinct()?.ToArray();
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