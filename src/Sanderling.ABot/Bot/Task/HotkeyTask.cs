using System.Collections.Generic;
using Sanderling.Motor;
using System.Linq;
using Bib3.Geometrik;
using WindowsInput.Native;

namespace Sanderling.ABot.Bot.Task
{
	public class HotkeyTask : IBotTask
	{
		private readonly VirtualKeyCode[] modifiers;

		public HotkeyTask(VirtualKeyCode key, params VirtualKeyCode[] modifiers)
		{
			this.modifiers = modifiers;
			Key = key;
		}

		private VirtualKeyCode Key { get; }
		public IEnumerable<IBotTask> Component => null;
		public IEnumerable<MotionParam> ClientActions
		{
			get
			{
				foreach (var m in modifiers)
					yield return m.KeyDown();
				yield return Key.KeyboardPress();
				foreach (var m in modifiers.Reverse())
					yield return m.KeyUp();
			}
		}
	}
}
