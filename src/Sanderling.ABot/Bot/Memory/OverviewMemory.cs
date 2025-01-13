using Bib3;
using BotEngine.Interface;
using Sanderling.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Memory
{
	public class OverviewMemory
	{
		private readonly IDictionary<Int64, HashSet<EWarTypeEnum>> setEWarTypeFromOverviewEntryId = new Dictionary<Int64, HashSet<EWarTypeEnum>>();

		public IEnumerable<EWarTypeEnum> SetEWarTypeFromOverviewEntry(Interface.MemoryStruct.IOverviewEntry entry) =>
			setEWarTypeFromOverviewEntryId?.TryGetValueOrDefault(entry?.Id ?? -1);

		private static readonly IEnumerable<ShipManeuverType> setManeuverReset =
			new[] { ShipManeuverType.Warp, ShipManeuverType.Docked, ShipManeuverType.Jump };

		public void Aggregate(FromProcessMeasurement<Sanderling.Parse.IMemoryMeasurement> memoryMeasurementAtTime)
		{
			var memoryMeasurement = memoryMeasurementAtTime?.Value;

			var overviewWindow = memoryMeasurement?.WindowOverview?.FirstOrDefault();

			foreach (var overviewEntry in (overviewWindow?.Entries?.WhereNotDefault()).EmptyIfNull())
			{
				var setEWarType = setEWarTypeFromOverviewEntryId.TryGetValueOrDefault(overviewEntry.Id);

				/*TODO foreach (var ewarType in overviewEntry.EWarType.EmptyIfNull())
				{
					if (null == setEWarType)
						setEWarType = new HashSet<EWarTypeEnum>();

					setEWarType.Add(ewarType);
				}*/

				if (null != setEWarType)
					setEWarTypeFromOverviewEntryId[overviewEntry.Id] = setEWarType;
			}

			if (setManeuverReset.Contains(memoryMeasurement?.ShipUi?.Indication?.ManeuverType ?? ShipManeuverType.None))
			{
				var setOverviewEntryVisibleId = overviewWindow?.Entries?.Select(entry => entry.Id)?.ToArray();

				foreach (var entryToRemoveId in setEWarTypeFromOverviewEntryId.Keys.Where(entryId => !(setOverviewEntryVisibleId?.Contains(entryId) ?? false)).ToArray())
					setEWarTypeFromOverviewEntryId.Remove(entryToRemoveId);
			}
		}
	}
}
