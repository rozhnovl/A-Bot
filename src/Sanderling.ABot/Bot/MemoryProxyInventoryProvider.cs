using System.Linq;
using BotEngine.Common;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot
{
	public class MemoryProxyInventoryProvider : IInventoryProvider
	{
		private readonly Bot bot;
		private readonly Sanderling.Parse.IMemoryMeasurement memoryMeasurement;
		private IWindowInventory selectedWindowInventory;

		public MemoryProxyInventoryProvider(Bot bot, IWindowInventory selectedWindowInventory = null)
		{
			this.bot = bot;
			memoryMeasurement = bot.MemoryMeasurementAtTime.Value;
			this.selectedWindowInventory = selectedWindowInventory ?? memoryMeasurement?.WindowInventory?.FirstOrDefault();
		}

		public ISerializableBotTask GetCloseWindowTask()
		{
			if ((memoryMeasurement?.WindowInventory?.Any() ?? false))
				return memoryMeasurement.Neocom.InventoryButton.ClickTask();
			return null;
		}

		public ISerializableBotTask GetOpenWindowTask()
		{
			if (!(memoryMeasurement?.WindowInventory?.Any() ?? false))
				return memoryMeasurement.Neocom.InventoryButton.ClickTask();
			return null;
		}

		public ISerializableBotTask GetActvateItemIfPresentTask(string mobileTractorUnit, string launch)
		{
			var tractorInCargo = selectedWindowInventory.SelectedContainerInventory.ItemsView
				.FirstOrDefault(lt => lt.CellsTexts.ContainsValue(mobileTractorUnit));
			if (tractorInCargo != null)
				return tractorInCargo.ClickMenuEntryByRegexPattern(bot, launch);
			return null;
		}

		public IInventoryProvider GetLootableWindow()
		{
			var lootWindow = memoryMeasurement?.WindowInventory
				?.FirstOrDefault(wi => wi?.LootAllButton !=null);
			if (lootWindow != null)
				return new MemoryProxyInventoryProvider(bot, lootWindow);
			return null;
		}

		public bool IsEmpty => !(selectedWindowInventory?.SelectedContainerInventory?.ItemsView?.Any() ?? false);

		public ISerializableBotTask GetClickLootButtonTask()
		{
			var lootButton = selectedWindowInventory?.LootAllButton;
			if (lootButton != null)
				return lootButton.ClickTask();
			return null;
		}
	}
}