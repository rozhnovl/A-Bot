using BotEngine;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IContainer : IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		IEnumerable<IUIElementText?> ButtonText
		{
			get;
		}

		IEnumerable<IUIElementInputText> InputText
		{
			get;
		}

		IEnumerable<IUIElementText> LabelText
		{
			get;
		}

		IEnumerable<ISprite> Sprite
		{
			get;
		}
	}
}
