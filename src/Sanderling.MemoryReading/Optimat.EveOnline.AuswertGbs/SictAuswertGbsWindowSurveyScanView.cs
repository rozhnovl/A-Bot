using System;
using System.Linq;
using Bib3.Geometrik;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowSurveyScanView : SictAuswertGbsWindow
{
	public UINodeInfoInTree ScrollAst { get; private set; }

	public UINodeInfoInTree ListAst { get; private set; }

	public SictAuswertGbsListViewport<IListEntry> ListAuswert { get; private set; }

	public UINodeInfoInTree ButtonGroupAst { get; private set; }

	public WindowSurveyScanView ErgeebnisWindowSurveyScanView { get; private set; }

	public new static WindowSurveyScanView BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowSurveyScanView sictAuswertGbsWindowSurveyScanView = new SictAuswertGbsWindowSurveyScanView(windowAst);
		sictAuswertGbsWindowSurveyScanView.Berecne();
		return sictAuswertGbsWindowSurveyScanView.ErgeebnisWindowSurveyScanView;
	}

	public SictAuswertGbsWindowSurveyScanView(UINodeInfoInTree windowAst)
		: base(windowAst)
	{
	}

	public static IListEntry SurveyScanViewEntryKonstrukt(UINodeInfoInTree entryAst, IColumnHeader[] listeScrollHeader, RectInt? regionConstraint)
	{
		if ((!(entryAst?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		GbsAstInfo[] array = entryAst.MengeChildAstTransitiiveHüle()?.ToArray();
		SictAuswertGbsListEntry sictAuswertGbsListEntry = new SictAuswertGbsListEntry(entryAst, listeScrollHeader, regionConstraint, ListEntryTrenungZeleTypEnum.InLabelTab);
		sictAuswertGbsListEntry.Berecne();
		return sictAuswertGbsListEntry?.ErgeebnisListEntry;
	}

	public override void Berecne()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		base.Berecne();
		if (base.Ergeebnis != null)
		{
			ScrollAst = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Scroll", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			ButtonGroupAst = base.AstMainContainer.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("ButtonGroup", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			if (ScrollAst != null)
			{
				ListAuswert = new SictAuswertGbsListViewport<IListEntry>(ScrollAst, SurveyScanViewEntryKonstrukt);
				ListAuswert.Read();
				ErgeebnisWindowSurveyScanView = new WindowSurveyScanView((IWindow)(object)base.Ergeebnis)
				{
					ListView = (IListViewAndControl)(object)ListAuswert.Result
				};
			}
		}
	}
}
