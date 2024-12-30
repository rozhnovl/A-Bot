using System;
using System.Linq;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsInfoPanelRoute : SictAuswertGbsInfoPanelGen
{
	public UINodeInfoInTree AstLabelNoDestination { get; private set; }

	public UINodeInfoInTree AstMarkersParent { get; private set; }

	public UINodeInfoInTree[] MengeAstDestinationMarker { get; private set; }

	public UINodeInfoInTree AstCurrentParent { get; private set; }

	public UINodeInfoInTree AstEndParent { get; private set; }

	public UINodeInfoInTree AstEndParentLabel { get; private set; }

	public UINodeInfoInTree AstCurrentParentLabel { get; private set; }

	public InfoPanelRoute ErgeebnisScpez { get; private set; }

	public SictAuswertGbsInfoPanelRoute(UINodeInfoInTree astInfoPanelRoute)
		: base(astInfoPanelRoute)
	{
	}

	public override void Berecne()
	{
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Expected O, but got Unknown
		base.Berecne();
		InfoPanel ergeebnis = base.Ergeebnis;
		if (ergeebnis != null)
		{
			AstLabelNoDestination = base.MainContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("noDestinationLabel", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
			AstMarkersParent = base.MainContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("markersParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
			AstCurrentParent = base.MainContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("currentParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			AstCurrentParentLabel = AstCurrentParent.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EveLabelMedium", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			AstEndParent = base.MainContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("endParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			AstEndParentLabel = AstEndParent.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EveLabelMedium", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			MengeAstDestinationMarker = AstMarkersParent.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("AutopilotDestinationIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2);
			IUIElement[] array = MengeAstDestinationMarker?.Select((UINodeInfoInTree astDestinationMarker) => astDestinationMarker.AsUIElementIfVisible())?.ToArray();
			ErgeebnisScpez = new InfoPanelRoute((IInfoPanel)(object)ergeebnis)
			{
				NextLabel = AstCurrentParentLabel.LargestLabelInSubtree().AsUIElementTextIfTextNotEmpty(),
				DestinationLabel = AstEndParentLabel.LargestLabelInSubtree().AsUIElementTextIfTextNotEmpty(),
				RouteElementMarker = array?.OrdnungLabel<IUIElement>()?.ToArray()
			};
		}
	}
}
