using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using WindowsInput.Native;
using Bib3;
using BotEngine.Motor;
using Sanderling.Accumulation;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Task
{
	public class ModuleToggleTask : ISerializableBotTask
	{
		public readonly Sanderling.Accumulation.IShipUiModule module;
		private VirtualKeyCode[] hotKey;

		public IEnumerable<IBotTask> Component { get; }

		public IEnumerable<MotionRecommendation> ClientActions
		{
			get
			{
				var toggleKey = hotKey?? module?.TooltipLast?.Value?.ToggleKey;

				if (0 < toggleKey?.Length)
					yield return toggleKey?.KeyboardPressCombined().AsRecommendation();
				else
					yield return module?.MouseClick(MouseButtonIdEnum.Left).AsRecommendation();
			}
		}

		public string ToJson()
		{
			return
				$"{nameof(ModuleToggleTask)}[{(hotKey != null ? string.Join("+", hotKey.Select(h => h.ToString())) : module.TooltipLast.Value.LabelText.FirstOrDefault()?.Text)}]";
		}

		public ModuleToggleTask([NotNull] Sanderling.Accumulation.IShipUiModule module)
		{
			this.module = module;
		}

		public ModuleToggleTask([NotNull] ShipFit.ModuleInfo module, VirtualKeyCode? modifier)
		{
			this.module = module.UiModule;
			this.hotKey = module.HotKey.NullIfEmpty();
			if (modifier != null)
				hotKey = new[] {modifier.Value}.Concat(hotKey).ToArray();
		}

		public override string ToString()
		{
			return
				$"{nameof(ModuleToggleTask)}[{(hotKey != null ? string.Join("+", hotKey.Select(h => h.ToString())) : module.ToString())}";
		}
	}
}