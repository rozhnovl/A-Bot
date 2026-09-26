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
			// Name the slot and type so the log shows WHICH module was clicked (2026-09-27: the bot was
			// seen clicking a passive BCS and the log only said "ModuleToggleTask[]").
			var slots = string.Join(",", modules.Select(m => $"{m?.Rack}{m?.SlotIndex}:{m?.ModuleInfo?.ModuleId}"));
			var keys = hotKey != null ? string.Join("+", hotKey.Select(h => h.ToString())) : string.Empty;
			return $"{nameof(ModuleToggleTask)}[{slots}{(keys.Length > 0 ? " " + keys : "")}]";
		}

		public ModuleToggleTask([NotNull] ShipUIModuleButton module)
		{
			this.modules = [module];
		}

		public ModuleToggleTask([NotNull] ShipFit.ModuleInfo module, VirtualKeyCode? modifier)
		{
			this.modules = [module.UiModule];
			// When the module has no hotkey and no modifier, leave hotKey null so activation
			// falls back to clicking the module button — that keeps a fit independent of the
			// player's custom keybinds. A [null] entry here would instead try to press a null key.
			var moduleHotKey = module.HotKey.NullIfEmpty();
			this.hotKey = moduleHotKey == null ? null : [moduleHotKey];
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