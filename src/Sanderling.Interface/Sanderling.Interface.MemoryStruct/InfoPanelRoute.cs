using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class InfoPanelRoute : InfoPanel, IInfoPanelRoute, IInfoPanel, IUIElement, IObjectIdInMemory, IObjectIdInt64, IExpandable
	{
		public IUIElementText NextLabel
		{
			get;
			set;
		}

		public IUIElementText DestinationLabel
		{
			get;
			set;
		}

		public IUIElement[] RouteElementMarker
		{
			get;
			set;
		}

		public InfoPanelRoute()
		{
		}

		public InfoPanelRoute(IInfoPanel @base)
			: base(@base)
		{
		}
	}
}
