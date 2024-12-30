using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsInfoPanelCurrentSystem : SictAuswertGbsInfoPanelGen
{
	public UINodeInfoInTree AstInfoPanelContainer { get; private set; }

	public UINodeInfoInTree AstListSurroundingsBtn { get; private set; }

	public UINodeInfoInTree AstHeaderContLabelHeader { get; private set; }

	public string TopHeaderLabelText { get; private set; }

	public UINodeInfoInTree AstMainContLabelNearestLocationInfo { get; private set; }

	public InfoPanelSystem ErgeebnisScpez { get; private set; }

	public SictAuswertGbsInfoPanelCurrentSystem(UINodeInfoInTree astInfoPanelLocationInfo)
		: base(astInfoPanelLocationInfo)
	{
	}

	public override void Berecne()
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		base.Berecne();
		InfoPanel ergeebnis = base.Ergeebnis;
		if (ergeebnis != null)
		{
			AstListSurroundingsBtn = base.HeaderBtnContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("ListSurroundingsBtn", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 5);
			AstHeaderContLabelHeader = base.HeaderContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("header", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 4);
			AstMainContLabelNearestLocationInfo = base.MainContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => true == kandidaat.VisibleIncludingInheritance && string.Equals("nearestLocationInfo", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
			if (AstHeaderContLabelHeader != null)
			{
				TopHeaderLabelText = AstHeaderContLabelHeader.SetText;
			}
			IUIElement listSurroundingsButton = null;
			if (AstListSurroundingsBtn != null)
			{
				listSurroundingsButton = AstListSurroundingsBtn.AsUIElementIfVisible();
			}
			ErgeebnisScpez = new InfoPanelSystem((IInfoPanel)(object)ergeebnis)
			{
				ListSurroundingsButton = listSurroundingsButton
			};
		}
	}
}
