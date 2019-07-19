using BotEngine;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public class Menu : UIElement, IMenu, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public IMenuEntry[] Entry
		{
			get;
			set;
		}

		IEnumerable<IMenuEntry> IMenu.Entry
		{
			get
			{
				return Entry;
			}
		}

		public Menu(IUIElement @base)
			: base(@base)
		{
		}

		public Menu()
		{
		}
	}
}
