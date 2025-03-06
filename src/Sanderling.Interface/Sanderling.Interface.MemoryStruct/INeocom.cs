using BotEngine;
using Newtonsoft.Json.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	public interface INeocom
	{
		//IUIElement EveMenuButton
		//{
		//	get;
		//}

		//IUIElement CharButton
		//{
		//	get;
		//}
		IUIElement InventoryButton
		{
			get;
		}
		IUIElement PeopleAndPlacesButton { get; }

		IUIElement ChatButton { get; }

		IUIElement MailButton { get; }

		IUIElement FittingButton { get; }

		IUIElement MarketButton { get; }


		//ISprite[] Button
		//{
		//	get;
		//}
	}
}
