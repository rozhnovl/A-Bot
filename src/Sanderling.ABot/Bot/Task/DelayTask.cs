using System.Collections.Generic;
using System.Threading;
using WindowsInput.Native;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Task
{
	public class DelayTask : IBotTask
	{
		private const int delayMs = 350;
		private VirtualKeyCode Key { get; }
		public IEnumerable<IBotTask> Component => null;
		public IEnumerable<MotionParam> ClientActions
		{
			get
			{
				Thread.Sleep(delayMs);
				yield break;
			}
		}
	}
}