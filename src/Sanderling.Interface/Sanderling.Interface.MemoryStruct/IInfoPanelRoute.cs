using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IInfoPanelRoute : IInfoPanel, IUIElement, IObjectIdInMemory, IObjectIdInt64, IExpandable
	{
		IUIElementText NextLabel
		{
			get;
		}

		IUIElementText DestinationLabel
		{
			get;
		}

		IUIElement[] RouteElementMarker
		{
			get;
		}
	}
}
