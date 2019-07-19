using Bib3.Geometrik;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IMemoryMeasurement
	{
		int? SessionDurationRemaining
		{
			get;
		}

		string UserDefaultLocaleName
		{
			get;
		}

		string VersionString
		{
			get;
		}

		Vektor2DInt ScreenSize
		{
			get;
		}

		IMenu[] Menu
		{
			get;
		}

		IContainer[] Tooltip
		{
			get;
		}

		IShipUi ShipUi
		{
			get;
		}

		IShipUiTarget[] Target
		{
			get;
		}

		IInSpaceBracket[] InflightBracket
		{
			get;
		}

		IContainer ModuleButtonTooltip
		{
			get;
		}

		IWindow SystemMenu
		{
			get;
		}

		INeocom Neocom
		{
			get;
		}

		IUIElement InfoPanelButtonCurrentSystem
		{
			get;
		}

		IUIElement InfoPanelButtonRoute
		{
			get;
		}

		IUIElement InfoPanelButtonMissions
		{
			get;
		}

		IUIElement InfoPanelButtonIncursions
		{
			get;
		}

		IInfoPanelSystem InfoPanelCurrentSystem
		{
			get;
		}

		IInfoPanelRoute InfoPanelRoute
		{
			get;
		}

		IInfoPanelMissions InfoPanelMissions
		{
			get;
		}

		IContainer[] Utilmenu
		{
			get;
		}

		IUIElementText[] AbovemainMessage
		{
			get;
		}

		PanelGroup[] AbovemainPanelGroup
		{
			get;
		}

		PanelGroup[] AbovemainPanelEveMenu
		{
			get;
		}

		IWindow[] WindowOther
		{
			get;
		}

		WindowStack[] WindowStack
		{
			get;
		}

		IWindowOverview[] WindowOverview
		{
			get;
		}

		WindowChatChannel[] WindowChatChannel
		{
			get;
		}

		IWindowSelectedItemView[] WindowSelectedItemView
		{
			get;
		}

		IWindowDroneView[] WindowDroneView
		{
			get;
		}

		WindowPeopleAndPlaces[] WindowPeopleAndPlaces
		{
			get;
		}

		IWindowStation[] WindowStation
		{
			get;
		}

		WindowShipFitting[] WindowShipFitting
		{
			get;
		}

		WindowFittingMgmt[] WindowFittingMgmt
		{
			get;
		}

		IWindowSurveyScanView[] WindowSurveyScanView
		{
			get;
		}

		IWindowInventory[] WindowInventory
		{
			get;
		}

		IWindowAgentDialogue[] WindowAgentDialogue
		{
			get;
		}

		WindowAgentBrowser[] WindowAgentBrowser
		{
			get;
		}

		WindowTelecom[] WindowTelecom
		{
			get;
		}

		WindowRegionalMarket[] WindowRegionalMarket
		{
			get;
		}

		WindowMarketAction[] WindowMarketAction
		{
			get;
		}

		WindowItemSell[] WindowItemSell
		{
			get;
		}

		IEnumerable<IWindowProbeScanner> WindowProbeScanner
		{
			get;
		}
	}
}
