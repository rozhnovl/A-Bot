using System.Linq;
using Bib3.Geometrik;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Strategies
{
	class TravelState : IStragegyState
	{
		private bool arrived;

		public IBotTask GetStateActions(Bot bot)
		{
			var memory = bot.MemoryMeasurementAtTime.Value;
			var ManeuverType = memory?.ShipUi?.Indication?.ManeuverType;

			if (ShipManeuverTypeEnum.Warp == ManeuverType ||
			    ShipManeuverTypeEnum.Jump == ManeuverType)
				return null;  //	do nothing while warping or jumping.

			//	from the set of route element markers in the Info Panel pick the one that represents the next Waypoint/System.
			//	We assume this is the one which is nearest to the topleft corner of the Screen which is at (0,0)
			var RouteElementMarkerNext =
				memory?.InfoPanelRoute?.RouteElementMarker
					?.OrderByCenterDistanceToPoint(new Vektor2DInt(0, 0))?.FirstOrDefault();

			if (RouteElementMarkerNext!=null)
			{
				var undockTask = new UndockTask(bot.MemoryMeasurementAtTime.Value);
				if (undockTask.ClientActions.Any())
					return undockTask;
			}
			else
			{
				if ((memory?.IsDocked ?? false))
					arrived = true;
				return null;
			}
			
			return RouteElementMarkerNext.ClickMenuEntryByRegexPattern(bot, "Dock|Jump.*");
		}

		public IBotTask GetStateExitActions(Bot bot)
		{
			return null;
		}

		public bool MoveToNext => arrived;
	}
}