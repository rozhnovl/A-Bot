using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;
using Sanderling.ABot.Bot.Configuration;

namespace Sanderling.ABot.Bot.Configuration
{
	internal static class FitsRegistry
	{
		private static ShipFitConfiguration? _config;

		private static ShipFitConfiguration Config => _config ??= ConfigurationLoader.LoadConfigurationsStandalone().ShipFits;

		public static ShipFit Gila(Bot bot) => GetShipFit(bot, "Gila");

		public static ShipFit Hawk(Bot bot) => GetShipFit(bot, "Hawk");

		private static ShipFit GetShipFit(Bot bot, string shipName)
		{
			if (!Config.Ships.TryGetValue(shipName, out var shipConfig))
			{
				throw new InvalidOperationException($"Ship configuration for '{shipName}' not found");
			}

			var fitInfo = new[]
			{
				shipConfig.HighSlots.Select(ConvertToModuleInfo).ToArray(),
				shipConfig.MidSlots.Select(ConvertToModuleInfo).ToArray(),
				shipConfig.LowSlots.Select(ConvertToModuleInfo).ToArray()
			};

			return new ShipFit(bot.MemoryMeasurementAtTime?.Value?.ShipUi, fitInfo)
			{
				MaxTargetingRange = shipConfig.MaxTargetingRange,
				MaxTargets = shipConfig.MaxTargets,
				MaxDronesInSpace = shipConfig.MaxDronesInSpace
			};
		}

		private static ShipFit.ModuleInfo ConvertToModuleInfo(ModuleSlotConfiguration slotConfig)
		{
			var moduleType = ParseModuleType(slotConfig.ModuleType);
			var hotkeys = ParseHotkey(slotConfig.Hotkey);

			var moduleInfo = new ShipFit.ModuleInfo(moduleType, hotkeys)
			{
				OptimalRange = slotConfig.OptimalRange
			};

			return moduleInfo;
		}

		private static ShipFit.ModuleType ParseModuleType(string moduleType)
		{
			return moduleType switch
			{
				"Hardener" => ShipFit.ModuleType.Hardener,
				"Weapon" => ShipFit.ModuleType.Weapon,
				"ShieldBooster" => ShipFit.ModuleType.ShieldBooster,
				"MWD" => ShipFit.ModuleType.MWD,
				_ => ShipFit.ModuleType.Etc
			};
		}

		private static VirtualKeyCode[] ParseHotkey(string? hotkey)
		{
			if (string.IsNullOrWhiteSpace(hotkey))
				return Array.Empty<VirtualKeyCode>();

			var keys = hotkey.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var virtualKeys = new List<VirtualKeyCode>();

			foreach (var key in keys)
			{
				if (Enum.TryParse<VirtualKeyCode>(key, ignoreCase: true, out var virtualKey))
				{
					virtualKeys.Add(virtualKey);
				}
			}

			return virtualKeys.ToArray();
		}
	}
}
