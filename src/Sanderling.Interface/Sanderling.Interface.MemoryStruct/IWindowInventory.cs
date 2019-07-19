using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowInventory : IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		ITreeViewEntry[] LeftTreeListEntry
		{
			get;
		}

		IScroll LeftTreeViewportScroll
		{
			get;
		}

		IUIElementText SelectedRightInventoryPathLabel
		{
			get;
		}

		IInventory SelectedRightInventory
		{
			get;
		}

		IUIElementText SelectedRightInventoryCapacity
		{
			get;
		}

		ISprite[] SelectedRightControlViewButton
		{
			get;
		}

		IUIElementInputText SelectedRightFilterTextBox
		{
			get;
		}

		IUIElement SelectedRightFilterButtonClear
		{
			get;
		}
	}
}
