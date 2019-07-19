using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowInventory : Window, IWindowInventory, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public ITreeViewEntry[] LeftTreeListEntry
		{
			get;
			set;
		}

		public IScroll LeftTreeViewportScroll
		{
			get;
			set;
		}

		public IUIElementText SelectedRightInventoryPathLabel
		{
			get;
			set;
		}

		public IInventory SelectedRightInventory
		{
			get;
			set;
		}

		public IUIElementText SelectedRightInventoryCapacity
		{
			get;
			set;
		}

		public ISprite[] SelectedRightControlViewButton
		{
			get;
			set;
		}

		public IUIElementInputText SelectedRightFilterTextBox
		{
			get;
			set;
		}

		public IUIElement SelectedRightFilterButtonClear
		{
			get;
			set;
		}

		public int? SelectedRightItemDisplayedCount
		{
			get;
			set;
		}

		public int? SelectedRightItemFilteredCount
		{
			get;
			set;
		}

		public WindowInventory()
			: this(null)
		{
		}

		public WindowInventory(IWindow @base)
			: base(@base)
		{
		}
	}
}
