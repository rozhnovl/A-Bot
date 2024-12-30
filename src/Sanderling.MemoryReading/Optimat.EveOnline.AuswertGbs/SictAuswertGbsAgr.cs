using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using BotEngine.WinApi;
using Sanderling.Interface.MemoryStruct;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsAgr
{
	private static readonly string VersionLabelRegexPattern = Regex.Escape("Version:") + "\\s*([\\s\\d\\.]+)";

	public readonly UINodeInfoInTree GbsBaumWurzel;

	public UINodeInfoInTree AstLayerMenu { get; private set; }

	public UINodeInfoInTree AstLayerMain { get; private set; }

	public UINodeInfoInTree AstLayerModal { get; private set; }

	public UINodeInfoInTree LayerSystemmenuAst { get; private set; }

	public UINodeInfoInTree LayerLoginAst { get; private set; }

	public UINodeInfoInTree LayerSystemmenuSysmenuAst { get; private set; }

	public UINodeInfoInTree AstLayerModalModal { get; private set; }

	public string VersionString { get; private set; }

	public SictAuswertGbsSystemMenu SystemmenuAuswert { get; private set; }

	public UINodeInfoInTree[] AstLayerModalMengeKandidaatWindow { get; private set; }

	public UINodeInfoInTree LayerHintAst { get; private set; }

	public UINodeInfoInTree ModuleButtonHintAst { get; private set; }

	public SictAuswertGbsModuleButtonTooltip ModuleButtonTooltipAuswert { get; private set; }

	public UINodeInfoInTree AstLayerUtilmenu { get; private set; }

	public UINodeInfoInTree AstLayerAbovemain { get; private set; }

	public UINodeInfoInTree AstSidePanels { get; private set; }

	public UINodeInfoInTree[] MengeKandidaatMenuAst { get; private set; }

	public UINodeInfoInTree[] MengeKandidaatAbovemainMessageAst { get; private set; }

	public UINodeInfoInTree[] MengeKandidaatAbovemainPanelEveMenuAst { get; private set; }

	public UINodeInfoInTree[] MengeKandidaatAbovemainPanelGroupAst { get; private set; }

	public SictAuswertGbsPanelEveMenu[] MengeKandidaatAbovemainPanelEveMenuAuswert { get; private set; }

	public SictAuswertGbsPanelGroup[] MengeKandidaatAbovemainPanelGroupAuswert { get; private set; }

	public UINodeInfoInTree[] LayerMainMengeKandidaatWindowAst { get; private set; }

	public UINodeInfoInTree[] MengeKandidaatWindowAst { get; private set; }

	public Window[] MengeWindow { get; private set; }

	public IContainer[] Utilmenu { get; private set; }

	public UINodeInfoInTree LayerShipUiAst { get; private set; }

	public SictAuswertGbsShipUi LayerShipUiAstAuswert { get; private set; }

	public SictAuswertGbsLayerTarget AuswertLayerTarget { get; private set; }

	public SictAuswertGbsSidePanels AuswertSidePanels { get; private set; }

	public IMemoryMeasurement AuswertErgeebnis { get; private set; }

	public SictAuswertGbsAgr(UINodeInfoInTree GbsBaumWurzel)
	{
		this.GbsBaumWurzel = GbsBaumWurzel;
	}

	public void Berecne(int? sessionDurationRemaining)
	{
		//IL_0a54: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a59: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a63: Expected O, but got Unknown
		if (GbsBaumWurzel == null)
		{
			return;
		}
		AstSidePanels = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("SidePanels", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase));
		LayerShipUiAst = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("ShipUI", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 5, 1);
		UINodeInfoInTree layerTargetNode = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("l_target", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));
		UINodeInfoInTree uINodeInfoInTree = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("OverView", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase));
		AstLayerMenu = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("l_menu", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		AstLayerMain = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("l_main", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		AstLayerModal = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("l_modal", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		LayerSystemmenuAst = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("SystemMenu", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("l_systemmenu", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		LayerSystemmenuSysmenuAst = LayerSystemmenuAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.PyObjTypNameIsContainer() && string.Equals("sysmenu", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		LayerLoginAst = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("Login", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("l_login", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);
		IInSpaceBracket[] inflightBracket = ((GbsBaumWurzel?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree c) => (c?.PyObjTypName?.RegexMatchSuccessIgnoreCase("Layer") ?? false) && (c?.Name?.RegexMatchSuccessIgnoreCase("inflight") ?? false)))?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree c) => (c?.PyObjTypName?.RegexMatchSuccessIgnoreCase("Layer") ?? false) && (c?.Name?.RegexMatchSuccessIgnoreCase("bracket") ?? false)))?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree c) => c?.PyObjTypName?.RegexMatchSuccessIgnoreCase("InSpaceBracket") ?? false, null, null, null, omitNodesBelowNodesMatchingPredicate: true)?.Select((UINodeInfoInTree bracketNode) => bracketNode?.AsInSpaceBracket())?.ToArrayIfNotEmpty();
		UINodeInfoInTree[] array = LayerSystemmenuAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.GbsAstTypeIstLabel(), null, 3, 1).ConcatNullable(LayerLoginAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.GbsAstTypeIstLabel(), null, 3, 1))?.ToArray();
		if (LayerSystemmenuAst != null)
		{
			SystemmenuAuswert = new SictAuswertGbsSystemMenu(LayerSystemmenuAst);
			SystemmenuAuswert.Berecne();
		}
		if (array != null)
		{
			UINodeInfoInTree[] array2 = array;
			foreach (UINodeInfoInTree node in array2)
			{
				string text = node.LabelText();
				if (text != null)
				{
					Match match = Regex.Match(text ?? "", VersionLabelRegexPattern, RegexOptions.IgnoreCase);
					if (match.Success)
					{
						VersionString = match.Groups[1].Value;
						break;
					}
				}
			}
		}
		AstLayerModalMengeKandidaatWindow = AstLayerModal.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Regex.Match(Kandidaat.PyObjTypName ?? "", "MessageBox", RegexOptions.IgnoreCase).Success || Regex.Match(Kandidaat.PyObjTypName ?? "", "HybridWindow", RegexOptions.IgnoreCase).Success || Regex.Match(Kandidaat.PyObjTypName ?? "", "PopupWnd", RegexOptions.IgnoreCase).Success, null, 3, 1);
		LayerHintAst = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("l_hint", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		Container[] tooltip = GbsBaumWurzel.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree k) => k?.PyObjTypNameMatchesRegexPattern("TooltipGeneric|TooltipPanel") ?? false)?.Select((UINodeInfoInTree TooltipNode) => TooltipNode?.AlsContainer())?.WhereNotDefault()?.ToArrayIfNotEmpty();
		ModuleButtonHintAst = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("ModuleButtonTooltip", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && true == Kandidaat.VisibleIncludingInheritance, 2, 1);
		AstLayerUtilmenu = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("l_utilmenu", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		AstLayerAbovemain = GbsBaumWurzel.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("l_abovemain", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		MengeKandidaatAbovemainMessageAst = AstLayerAbovemain.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("Message", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 3, 1);
		MengeKandidaatAbovemainPanelEveMenuAst = AstLayerAbovemain.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("PanelEveMenu", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 3, 1);
		MengeKandidaatAbovemainPanelGroupAst = AstLayerAbovemain.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("PanelGroup", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 3, 1);
		LayerMainMengeKandidaatWindowAst = AstLayerMain.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => true == Kandidaat.VisibleIncludingInheritance && (Kandidaat.Caption != null || Kandidaat.WindowID != null), null, 2, 1, omitNodesBelowNodesMatchingPredicate: true);
		MengeKandidaatWindowAst = new UINodeInfoInTree[2][] { LayerMainMengeKandidaatWindowAst, AstLayerModalMengeKandidaatWindow }.ListeEnumerableAgregiirt()?.WhereNotDefault()?.ToArray();
		MengeWindow = MengeKandidaatWindowAst?.Select((UINodeInfoInTree WindowAst) => Glob.WindowBerecneScpezTypFürGbsAst(WindowAst))?.WhereNotDefault()?.ToArray();
		MengeKandidaatMenuAst = AstLayerMenu.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Regex.Match(Kandidaat.PyObjTypName ?? "", "DropDownMenu", RegexOptions.IgnoreCase).Success, null, 2, 1);
		if (ModuleButtonHintAst != null)
		{
			ModuleButtonTooltipAuswert = new SictAuswertGbsModuleButtonTooltip(ModuleButtonHintAst);
			ModuleButtonTooltipAuswert.Berecne();
		}
		if (MengeKandidaatAbovemainPanelEveMenuAst != null)
		{
			MengeKandidaatAbovemainPanelEveMenuAuswert = MengeKandidaatAbovemainPanelEveMenuAst.Select(delegate(UINodeInfoInTree GbsAst)
			{
				SictAuswertGbsPanelEveMenu sictAuswertGbsPanelEveMenu = new SictAuswertGbsPanelEveMenu(GbsAst);
				sictAuswertGbsPanelEveMenu.Berecne();
				return sictAuswertGbsPanelEveMenu;
			}).ToArray();
		}
		if (MengeKandidaatAbovemainPanelGroupAst != null)
		{
			MengeKandidaatAbovemainPanelGroupAuswert = MengeKandidaatAbovemainPanelGroupAst.Select(delegate(UINodeInfoInTree GbsAst)
			{
				SictAuswertGbsPanelGroup sictAuswertGbsPanelGroup = new SictAuswertGbsPanelGroup(GbsAst);
				sictAuswertGbsPanelGroup.Berecne();
				return sictAuswertGbsPanelGroup;
			}).ToArray();
		}
		UINodeInfoInTree[] array3 = ((AstLayerUtilmenu == null) ? null : GbsBaumWurzel.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => true == Kandidaat.VisibleIncludingInheritance && string.Equals("UtilMenu", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, null, 1));
		Utilmenu = ((IEnumerable<IContainer>)(object)new IContainer[1] { AstLayerUtilmenu.AlsUtilmenu() }).WhereNotDefault()?.ToArrayIfNotEmpty();
		AuswertSidePanels = new SictAuswertGbsSidePanels(AstSidePanels);
		AuswertSidePanels.Berecne();
		LayerShipUiAstAuswert = new SictAuswertGbsShipUi(LayerShipUiAst);
		LayerShipUiAstAuswert.Berecne();
		AuswertLayerTarget = new SictAuswertGbsLayerTarget(layerTargetNode);
		AuswertLayerTarget.Berecne();
		SictAuswertGbsInfoPanelCurrentSystem auswertPanelCurrentSystem = AuswertSidePanels.AuswertPanelCurrentSystem;
		SictAuswertGbsInfoPanelRoute auswertPanelRoute = AuswertSidePanels.AuswertPanelRoute;
		SictAuswertGbsInfoPanelMissions auswertPanelMissions = AuswertSidePanels.AuswertPanelMissions;
		MemoryMeasurement val = new MemoryMeasurement
		{
			SessionDurationRemaining = sessionDurationRemaining
		};
		val.UserDefaultLocaleName = Kernel32.GetUserDefaultLocaleName();
		if (SystemmenuAuswert != null)
		{
			val.SystemMenu = SystemmenuAuswert.Ergeebnis;
		}
		Vektor2DSingle? vektor2DSingle = AstLayerMain?.Grööse;
		if (vektor2DSingle.HasValue)
		{
			val.ScreenSize = new Vektor2DInt((int)vektor2DSingle.Value.A, (int)vektor2DSingle.Value.B);
		}
		if (ModuleButtonTooltipAuswert != null)
		{
			val.ModuleButtonTooltip = ModuleButtonTooltipAuswert.Ergeebnis;
		}
		val.Menu = (IMenu[])(object)MengeKandidaatMenuAst?.Select(SictAuswertGbsMenu.ReadMenu)?.WhereNotDefault()?.ToArrayIfNotEmpty();
		val.AbovemainMessage = MengeKandidaatAbovemainMessageAst?.Select((UINodeInfoInTree GbsAst) => GbsAst?.LargestLabelInSubtree()?.AsUIElementTextIfTextNotEmpty())?.Where((IUIElementText Label) => 0 < ((Label == null) ? null : Label.Text?.Length))?.ToArrayIfNotEmpty();
		if (MengeKandidaatAbovemainPanelEveMenuAuswert != null)
		{
			val.AbovemainPanelEveMenu = (from Kandidaat in MengeKandidaatAbovemainPanelEveMenuAuswert
				select Kandidaat.Ergeebnis into Kandidaat
				where Kandidaat != null
				select Kandidaat)?.ToArrayIfNotEmpty();
		}
		if (MengeKandidaatAbovemainPanelGroupAuswert != null)
		{
			val.AbovemainPanelGroup = (from Kandidaat in MengeKandidaatAbovemainPanelGroupAuswert
				select Kandidaat.Ergeebnis into Kandidaat
				where Kandidaat != null
				select Kandidaat)?.ToArrayIfNotEmpty();
		}
		IWindowStation val2 = null;
		IWindowOverview val3 = null;
		IWindowInventory[] array4 = null;
		if (MengeWindow != null)
		{
			WindowStack[] array5 = MengeWindow.OfType<WindowStack>().ToArray();
			IWindow[] seq = (from WindowStack in array5
				select WindowStack.TabSelectedWindow into Window
				where Window != null
				select Window).ToArray();
			IWindow[] array6 = ((IEnumerable<IWindow>)(object)MengeWindow).ConcatNullable(seq)?.ToArrayIfNotEmpty();
			IWindowOverview[] array7 = array6?.OfType<IWindowOverview>().ToArrayIfNotEmpty();
			WindowChatChannel[] array8 = array6?.OfType<WindowChatChannel>().ToArrayIfNotEmpty();
			IWindowSelectedItemView[] array9 = array6?.OfType<IWindowSelectedItemView>().ToArrayIfNotEmpty();
			WindowPeopleAndPlaces[] array10 = array6?.OfType<WindowPeopleAndPlaces>().ToArrayIfNotEmpty();
			IWindowDroneView[] array11 = array6?.OfType<IWindowDroneView>().ToArrayIfNotEmpty();
			WindowShipFitting[] array12 = array6?.OfType<WindowShipFitting>().ToArrayIfNotEmpty();
			WindowFittingMgmt[] array13 = array6?.OfType<WindowFittingMgmt>().ToArrayIfNotEmpty();
			IWindowStation[] array14 = array6?.OfType<IWindowStation>().ToArrayIfNotEmpty();
			IWindowSurveyScanView[] array15 = array6?.OfType<IWindowSurveyScanView>().ToArrayIfNotEmpty();
			array4 = array6?.OfType<IWindowInventory>().ToArrayIfNotEmpty();
			WindowAgentDialogue[] array16 = array6?.OfType<WindowAgentDialogue>().ToArrayIfNotEmpty();
			WindowAgentBrowser[] array17 = array6?.OfType<WindowAgentBrowser>().ToArrayIfNotEmpty();
			WindowTelecom[] array18 = array6?.OfType<WindowTelecom>().ToArrayIfNotEmpty();
			WindowRegionalMarket[] array19 = array6?.OfType<WindowRegionalMarket>().ToArrayIfNotEmpty();
			WindowMarketAction[] array20 = array6?.OfType<WindowMarketAction>().ToArrayIfNotEmpty();
			WindowItemSell[] array21 = array6?.OfType<WindowItemSell>().ToArrayIfNotEmpty();
			val.WindowStack = array5.ToArrayIfNotEmpty();
			val3 = array7?.FirstOrDefault();
			val2 = array14?.FirstOrDefault();
			val.WindowOverview = array7.ToArrayIfNotEmpty();
			val.WindowChatChannel = array8.ToArrayIfNotEmpty();
			val.WindowSelectedItemView = array9.ToArrayIfNotEmpty();
			val.WindowPeopleAndPlaces = array10.ToArrayIfNotEmpty();
			val.WindowDroneView = array11.ToArrayIfNotEmpty();
			val.WindowShipFitting = array12.ToArrayIfNotEmpty();
			val.WindowFittingMgmt = array13.ToArrayIfNotEmpty();
			val.WindowStation = array14.ToArrayIfNotEmpty();
			val.WindowSurveyScanView = array15.ToArrayIfNotEmpty();
			val.WindowInventory = array4;
			val.WindowAgentDialogue = (IWindowAgentDialogue[])(object)array16;
			val.WindowAgentBrowser = array17;
			val.WindowTelecom = array18;
			val.WindowRegionalMarket = array19;
			val.WindowMarketAction = array20;
			val.WindowItemSell = array21;
			val.WindowProbeScanner = array6?.OfType<IWindowProbeScanner>().ToArrayIfNotEmpty();
			IWindow[] windowOther = ((IEnumerable<IWindow>)(object)MengeWindow).Except(new IEnumerable<IWindow>[18]
			{
				(IEnumerable<IWindow>)(object)array5,
				(IEnumerable<IWindow>)(object)array7,
				(IEnumerable<IWindow>)(object)array8,
				(IEnumerable<IWindow>)(object)array10,
				(IEnumerable<IWindow>)(object)array9,
				(IEnumerable<IWindow>)(object)array15,
				(IEnumerable<IWindow>)(object)array11,
				(IEnumerable<IWindow>)(object)array12,
				(IEnumerable<IWindow>)(object)array13,
				(IEnumerable<IWindow>)(object)array14,
				(IEnumerable<IWindow>)(object)array4,
				(IEnumerable<IWindow>)(object)array16,
				(IEnumerable<IWindow>)(object)array17,
				(IEnumerable<IWindow>)(object)array18,
				(IEnumerable<IWindow>)(object)array19,
				(IEnumerable<IWindow>)(object)array20,
				(IEnumerable<IWindow>)(object)array21,
				(IEnumerable<IWindow>)val.WindowProbeScanner
			}.ConcatNullable()).ToArrayIfNotEmpty();
			val.WindowOther = windowOther;
		}
		val.VersionString = VersionString;
		val.Neocom = (INeocom)(object)AuswertSidePanels?.Neocom;
		//val.InfoPanelCurrentSystem = (IInfoPanelSystem)(object)auswertPanelCurrentSystem?.ErgeebnisScpez;
		//val.InfoPanelRoute = (IInfoPanelRoute)(object)auswertPanelRoute?.ErgeebnisScpez;
		val.InfoPanelMissions = (IInfoPanelMissions)(object)auswertPanelMissions?.ErgeebnisScpez;
		IUIElement infoPanelButtonCurrentSystem = AuswertSidePanels.InfoPanelButtonLocationInfoAst.AsUIElementIfVisible();
		IUIElement infoPanelButtonRoute = AuswertSidePanels.InfoPanelButtonRouteAst.AsUIElementIfVisible();
		IUIElement infoPanelButtonMissions = AuswertSidePanels.InfoPanelButtonMissionAst.AsUIElementIfVisible();
		val.InfoPanelButtonCurrentSystem = infoPanelButtonCurrentSystem;
		val.InfoPanelButtonRoute = infoPanelButtonRoute;
		val.InfoPanelButtonMissions = infoPanelButtonMissions;
		val.InfoPanelButtonIncursions = AuswertSidePanels?.InfoPanelButtonIncursionsAst?.AsUIElementIfVisible();
		val.Utilmenu = Utilmenu;
		val.ShipUi = (IShipUi)(object)LayerShipUiAstAuswert.Ergeebnis;
		val.Target = (IShipUiTarget[])(object)AuswertLayerTarget?.SetTarget?.ToArrayIfNotEmpty();
		val.Tooltip = (IContainer[])(object)tooltip;
		val.InflightBracket = inflightBracket;
		AuswertErgeebnis = (IMemoryMeasurement)(object)val;
	}
}
