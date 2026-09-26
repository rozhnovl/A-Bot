using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowSelectedItemView : Window, IWindowSelectedItemView, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public ISprite[] ActionSprite
		{
			get;
			set;
		}

		public string? SelectedItemName { get; set; }

		public System.Collections.Generic.IReadOnlyDictionary<string, IUIElement>? ActionButtons { get; set; }

		public WindowSelectedItemView(IWindow @base)
			: base(@base)
		{
		}

		public WindowSelectedItemView()
		{
		}
	}
}
