using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowShipFitting : SictAuswertGbsWindow
{
	public UINodeInfoInTree FittingAst { get; private set; }

	public UINodeInfoInTree SlotParentAst { get; private set; }

	public UINodeInfoInTree RightSideAst { get; private set; }

	public UINodeInfoInTree[] MengeFittingSlotAst { get; private set; }

	public WindowShipFitting ErgeebnisScpez { get; private set; }

	public new static WindowShipFitting BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowShipFitting sictAuswertGbsWindowShipFitting = new SictAuswertGbsWindowShipFitting(windowAst);
		sictAuswertGbsWindowShipFitting.Berecne();
		return sictAuswertGbsWindowShipFitting.ErgeebnisScpez;
	}

	public SictAuswertGbsWindowShipFitting(UINodeInfoInTree windowAst)
		: base(windowAst)
	{
	}

	public override void Berecne()
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		base.Berecne();
		if (base.Ergeebnis != null)
		{
			FittingAst = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Fitting", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			SlotParentAst = FittingAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("slotParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			ErgeebnisScpez = new WindowShipFitting((IWindow)(object)base.Ergeebnis);
		}
	}
}
