using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace Sanderling.ABot.Bot.Configuration
{
	internal static class FitsRegistry
	{
		public static ShipFit Gila(Bot bot) =>
			new ShipFit(bot.MemoryMeasurementAtTime?.Value?.ShipUi,
				new[]
				{
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon, VirtualKeyCode.F1),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster, VirtualKeyCode.F2),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster, VirtualKeyCode.F3),
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc)
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener, VirtualKeyCode.CONTROL,
							VirtualKeyCode.F1),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener, VirtualKeyCode.CONTROL,
							VirtualKeyCode.F2),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.MWD, VirtualKeyCode.CONTROL, VirtualKeyCode.F3),
					}
				})
			{
				MaxDronesInSpace = 2,
			};

		public static ShipFit Hawk(Bot bot) =>
			new ShipFit(bot.MemoryMeasurementAtTime.Value?.ShipUi,
				new[]
				{
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon, VirtualKeyCode.F1),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon, VirtualKeyCode.F2),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster, VirtualKeyCode.F3),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.MWD, VirtualKeyCode.F4),
					},
					new ShipFit.ModuleInfo[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc)
					},
					new ShipFit.ModuleInfo[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc)
						//new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener, VirtualKeyCode.CONTROL,
						//	VirtualKeyCode.F1),
						//new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener, VirtualKeyCode.CONTROL,
						//	VirtualKeyCode.F2),
						//new ShipFit.ModuleInfo(ShipFit.ModuleType.MWD, VirtualKeyCode.CONTROL, VirtualKeyCode.F3),
					}
				})
			{
				MaxTargetingRange = 20000,
				MaxTargets = 4,
			};

	}
}
