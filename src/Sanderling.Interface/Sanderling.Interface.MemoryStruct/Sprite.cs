using BotEngine;
using BotEngine.Interface;

namespace Sanderling.Interface.MemoryStruct
{
	public class Sprite : UIElement, ISprite, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public ColorORGB Color
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public IObjectIdInMemory Texture0Id
		{
			get;
			set;
		}

		public string TexturePath
		{
			get;
			set;
		}

		public string HintText
		{
			get;
			set;
		}

		public Sprite(IUIElement @base)
			: base(@base)
		{
		}

		public Sprite()
		{
		}
	}
}
