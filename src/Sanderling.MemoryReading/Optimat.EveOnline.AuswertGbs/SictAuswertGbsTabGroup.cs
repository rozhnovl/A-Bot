using System.Linq;
using Bib3.Geometrik;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsTabGroup
{
	public UINodeInfoInTree TabGroupAst { get; private set; }

	public UINodeInfoInTree[] MengeTabAst { get; private set; }

	public SictAuswertGbsTab[] MengeTabAuswert { get; private set; }

	public string FittingBezaicner { get; private set; }

	public bool? Selected { get; private set; }

	public TabGroup Ergeebnis { get; private set; }

	public SictAuswertGbsTabGroup(UINodeInfoInTree tabGroupAst)
	{
		TabGroupAst = tabGroupAst;
	}

	public void Berecne()
	{
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Expected O, but got Unknown
		UINodeInfoInTree tabGroupAst = TabGroupAst;
		if (tabGroupAst == null || true != tabGroupAst.VisibleIncludingInheritance)
		{
			return;
		}
		MengeTabAst = tabGroupAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => "Tab".EqualsIgnoreCase(kandidaat.PyObjTypName), null, 2, 1, omitNodesBelowNodesMatchingPredicate: true);
		MengeTabAuswert = MengeTabAst?.Select(delegate(UINodeInfoInTree tabAst)
		{
			SictAuswertGbsTab sictAuswertGbsTab = new SictAuswertGbsTab(tabAst);
			sictAuswertGbsTab.Berecne();
			return sictAuswertGbsTab;
		})?.ToArray();
		Tab[] MengeTab = MengeTabAuswert?.Select((SictAuswertGbsTab tabAuswert) => tabAuswert.Ergeebnis)?.Where((Tab tab) => tab != null)?.ToArray();
		Tab[] listTab = MengeTab?.OrderBy((Tab tab) => ((UIElement)tab).Region.Value.Center().A)?.ToArray();
		Tab val = ((MengeTab == null) ? null : MengeTab.FirstOrDefault((Tab kandidaatTab) => MengeTab.All((Tab kandidaatKonkurent) => kandidaatKonkurent == kandidaatTab || (kandidaatKonkurent.LabelColorOpacityMilli ?? 0) < kandidaatTab.LabelColorOpacityMilli - 100)));
		TabGroup ergeebnis = new TabGroup(tabGroupAst.AsUIElementIfVisible())
		{
			ListTab = listTab
		};
		Ergeebnis = ergeebnis;
	}
}
