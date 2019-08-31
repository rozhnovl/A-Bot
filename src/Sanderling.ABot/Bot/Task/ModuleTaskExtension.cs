using Sanderling.Accumulation;
using System.Collections.Generic;
using System.Linq;

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

		public static bool IsReloading(
			this IShipUiModule module,
			Bot bot)
		{
			return !module.IsActive() && module.RampRotationMilli.HasValue && module.RampRotationMilli.Value > 0;
		}

		static public IBotTask EnsureIsActive(
			this Bot bot,
			IShipUiModule module)
		{
			if (module?.IsActive(bot) ?? true)
				return null;

			return new ModuleToggleTask(module);
		}

		static public IBotTask EnsureIsInactive(
			this Bot bot,
			IShipUiModule module)
		{
			if (module?.IsActive(bot) ?? false)

				return new ModuleToggleTask(module);
			return null;
		}

		static public IBotTask EnsureIsActive(
			this Bot bot,
			IEnumerable<IShipUiModule> setModule) =>
			new BotTask(nameof(EnsureIsActive) + " for list of modules")
				{Component = setModule?.Select(module => bot?.EnsureIsActive(module))};
	}
}
