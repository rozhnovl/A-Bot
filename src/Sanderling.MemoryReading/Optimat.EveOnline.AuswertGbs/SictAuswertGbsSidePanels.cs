using System;
using System.Linq;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsSidePanels
{
	public readonly UINodeInfoInTree AstSidePanels;

	private SictAuswertNeocom NeocomAuswert;

	public UINodeInfoInTree NeocomAst { get; private set; }

	public Neocom Neocom => NeocomAuswert?.Ergeebnis;

	public UINodeInfoInTree InfoPanelContainerAst { get; private set; }

	public UINodeInfoInTree InfoPanelContainerTopContAst { get; private set; }

	public UINodeInfoInTree[] InfoPanelContainerTopContMengeButtonAst { get; private set; }

	public UINodeInfoInTree InfoPanelButtonIncursionsAst { get; private set; }

	public UINodeInfoInTree InfoPanelButtonLocationInfoAst { get; private set; }

	public UINodeInfoInTree InfoPanelButtonRouteAst { get; private set; }

	public UINodeInfoInTree InfoPanelButtonMissionAst { get; private set; }

	public UINodeInfoInTree AstInfoPanelLocationInfo { get; private set; }

	public UINodeInfoInTree AstInfoPanelRoute { get; private set; }

	public UINodeInfoInTree AstInfoPanelMissions { get; private set; }

	public SictAuswertGbsInfoPanelCurrentSystem AuswertPanelCurrentSystem { get; private set; }

	public SictAuswertGbsInfoPanelRoute AuswertPanelRoute { get; private set; }

	public SictAuswertGbsInfoPanelMissions AuswertPanelMissions { get; private set; }

	public SictAuswertGbsSidePanels(UINodeInfoInTree AstSidePanels)
	{
		this.AstSidePanels = AstSidePanels;
	}

	public void Berecne()
	{
		if (AstSidePanels == null || true != AstSidePanels.VisibleIncludingInheritance)
		{
			return;
		}
		NeocomAst = AstSidePanels.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("Neocom", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		NeocomAuswert = new SictAuswertNeocom(NeocomAst);
		NeocomAuswert.Berecne();
		InfoPanelContainerAst = AstSidePanels.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("InfoPanelContainer", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);
		InfoPanelContainerTopContAst = InfoPanelContainerAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("topCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 1);
		InfoPanelContainerTopContMengeButtonAst = InfoPanelContainerTopContAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => true == Kandidaat.VisibleIncludingInheritance && string.Equals("ButtonIconInfoPanel", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 3, 1);
		if (InfoPanelContainerTopContMengeButtonAst != null)
		{
			InfoPanelButtonIncursionsAst = InfoPanelContainerTopContMengeButtonAst.FirstOrDefault((UINodeInfoInTree Kandidaat) => Regex.Match(Kandidaat.Name ?? "", "Incursion", RegexOptions.IgnoreCase).Success);
			InfoPanelButtonLocationInfoAst = InfoPanelContainerTopContMengeButtonAst.FirstOrDefault((UINodeInfoInTree Kandidaat) => Regex.Match(Kandidaat.Name ?? "", "Location", RegexOptions.IgnoreCase).Success);
			InfoPanelButtonRouteAst = InfoPanelContainerTopContMengeButtonAst.FirstOrDefault((UINodeInfoInTree Kandidaat) => Regex.Match(Kandidaat.Name ?? "", "Route", RegexOptions.IgnoreCase).Success);
			InfoPanelButtonMissionAst = InfoPanelContainerTopContMengeButtonAst.FirstOrDefault((UINodeInfoInTree Kandidaat) => Regex.Match(Kandidaat.Name ?? "", "Mission", RegexOptions.IgnoreCase).Success);
		}
		AstInfoPanelLocationInfo = InfoPanelContainerAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("InfoPanelLocationInfo", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);
		if (AstInfoPanelLocationInfo == null)
		{
			UINodeInfoInTree[] array = InfoPanelContainerAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => true, null, 2);
			if (array != null)
			{
				UINodeInfoInTree[] array2 = array;
				foreach (UINodeInfoInTree astInfoPanelLocationInfo in array2)
				{
					SictAuswertGbsInfoPanelCurrentSystem sictAuswertGbsInfoPanelCurrentSystem = new SictAuswertGbsInfoPanelCurrentSystem(astInfoPanelLocationInfo);
					sictAuswertGbsInfoPanelCurrentSystem.Berecne();
					object obj;
					if (sictAuswertGbsInfoPanelCurrentSystem == null)
					{
						obj = null;
					}
					else
					{
						InfoPanelSystem ergeebnisScpez = sictAuswertGbsInfoPanelCurrentSystem.ErgeebnisScpez;
						obj = ((ergeebnisScpez != null) ? ergeebnisScpez.ListSurroundingsButton : null);
					}
					if (obj != null)
					{
						AstInfoPanelLocationInfo = astInfoPanelLocationInfo;
					}
				}
			}
		}
		AstInfoPanelRoute = InfoPanelContainerAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("InfoPanelRoute", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);
		AstInfoPanelMissions = InfoPanelContainerAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("InfoPanelMissions", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);
		AuswertPanelCurrentSystem = new SictAuswertGbsInfoPanelCurrentSystem(AstInfoPanelLocationInfo);
		AuswertPanelCurrentSystem.Berecne();
		AuswertPanelRoute = new SictAuswertGbsInfoPanelRoute(AstInfoPanelRoute);
		AuswertPanelRoute.Berecne();
		AuswertPanelMissions = new SictAuswertGbsInfoPanelMissions(AstInfoPanelMissions);
		AuswertPanelMissions.Berecne();
	}
}
