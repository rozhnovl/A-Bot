using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using WindowsInput.Native;
using Bib3;
using BotEngine.Motor;
using Sanderling.Accumulation;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Task
{
	public class ModuleToggleTask : ISerializableBotTask
	{
		public readonly IEnumerable<ShipUIModuleButton> modules;
		private VirtualKeyCode[][] hotKey;

		public IEnumerable<IBotTask> Component { get; }

		public IEnumerable<MotionRecommendation> ClientActions
		{
			get
			{
				var toggleKey = hotKey;//TODO?? module?.TooltipLast?.Value?.ToggleKey;

				if (0 < toggleKey?.Length)
				{

					foreach (var c in toggleKey)
					{
						yield return c?.KeyboardPressCombined().AsRecommendation();
					}
				}
				else
				{
					foreach (var c in modules.Select(m => m.UINode).ClickWithModifier().ClientActions)
					{
						yield return c;
					};
				}
			}
		}

		public string ToJson()
		{
			return
				$"{nameof(ModuleToggleTask)}[{(hotKey != null ? string.Join("+", hotKey.Select(h => h.ToString())) : string.Empty /*TODO module.TooltipLast.Value.LabelText.FirstOrDefault()?.Text)*/)}]";
		}

		public ModuleToggleTask([NotNull] ShipUIModuleButton module)
		{
			this.modules = [module];
		}

		public ModuleToggleTask([NotNull] ShipFit.ModuleInfo module, VirtualKeyCode? modifier)
		{
			this.modules = [module.UiModule];
			this.hotKey = [module.HotKey.NullIfEmpty()];
			if (modifier != null)
				hotKey = [new[] { modifier.Value }.Concat(module.HotKey.NullIfEmpty()).ToArray()];
		}


		public ModuleToggleTask([NotNull] ShipFit.ModuleInfo[] modules, VirtualKeyCode? modifier)
		{
			this.modules = modules.Select(m=>m.UiModule).ToArray();
			if (modifier != null)
			{
				hotKey = modules.Select(m => new[] { modifier.Value }.Concat(m.HotKey).ToArray()).ToArray().NullIfEmpty();;
			}
			else
			{
				this.hotKey = modules.Select(m => m.HotKey).ToArray().NullIfEmpty();
			}
		}

		public override string ToString()
		{
			return
				$"{nameof(ModuleToggleTask)}[{(hotKey != null ? string.Join("+", hotKey.Select(h => h.ToString())) : modules.ToString())}";
		}
	}
}