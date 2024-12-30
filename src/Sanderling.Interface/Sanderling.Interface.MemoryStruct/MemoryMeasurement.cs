using Bib3.Geometrik;
using System;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public class MemoryMeasurement : IMemoryMeasurement, ICloneable
	{
		public int? SessionDurationRemaining
		{
			get;
			set;
		}

		public string UserDefaultLocaleName
		{
			get;
			set;
		}

		public string VersionString
		{
			get;
			set;
		}

		public Vektor2DInt ScreenSize
		{
			get;
			set;
		}

		public IMenu[] Menu
		{
			get;
			set;
		}

		public IContainer[] Tooltip
		{
			get;
			set;
		}

		public IShipUi ShipUi
		{
			get;
			set;
		}

		public IShipUiTarget[] Target
		{
			get;
			set;
		}

		public IInSpaceBracket[] InflightBracket
		{
			get;
			set;
		}

		public IContainer ModuleButtonTooltip
		{
			get;
			set;
		}

		public IWindow SystemMenu
		{
			get;
			set;
		}

		public INeocom Neocom
		{
			get;
			set;
		}

		public IUIElement InfoPanelButtonCurrentSystem
		{
			get;
			set;
		}

		public IUIElement InfoPanelButtonRoute
		{
			get;
			set;
		}

		public IUIElement InfoPanelButtonMissions
		{
			get;
			set;
		}

		public IUIElement InfoPanelButtonIncursions
		{
			get;
			set;
		}

		public IInfoPanelContainer InfoPanelContainer { get; set; }

		//public IInfoPanelSystem InfoPanelCurrentSystem
		//{
		//	get;
		//	set;
		//}

		//public IInfoPanelRoute InfoPanelRoute
		//{
		//	get;
		//	set;
		//}

		public IInfoPanelMissions InfoPanelMissions
		{
			get;
			set;
		}

		public IContainer[] Utilmenu
		{
			get;
			set;
		}

		public IUIElementText[] AbovemainMessage
		{
			get;
			set;
		}

		public PanelGroup[] AbovemainPanelGroup
		{
			get;
			set;
		}

		public PanelGroup[] AbovemainPanelEveMenu
		{
			get;
			set;
		}

		public IWindow[] WindowOther
		{
			get;
			set;
		}

		public WindowStack[] WindowStack
		{
			get;
			set;
		}

		public IWindowOverview[] WindowOverview
		{
			get;
			set;
		}

		public WindowChatChannel[] WindowChatChannel
		{
			get;
			set;
		}

		public IWindowSelectedItemView[] WindowSelectedItemView
		{
			get;
			set;
		}

		public IWindowDroneView[] WindowDroneView
		{
			get;
			set;
		}

		public WindowPeopleAndPlaces[] WindowPeopleAndPlaces
		{
			get;
			set;
		}

		public IWindowStation[] WindowStation
		{
			get;
			set;
		}

		public WindowShipFitting[] WindowShipFitting
		{
			get;
			set;
		}

		public WindowFittingMgmt[] WindowFittingMgmt
		{
			get;
			set;
		}

		public IWindowSurveyScanView[] WindowSurveyScanView
		{
			get;
			set;
		}

		public IWindowInventory[] WindowInventory
		{
			get;
			set;
		}

		public IWindowAgentDialogue[] WindowAgentDialogue
		{
			get;
			set;
		}

		public WindowAgentBrowser[] WindowAgentBrowser
		{
			get;
			set;
		}

		public WindowTelecom[] WindowTelecom
		{
			get;
			set;
		}

		public WindowRegionalMarket[] WindowRegionalMarket
		{
			get;
			set;
		}

		public WindowMarketAction[] WindowMarketAction
		{
			get;
			set;
		}

		public WindowItemSell[] WindowItemSell
		{
			get;
			set;
		}

		public IEnumerable<IWindowProbeScanner> WindowProbeScanner
		{
			get;
			set;
		}

		public MemoryMeasurement Copy()
		{
			return this.CopyByPolicyMemoryMeasurement();
		}

		public object Clone()
		{
			return Copy();
		}
	}
}
