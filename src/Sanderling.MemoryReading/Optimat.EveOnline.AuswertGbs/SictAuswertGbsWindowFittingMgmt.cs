using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowFittingMgmt : SictAuswertGbsWindow
{
	public UINodeInfoInTree LeftSideAst { get; private set; }

	public UINodeInfoInTree LeftMainPanelAst { get; private set; }

	public UINodeInfoInTree LeftMainPanelScrollAst { get; private set; }

	public WindowFittingMgmt ErgeebnisWindowFittingMgmt { get; private set; }

	public new static WindowFittingMgmt BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowFittingMgmt sictAuswertGbsWindowFittingMgmt = new SictAuswertGbsWindowFittingMgmt(windowAst);
		sictAuswertGbsWindowFittingMgmt.Berecne();
		return sictAuswertGbsWindowFittingMgmt.ErgeebnisWindowFittingMgmt;
	}

	public SictAuswertGbsWindowFittingMgmt(UINodeInfoInTree windowAst)
		: base(windowAst)
	{
	}

	public override void Berecne()
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		base.Berecne();
		if (base.Ergeebnis != null)
		{
			LeftSideAst = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && "leftside".EqualsIgnoreCase(kandidaat.Name), 2, 1);
			LeftMainPanelAst = LeftSideAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && "leftMainPanel".EqualsIgnoreCase(kandidaat.Name), 2, 1);
			LeftMainPanelScrollAst = LeftMainPanelAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => "Scroll".EqualsIgnoreCase(kandidaat.PyObjTypName), 3, 1);
			SictAuswertGbsListViewport<IListEntry> sictAuswertGbsListViewport = new SictAuswertGbsListViewport<IListEntry>(LeftMainPanelScrollAst, SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);
			sictAuswertGbsListViewport.Read();
			ErgeebnisWindowFittingMgmt = new WindowFittingMgmt((IWindow)(object)base.Ergeebnis)
			{
				FittingView = (IListViewAndControl)(object)sictAuswertGbsListViewport?.Result
			};
		}
	}
}
