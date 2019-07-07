using System.Collections.Generic;

namespace Sanderling.ABot.Bot
{
	internal interface IStrategy
	{
		IEnumerable<IBotTask> GetTasks(Bot bot);
	}
}