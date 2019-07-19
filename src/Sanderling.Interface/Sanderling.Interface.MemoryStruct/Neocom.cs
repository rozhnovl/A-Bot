using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class Neocom : Container, INeocom, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public IUIElement EveMenuButton
		{
			get;
			set;
		}

		public IUIElement CharButton
		{
			get;
			set;
		}

		public ISprite[] Button
		{
			get;
			set;
		}

		public IUIElementText Clock
		{
			get;
			set;
		}

		public Neocom()
		{
		}

		public Neocom(IUIElement @base)
			: base(@base)
		{
		}
	}
}
