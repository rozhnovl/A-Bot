using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot
{
	public class MemoryProxyInventoryProvider : IInventoryProvider
	{
		private readonly Bot bot;
		private readonly Sanderling.Parse.IMemoryMeasurement memoryMeasurement;
		private IWindowInventory? selectedWindowInventory;

		public MemoryProxyInventoryProvider(Bot bot, IWindowInventory? selectedWindowInventory = null)
		{
			this.bot = bot;
			memoryMeasurement = bot.MemoryMeasurementAtTime.Value;
			this.selectedWindowInventory = selectedWindowInventory ?? memoryMeasurement?.WindowInventory?.FirstOrDefault();
		}

		public ISerializableBotTask? GetCloseWindowTask()
		{
			if ((memoryMeasurement?.WindowInventory?.Any() ?? false))
				return memoryMeasurement.Neocom.InventoryButton.ClickTask();
			return null;
		}

		public ISerializableBotTask? GetOpenWindowTask()
		{
			if (!(memoryMeasurement?.WindowInventory?.Any() ?? false))
				return memoryMeasurement.Neocom.InventoryButton.ClickTask();
			return null;
		}

		public ISerializableBotTask? GetActvateItemIfPresentTask(string mobileTractorUnit, string launch)
		{
			var tractorInCargo = selectedWindowInventory.SelectedContainerInventory.ItemsView
				.FirstOrDefault(lt => lt.CellsTexts.ContainsValue(mobileTractorUnit));
			return tractorInCargo != null ? tractorInCargo.ClickMenuEntryByRegexPattern(bot, launch) : null;
		}

		public IInventoryProvider? GetLootableWindow()
		{
			var lootWindow = memoryMeasurement?.WindowInventory
				?.FirstOrDefault(wi => wi?.LootAllButton !=null);
			return lootWindow != null ? new MemoryProxyInventoryProvider(bot, lootWindow) : null;
		}

		public bool IsEmpty => !(selectedWindowInventory?.SelectedContainerInventory?.ItemsView?.Any() ?? false);

		public ISerializableBotTask? GetClickLootButtonTask()
		{
			var lootButton = selectedWindowInventory?.LootAllButton;
			return lootButton?.ClickTask();
		}
	}
}