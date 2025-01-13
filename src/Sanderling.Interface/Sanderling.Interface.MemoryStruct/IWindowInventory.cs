using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowInventory
	{
		string? SubCaptionLabelText { get; }
		IInventory? SelectedContainerInventory { get; }
		IUIElement? LootAllButton { get; }
	}

	public interface IInventory
	{
		public List<IInventoryItemsListViewEntry>? ItemsView { get; set; }
	}
	public interface IInventoryItemsListViewEntry: IUiElementProvider
	{
		public Dictionary<string, string> CellsTexts { get; set; }
	}

	public interface IUiElementProvider
	{
		IUIElement Element { get; }
	}
}
