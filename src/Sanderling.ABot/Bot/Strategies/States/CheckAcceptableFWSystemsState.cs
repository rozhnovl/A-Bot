using System;
using System.Linq;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class CheckAcceptableFWSystemsState : IStragegyState
	{
		public string[] SystemsToSkip = null;

		public IBotTask GetStateActions(Bot bot)
		{
			var fwWindow =
				bot.MemoryMeasurementAtTime?.Value?.WindowOther?.FirstOrDefault(w => w.Caption == "Factional Warfare");
			if (fwWindow == null)
				return
					((Sanderling.Interface.MemoryStruct.WindowStation)
						((Sanderling.Parse.MemoryMeasurement)
							(bot.MemoryMeasurementAtTime?.Value)).WindowStation[0]).ServiceButton
					.Single(sb => sb.TexturePath.Contains("warfare")).ClickTask();
            //TODO Here we rely on fact that all enemy systems have low level and fit in single Screen. Also, systems list is sorted correctly
			var enemySystems =
				fwWindow.LabelText.Where(lt => lt.Text.Contains("color") && lt.Text.Contains("0xFFC74232"))
					.Select(lt => new String(lt.Text.Skip(18).TakeWhile(c => c != '<').ToArray()));
			if (enemySystems.Any())
				SystemsToSkip = enemySystems.ToArray();

			return null;
		}

		public IBotTask GetStateExitActions(Bot bot)
		{
			var fwWindow =
				bot.MemoryMeasurementAtTime?.Value?.WindowOther?.FirstOrDefault(w => w.Caption == "Factional Warfare");
			if (fwWindow != null)
				return fwWindow.ClickMenuEntryByRegexPattern(bot, ".*Close");
			return null;
		}

		public bool MoveToNext => SystemsToSkip!=null;
	}
}