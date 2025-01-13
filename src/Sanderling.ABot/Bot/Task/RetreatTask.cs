﻿using System.Collections.Generic;
using Sanderling.Motor;
using Sanderling.ABot.Parse;

namespace Sanderling.ABot.Bot.Task
{
	public class RetreatTask : IBotTask
	{
		public Bot Bot;

		public IEnumerable<IBotTask> Component
		{
			get
			{
				var memoryMeasurement = Bot?.MemoryMeasurementAtTime?.Value;

				if (!memoryMeasurement.ManeuverStartPossible())
					yield break;

				var retreatBookmark = Bot?.ConfigSerialAndStruct.Value?.RetreatBookmark;

				yield return new MenuPathTask
				{
					RootUIElement = memoryMeasurement?.InfoPanelContainer?.LocationInfo?.ListSurroundingsButtonElement,
					Bot = Bot,
					ListMenuListPriorityEntryRegexPattern = new[] { new[] { retreatBookmark }, new[] { @"dock", ParseStatic.MenuEntryWarpToAtLeafRegexPattern } },
				};
			}
		}

		public IEnumerable<MotionRecommendation> ClientActions => null;
	}
}
