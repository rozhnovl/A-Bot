using System;
using System.Collections.Generic;
using Sanderling.Motor;
using System.Linq;
using Bib3.Geometrik;
using WindowsInput.Native;

namespace Sanderling.ABot.Bot.Task
{
	public class HotkeyTask : ISerializableBotTask
	{
		private readonly VirtualKeyCode[] modifiers;

		public HotkeyTask(VirtualKeyCode key, params VirtualKeyCode[] modifiers)
		{
			this.modifiers = modifiers;
			Key = key;
		}

		private VirtualKeyCode Key { get; }
		public IEnumerable<IBotTask> Component => null;
		public IEnumerable<MotionRecommendation> ClientActions
		{
			get
			{
				foreach (var m in modifiers)
					yield return m.KeyDown().AsRecommendation();
				yield return Key.KeyboardPress().AsRecommendation();
				foreach (var m in modifiers.Reverse())
					yield return m.KeyUp().AsRecommendation();
			}
		}

		public string ToJson() => ToString();

		public override string ToString()
		{
			return $"{nameof(HotkeyTask)}[{Key}<{String.Join("+", modifiers.Select(m => m.ToString()))}>]";
		}
	}
}
