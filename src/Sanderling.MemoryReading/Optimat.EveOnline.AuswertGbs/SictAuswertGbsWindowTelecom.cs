using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowTelecom : SictAuswertGbsWindow
{
	public UINodeInfoInTree AstMainContainerBottom { get; private set; }

	public UINodeInfoInTree AstMainContainerBottomButtons { get; private set; }

	public WindowTelecom ErgeebnisScpez { get; private set; }

	public new static WindowTelecom BerecneFürWindowAst(UINodeInfoInTree WindowAst)
	{
		if (WindowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowTelecom sictAuswertGbsWindowTelecom = new SictAuswertGbsWindowTelecom(WindowAst);
		sictAuswertGbsWindowTelecom.Berecne();
		return sictAuswertGbsWindowTelecom.ErgeebnisScpez;
	}

	public SictAuswertGbsWindowTelecom(UINodeInfoInTree AstWindow)
		: base(AstWindow)
	{
	}

	public override void Berecne()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		base.Berecne();
		AstMainContainerBottom = base.AstMainContainer.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("bottom", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		AstMainContainerBottomButtons = AstMainContainerBottom.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => (string.Equals("EveButtonGroup", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) || string.Equals("ButtonGroup", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)) && string.Equals("btnsmainparent", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		WindowTelecom ergeebnisScpez = new WindowTelecom((IWindow)(object)base.Ergeebnis);
		ErgeebnisScpez = ergeebnisScpez;
	}
}
