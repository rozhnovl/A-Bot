using System.Collections.Generic;
using System.Linq;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot
{
	public class ActiveTargetsContoller
	{
		public ActiveTargetsContoller(Bot bot, IMemoryMeasurement memoryMeasurement)
		{
			List = memoryMeasurement.Target?.Select(t => new SimpleTargetInfo(bot, t))?.ToList<ITarget>();

			var at = memoryMeasurement?.Target?.FirstOrDefault(target => target?.IsSelected ?? false);
			if (at != null)
			{
				ActiveTarget = new SimpleTargetInfo(bot, at);
			}
		}

		public List<ITarget> List { get; }
		public int Count => List?.Count??0;

		public ITarget? ActiveTarget { get; }
	}
}