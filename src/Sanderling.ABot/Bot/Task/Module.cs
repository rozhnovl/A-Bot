using BotEngine.Motor;
using Sanderling.Accumulation;
using Sanderling.Motor;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Sanderling.ABot.Bot.Task
{
	public static class ModuleTaskExtension
	{
		public static bool? IsActive(
			this IShipUiModule module,
			Bot bot)
		{
			if (bot?.MouseClickLastAgeStepCountFromUIElement(module) <= 1)
				return null;

			if (bot?.ToggleLastAgeStepCountFromModule(module) <= 1)
				return null;

			return module?.RampActive;
		}

		static public IBotTask EnsureIsActive(
			this Bot bot,
			IShipUiModule module)
		{
			if (module?.IsActive(bot) ?? true)
				return null;

			return new ModuleToggleTask(bot, module);
		}

		static public IBotTask EnsureIsActive(
			this Bot bot,
			IEnumerable<IShipUiModule> setModule) =>
			new BotTask { Component = setModule?.Select(module => bot?.EnsureIsActive(module)) };
	}

	public class ModuleToggleTask : SimpleBotTask
	{
		public readonly IShipUiModule module;
		
		public override IEnumerable<MotionParam> Effects
		{
			get
			{
				var toggleKey = module?.TooltipLast?.Value?.ToggleKey;

				if (0 < toggleKey?.Length)
					yield return toggleKey?.KeyboardPressCombined();

				yield return module?.MouseClick(MouseButtonIdEnum.Left);
			}
		}

		public ModuleToggleTask(Bot bot, [NotNull] IShipUiModule module) : base(bot)
		{
			this.module = module;
		}
	}
}
