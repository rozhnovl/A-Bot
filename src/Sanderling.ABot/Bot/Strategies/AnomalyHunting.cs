using System.Collections.Generic;
using System.Linq;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class AnomalyHunting : IStrategy
	{
		public bool currentAnomalyLooted;
		public IEnumerable<IBotTask> GetTasks(Bot bot)
		{
			//yield return new BotTask { Component = EnumerateConfigDiagnostics() };
			yield return new UndockTask(bot.MemoryMeasurementAtTime?.Value);
			yield return new EnableInfoPanelCurrentSystem { MemoryMeasurement = bot.MemoryMeasurementAtTime?.Value };

			var saveShipTask = new SaveShipTask { Bot = bot };

			yield return saveShipTask;
			/*
			var shipFit = new ShipFit(bot.MemoryMeasurementAccu?.ShipUiModule,
				new[]
				{
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon),
						//new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						//new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						//new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						//new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster)
					}
				});

			yield return bot.EnsureIsActive(shipFit.GetAlwaysActiveModules().Select(m => m.UiModule));

			//var moduleUnknown = MemoryMeasurementAccu?.ShipUiModule?.FirstOrDefault(module => null == module?.TooltipLast?.Value);

			//yield return new BotTask { ClientActions = new[] { moduleUnknown?.MouseMove() } };

			if (!saveShipTask.AllowRoam)
				yield break;


			var combatTask = new CombatTask { bot = bot };

			yield return combatTask;

			if (!saveShipTask.AllowAnomalyEnter)
				yield break;

			yield return new UndockTask(bot.MemoryMeasurementAtTime?.Value);

			if (combatTask.Completed)
			{
				if (!currentAnomalyLooted)
				{
					var lootTask = new LootTask(bot);
					yield return lootTask;
					if (!lootTask.HasWreckToLoot)
						currentAnomalyLooted = true;
				}
				yield return new AnomalyEnter { bot = bot };
			}*/
		}
	}
}
