using Sanderling.ABot.Bot.Configuration;
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
			var shipFit = FitsRegistry.Hawk(bot);

			yield return bot.EnsureIsActive(shipFit.GetAlwaysActiveModules());

			//var moduleUnknown = MemoryMeasurementAccu?.ShipUiModule?.FirstOrDefault(module => null == module?.TooltipLast?.Value);

			//yield return new BotTask { ClientActions = new[] { moduleUnknown?.MouseMove() } };

			if (!saveShipTask.AllowRoam)
				yield break;


			var combatTask = new CombatTask(bot, shipFit, new DronesContoller(bot.MemoryMeasurementAtTime.Value, shipFit));

			yield return combatTask;

			if (!saveShipTask.AllowAnomalyEnter)
				yield break;

			yield return new UndockTask(bot.MemoryMeasurementAtTime?.Value);

			if (combatTask.Completed)
			{
				//if (!currentAnomalyLooted)
				//{
				//	var lootTask = new LootTask(bot);
				//	yield return lootTask;
				//	if (!lootTask.HasWreckToLoot)
				//		currentAnomalyLooted = true;
				//}
				yield return new AnomalyEnter { bot = bot };
			}
		}
	}
}
