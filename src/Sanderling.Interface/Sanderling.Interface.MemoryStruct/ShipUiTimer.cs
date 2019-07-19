using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class ShipUiTimer : Container, IShipUiTimer, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public string Name
		{
			get;
			set;
		}

		public ShipUiTimer()
		{
		}

		public ShipUiTimer(IUIElement @base)
			: base(@base)
		{
		}
	}
}
