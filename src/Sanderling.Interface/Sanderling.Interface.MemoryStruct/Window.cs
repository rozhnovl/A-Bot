using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class Window : Container, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public bool? isModal
		{
			get;
			set;
		}

		public string Caption
		{
			get;
			set;
		}

		public bool? HeaderButtonsVisible
		{
			get;
			set;
		}

		public ISprite[] HeaderButton
		{
			get;
			set;
		}

		public Window()
		{
		}

		public Window(IUIElement @base)
			: base(@base)
		{
			IWindow window = @base as IWindow;
			isModal = window?.isModal;
			Caption = window?.Caption;
			HeaderButtonsVisible = window?.HeaderButtonsVisible;
			HeaderButton = window?.HeaderButton;
		}
	}
}
