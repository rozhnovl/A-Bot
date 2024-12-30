using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3.Geometrik;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowPeopleAndPlaces : SictAuswertGbsWindow
{
	public WindowPeopleAndPlaces ErgeebnisScpez { get; private set; }

	public new static WindowPeopleAndPlaces BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowPeopleAndPlaces sictAuswertGbsWindowPeopleAndPlaces = new SictAuswertGbsWindowPeopleAndPlaces(windowAst);
		sictAuswertGbsWindowPeopleAndPlaces.Berecne();
		return sictAuswertGbsWindowPeopleAndPlaces.ErgeebnisScpez;
	}

	public SictAuswertGbsWindowPeopleAndPlaces(UINodeInfoInTree windowAst)
		: base(windowAst)
	{
	}

	public override void Berecne()
	{
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		base.Berecne();
		Window ergeebnis = base.Ergeebnis;
		if (ergeebnis != null)
		{
			UINodeInfoInTree[][] array = base.AstMainContainerMain?.ListPathToNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameMatchesRegexPatternIgnoreCase("tab"))?.ToArray();
			UINodeInfoInTree[][] array2 = base.AstMainContainerMain.ListPathToNodeFromSubtreeBreadthFirst((UINodeInfoInTree ast) => ast.PyObjTypNameIsScroll())?.ToArray();
			UINodeInfoInTree[] array3 = base.AstMainContainerMain.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree ast) => ast.PyObjTypNameIsScroll())?.ToArray();
			UINodeInfoInTree uINodeInfoInTree = base.AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => Regex.Match(k?.LabelText() ?? "", "search", RegexOptions.IgnoreCase).Success);
			ListViewAndControl<IListEntry> listView = array3?.FirstOrDefault()?.AlsListView<IListEntry>((Func<UINodeInfoInTree, IColumnHeader[], RectInt?, IListEntry>)SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard, (ListEntryTrenungZeleTypEnum?)null);
			ErgeebnisScpez = new WindowPeopleAndPlaces((IWindow)(object)ergeebnis)
			{
				ListView = (IListViewAndControl)(object)listView
			};
		}
	}
}
