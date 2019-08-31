using Sanderling.Motor;
using System.Collections.Generic;

namespace Sanderling.ABot.Bot
{
	public class BotTask : ISerializableBotTask
	{
		private readonly string description;

		public BotTask(string description)
		{
			this.description = description;
		}
		public IEnumerable<IBotTask> Component { set; get; }

		public IEnumerable<MotionRecommendation> ClientActions { set; get; }
		public string ToJson() => description;
	}
}
