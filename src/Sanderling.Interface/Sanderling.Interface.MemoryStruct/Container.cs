using BotEngine;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public class Container : UIElement, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public IEnumerable<IUIElementText> ButtonText
		{
			get;
			set;
		}

		public IEnumerable<IUIElementInputText> InputText
		{
			get;
			set;
		}

		public IEnumerable<IUIElementText> LabelText
		{
			get;
			set;
		}

		public IEnumerable<ISprite> Sprite
		{
			get;
			set;
		}

		public Container()
		{
		}

		public Container(IUIElement @base)
			: base(@base)
		{
			IContainer container = @base as IContainer;
			ButtonText = container?.ButtonText;
			LabelText = container?.LabelText;
			InputText = container?.InputText;
			Sprite = container?.Sprite;
		}
	}
}
