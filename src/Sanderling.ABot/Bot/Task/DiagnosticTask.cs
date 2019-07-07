using System.Collections.Generic;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Task
{
	public class DiagnosticTask : IBotTask
	{
		public DiagnosticTask(string message)
		{
			MessageText = message;
		}
		public IEnumerable<IBotTask> Component => null;

		public IEnumerable<MotionParam> ClientActions => null;

		public string MessageText;
	}
}
