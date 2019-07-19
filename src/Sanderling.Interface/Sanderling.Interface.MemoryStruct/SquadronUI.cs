using BotEngine;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public class SquadronUI : UIElement, ISquadronUI, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public ISquadronContainer Squadron
		{
			get;
			set;
		}

		public IEnumerable<ISquadronAbilityIcon> SetAbilityIcon
		{
			get;
			set;
		}

		public SquadronUI()
		{
		}

		public SquadronUI(IUIElement @base)
			: base(@base)
		{
		}
	}
}
