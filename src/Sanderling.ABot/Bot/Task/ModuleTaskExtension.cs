using Sanderling.Accumulation;
using System.Collections.Generic;
using System.Linq;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Task
{
	public static class ModuleTaskExtension
	{
		public static bool IsReloading(
			this Sanderling.Accumulation.IShipUiModule module,
			Bot bot)
		{
			return !module.IsActive() && module.RampRotationMilli.HasValue && module.RampRotationMilli.Value > 0;
		}

		static public IBotTask EnsureIsActive(
			this Bot bot,
			ShipUIModuleButton module)
		{
			if (module?.IsActive ?? true)
				return null;

			return new ModuleToggleTask(module);
		}

		static public IBotTask EnsureIsInactive(
			this Bot bot,
			ShipUIModuleButton module)
		{
			if (module?.IsActive ?? false)

				return new ModuleToggleTask(module);
			return null;
		}

		static public IBotTask EnsureIsActive(
			this Bot bot,
			IEnumerable<ShipFit.ModuleInfo> setModule){

			var notActiveModules = setModule.Where(m=>m.UiModule?.IsActive !=true).ToArray();
			return !notActiveModules.Any() ? null : new BotTask(nameof(EnsureIsActive) + " for list of modules")
			{
				Component = [new ModuleToggleTask(notActiveModules, null),]
			};
		}
	}
}