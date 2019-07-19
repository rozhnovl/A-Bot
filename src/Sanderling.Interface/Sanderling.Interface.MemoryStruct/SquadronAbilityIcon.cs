using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class SquadronAbilityIcon : UIElement, ISquadronAbilityIcon, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public int? Quantity
		{
			get;
			set;
		}

		public bool? RampActive
		{
			get;
			set;
		}

		public SquadronAbilityIcon()
		{
		}

		public SquadronAbilityIcon(IUIElement @base)
			: base(@base)
		{
		}
	}
}
