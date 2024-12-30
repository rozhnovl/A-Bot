using System;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowStack : SictAuswertGbsWindow
{
	public UINodeInfoInTree TabGroupAst { get; private set; }

	public UINodeInfoInTree ContentAst { get; private set; }

	public UINodeInfoInTree KandidaatWindowAktiivAst { get; private set; }

	public SictAuswertGbsTabGroup TabGroupAuswert { get; private set; }

	public WindowStack ErgeebnisScpezStack { get; private set; }

	public new static WindowStack BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowStack sictAuswertGbsWindowStack = new SictAuswertGbsWindowStack(windowAst);
		sictAuswertGbsWindowStack.Berecne();
		return sictAuswertGbsWindowStack.ErgeebnisScpezStack;
	}

	public SictAuswertGbsWindowStack(UINodeInfoInTree windowStackAst)
		: base(windowStackAst)
	{
	}

	public override void Berecne()
	{
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Expected O, but got Unknown
		base.Berecne();
		UINodeInfoInTree astMainContainer = base.AstMainContainer;
		if (astMainContainer != null && true == astMainContainer.VisibleIncludingInheritance)
		{
			TabGroupAst = astMainContainer.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("TabGroup", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4, 1);
			ContentAst = astMainContainer.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && Regex.Match(kandidaat.Name ?? "", "content", RegexOptions.IgnoreCase).Success, 2, 1);
			KandidaatWindowAktiivAst = ContentAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => true == kandidaat.VisibleIncludingInheritance && kandidaat.Caption != null, 2, 1);
			if (TabGroupAst != null)
			{
				TabGroupAuswert = new SictAuswertGbsTabGroup(TabGroupAst);
				TabGroupAuswert.Berecne();
			}
			TabGroup val = ((TabGroupAuswert == null) ? null : TabGroupAuswert.Ergeebnis);
			Window tabSelectedWindow = Glob.WindowBerecneScpezTypFürGbsAst(KandidaatWindowAktiivAst);
			ErgeebnisScpezStack = new WindowStack((IWindow)(object)base.Ergeebnis)
			{
				Tab = val?.ListTab,
				TabSelectedWindow = (IWindow)(object)tabSelectedWindow
			};
		}
	}
}
