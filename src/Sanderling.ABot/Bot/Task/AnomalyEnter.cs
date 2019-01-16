using System.Collections.Generic;
using System.Linq;
using Sanderling.Motor;
using Sanderling.Parse;
using BotEngine.Common;
using BotEngine.Interface;
using Sanderling.ABot.Parse;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Task
{
	public class AnomalyEnter : IBotTask
	{
		public const string NoSuitableAnomalyFoundDiagnosticMessage = "no suitable anomaly found. waiting for anomaly to appear.";

		public Bot bot;

		public static bool AnomalySuitableGeneral(Interface.MemoryStruct.IListEntry scanResult) =>
			scanResult?.CellValueFromColumnHeader("Group")
				?.RegexMatchSuccessIgnoreCase("combat") ?? false;

		public IEnumerable<IBotTask> Component
		{
			get
			{
				var memoryMeasurement = bot?.MemoryMeasurementAtTime?.Value;

				if (!memoryMeasurement.ManeuverStartPossible())
					yield break;

				var scanResultCombatSite =
					memoryMeasurement?.WindowProbeScanner?.FirstOrDefault()
						?.ScanResultView?.Entry?
						.FirstOrDefault(AnomalySuitableGeneral);

				if (null == scanResultCombatSite)
					yield return new DiagnosticTask
					{
						MessageText = NoSuitableAnomalyFoundDiagnosticMessage,
					};

				if (null != scanResultCombatSite)
					yield return scanResultCombatSite.ClickMenuEntryByRegexPattern(bot, 
						ParseStatic.MenuEntryWarpToAtLeafRegexPattern);
			}
		}

		public IEnumerable<MotionParam> Effects => null;
	}
}
