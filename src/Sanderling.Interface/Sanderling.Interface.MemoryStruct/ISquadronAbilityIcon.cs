using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface ISquadronAbilityIcon : IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		int? Quantity
		{
			get;
		}

		bool? RampActive
		{
			get;
		}
	}
}
