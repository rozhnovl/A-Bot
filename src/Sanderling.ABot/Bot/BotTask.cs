using Sanderling.Motor;
using System.Collections.Generic;

namespace Sanderling.ABot.Bot
{
	public class BotTask : IBotTask
	{
		public IEnumerable<IBotTask> Component { set; get; }

		public IEnumerable<MotionParam> ClientActions { set; get; }
	}
}
