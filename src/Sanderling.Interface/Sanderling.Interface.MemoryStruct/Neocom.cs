using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class Neocom : INeocom
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
		public IUIElement InventoryButton
		{
			get;
			set;
		}

		public IUIElement PeopleAndPlacesButton { get; init; }
		public IUIElement ChatButton { get; init; }
		public IUIElement MailButton { get; init; }
		public IUIElement FittingButton { get; init; }
		public IUIElement MarketButton { get; init; }

		public ISprite[] Button
		{
			get;
			set;
		}
		
		public Neocom()
		{
		}
	}
}
