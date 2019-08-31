using System.Collections.Generic;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Task
{
	public abstract class SimpleBotTask : IBotTask
	{
		protected readonly Bot bot;

		protected SimpleBotTask(Bot bot)
		{
			this.bot = bot;
		}

		public virtual IEnumerable<IBotTask> Component { get; } = null;
		public virtual IEnumerable<MotionRecommendation> ClientActions { get; } = null;
		
	}
}