using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindow : IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		bool? isModal
		{
			get;
		}

		string Caption
		{
			get;
		}

		bool? HeaderButtonsVisible
		{
			get;
		}

		ISprite[] HeaderButton
		{
			get;
		}
	}
}
