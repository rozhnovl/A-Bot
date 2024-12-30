using System;
using System.Linq;
using Bib3.Geometrik;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowProbeScanner : SictAuswertGbsWindow
{
	public WindowProbeScanner ErgeebnisWindowProbeScanner { get; private set; }

	public new static WindowProbeScanner BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowProbeScanner sictAuswertGbsWindowProbeScanner = new SictAuswertGbsWindowProbeScanner(windowAst);
		sictAuswertGbsWindowProbeScanner.Berecne();
		return sictAuswertGbsWindowProbeScanner.ErgeebnisWindowProbeScanner;
	}

	public SictAuswertGbsWindowProbeScanner(UINodeInfoInTree windowAst)
		: base(windowAst)
	{
	}

	public override void Berecne()
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		base.Berecne();
		if (base.Ergeebnis != null)
		{
			ListViewAndControl<IListEntry> scanResultView = (base.AstMainContainerMain?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree node) => node?.PyObjTypNameIsScroll() ?? false)?.OrderBy((UINodeInfoInTree node) => node.LaagePlusVonParentErbeLaageB() ?? (-2.1474836E+09f))?.LastOrDefault())?.AlsListView<IListEntry>((Func<UINodeInfoInTree, IColumnHeader[], RectInt?, IListEntry>)((UINodeInfoInTree node, IColumnHeader[] setHeader, RectInt? regionConstraint) => SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard(node, setHeader, regionConstraint, ListEntryTrenungZeleTypEnum.Ast)), (ListEntryTrenungZeleTypEnum?)null);
			ErgeebnisWindowProbeScanner = new WindowProbeScanner((IWindow)(object)base.Ergeebnis)
			{
				ScanResultView = (IListViewAndControl)(object)scanResultView
			};
		}
	}
}
