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
			if (module is null || module.AppearsActive || module.IsActive is null)
				return null;

			return new ModuleToggleTask(module);
		}

		static public IBotTask EnsureIsInactive(
			this Bot bot,
			ShipUIModuleButton module)
		{
			if (module?.AppearsActive != true)
				return null;
			return new ModuleToggleTask(module);
		}

		static public IBotTask EnsureIsActive(
			this Bot bot,
			IEnumerable<ShipFit.ModuleInfo> setModule){

			var notActiveModules = setModule.Where(m => m.UiModule != null && !m.UiModule.AppearsActive && m.UiModule.IsActive != null).ToArray();
			return !notActiveModules.Any() ? null : new BotTask(nameof(EnsureIsActive) + " for list of modules")
			{
				Component = [new ModuleToggleTask(notActiveModules, null),]
			};
		}
	}
}