using System;
using System.Linq;
using System.Text.RegularExpressions;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertTreeViewEntry
{
	public readonly UINodeInfoInTree TreeViewEntryAst;

	public UINodeInfoInTree TopContAst { get; private set; }

	public UINodeInfoInTree TopContLabelAst { get; private set; }

	public UINodeInfoInTree ChildContAst { get; private set; }

	public UINodeInfoInTree[] MengeChildAst { get; private set; }

	public SictAuswertTreeViewEntry[] MengeChildAuswert { get; private set; }

	public UINodeInfoInTree TopContIconAst { get; private set; }

	public UINodeInfoInTree LabelAst { get; private set; }

	public TreeViewEntry Ergeebnis { get; private set; }

	public SictAuswertTreeViewEntry(UINodeInfoInTree TreeViewEntryAst)
	{
		this.TreeViewEntryAst = TreeViewEntryAst;
	}

	public void Berecne()
	{
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Expected O, but got Unknown
		UINodeInfoInTree treeViewEntryAst = TreeViewEntryAst;
		if (treeViewEntryAst == null)
		{
			return;
		}
		TopContAst = treeViewEntryAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.PyObjTypNameIsContainer() && string.Equals("topCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		TopContLabelAst = TopContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.GbsAstTypeIstLabel());
		ChildContAst = treeViewEntryAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.PyObjTypNameIsContainer() && string.Equals("childCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		MengeChildAst = ChildContAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Regex.Match(Kandidaat.PyObjTypName ?? "", "TreeViewEntry", RegexOptions.IgnoreCase).Success, null, 2, 1);
		TopContIconAst = TopContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("Icon", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) || string.Equals("EveIcon", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		UINodeInfoInTree uiNode = TopContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.PyObjTypNameIsContainer() && string.Equals("spacerCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		IUIElement expandToggleButton = uiNode.AsUIElementIfVisible();
		LabelAst = TopContLabelAst;
		if (MengeChildAst != null)
		{
			MengeChildAuswert = MengeChildAst.Select(delegate(UINodeInfoInTree Ast)
			{
				SictAuswertTreeViewEntry sictAuswertTreeViewEntry = new SictAuswertTreeViewEntry(Ast);
				sictAuswertTreeViewEntry.Berecne();
				return sictAuswertTreeViewEntry;
			}).ToArray();
		}
		IUIElement val = (IUIElement)((TopContAst == null) ? ((UIElement)null) : new UIElement(TopContAst.AsUIElementIfVisible()));
		UIElementText val2 = ((TopContLabelAst == null) ? ((UIElementText)null) : new UIElementText(TopContLabelAst.AsUIElementIfVisible(), TopContLabelAst.LabelText()));
		long? num = ((TopContIconAst == null) ? null : TopContIconAst.TextureIdent0);
		ColorORGBVal? val3 = ((TopContIconAst == null) ? null : TopContIconAst.Color);
		string text = ((LabelAst == null) ? null : LabelAst.LabelText());
		TreeViewEntry[] child = ((MengeChildAuswert == null) ? null : (from Auswert in MengeChildAuswert
			select Auswert.Ergeebnis into Kandidaat
			where Kandidaat != null
			select Kandidaat).ToArray());
		TreeViewEntry ergeebnis = new TreeViewEntry((IUIElement)(object)treeViewEntryAst.AlsContainer())
		{
			ExpandToggleButton = expandToggleButton,
			Child = (ITreeViewEntry[])(object)child,
			IsSelected = treeViewEntryAst?.isSelected
		};
		Ergeebnis = ergeebnis;
	}
}
