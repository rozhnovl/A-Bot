using System.Linq;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class WarpToBookmarkInSystemState : IStragegyState
	{
		private readonly string bookmarkName;
		private bool arrived;
		public WarpToBookmarkInSystemState(string bookmarkName)
		{
			this.bookmarkName = bookmarkName;
		}

		public IBotTask GetStateActions(Bot bot)
		{
			var memory = bot.MemoryMeasurementAtTime.Value;
			var ManeuverType = memory?.ShipUi?.Indication?.ManeuverType;

			if (ShipManeuverType.Warp == ManeuverType ||
			    ShipManeuverType.Jump == ManeuverType)
				return null; //	do nothing while warping or jumping.


			var undockTask = new UndockTask(bot.MemoryMeasurementAtTime.Value);
			if (undockTask.ClientActions.Any())
				return undockTask;

			if (memory?.Menu == null)
				return memory.InfoPanelCurrentSystem.ListSurroundingsButton.ClickTask();

			if (memory.Menu.Count() > 1 && memory.Menu.Skip(1).FirstOrDefault().Entry.Any(e => e.Text == "Approach Location"))
				arrived = true;
				
			return memory.InfoPanelCurrentSystem.ListSurroundingsButton.ClickMenuEntryByRegexPattern(bot, bookmarkName,
				"Warp to Location Within .*");
		}
		
		public IBotTask GetStateExitActions(Bot bot) => null;

		public bool MoveToNext => arrived;
	}
}