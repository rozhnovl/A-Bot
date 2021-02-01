using System.Collections.Generic;
using System.Linq;
using Bib3;
using Sanderling.Motor;
using Sanderling.Parse;
using BotEngine.Common;
using Sanderling.ABot.Parse;

namespace Sanderling.ABot.Bot.Task
{
	public class AnomalyEnter : IBotTask
	{
		public const string NoSuitableAnomalyFoundDiagnosticMessage = "no suitable anomaly found. waiting for anomaly to appear.";

		public Bot bot;
		private static string[] TargetAnomalies = new string[] { ".* Refuge" , ".* Hideaway" };

		public static bool AnomalySuitableGeneral(Interface.MemoryStruct.IListEntry scanResult) =>
			TargetAnomalies != null
				? TargetAnomalies.Any(taName =>
					scanResult.CellValueFromColumnHeader("Name")?.RegexMatchSuccessIgnoreCase(taName) ?? false)
				: scanResult?.CellValueFromColumnHeader("Group") 
					  ?.RegexMatchSuccessIgnoreCase("combat") ?? false;

		public IEnumerable<IBotTask> Component
		{
			get
			{
				var memoryMeasurement = bot?.MemoryMeasurementAtTime?.Value;

				if (!memoryMeasurement.WindowTelecom.IsNullOrEmpty())
				{
					yield return memoryMeasurement.WindowTelecom.Single()
						.ButtonText?.FirstOrDefault(text => text.Text.RegexMatchSuccessIgnoreCase("Close"))
						.ClickWithModifier(bot, null);
				}
				if (!memoryMeasurement.ManeuverStartPossible())
					yield break;

				var scanResultCombatSite =
					memoryMeasurement?.WindowProbeScanner?.FirstOrDefault()
						?.ScanResultView?.Entry?
						.FirstOrDefault(AnomalySuitableGeneral);

				if (null == scanResultCombatSite)
					yield return new DiagnosticTask(NoSuitableAnomalyFoundDiagnosticMessage);

				if (null != scanResultCombatSite)
					yield return scanResultCombatSite.ClickMenuEntryByRegexPattern(bot, 
						ParseStatic.MenuEntryWarpToAtLeafRegexPattern);

			}
		}

		public IEnumerable<MotionRecommendation> ClientActions => null;
	}
}
