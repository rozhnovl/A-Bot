using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowAgentDialogue : SictAuswertGbsWindow
{
	public UINodeInfoInTree LeftPaneAst { get; private set; }

	public UINodeInfoInTree RightPaneAst { get; private set; }

	public UINodeInfoInTree RightPaneBottomAst { get; private set; }

	public UINodeInfoInTree RightPaneButtonGroupAst { get; private set; }

	public UINodeInfoInTree AstRightPaneTop { get; private set; }

	public WindowAgentDialogue ErgeebnisWindowAgentDialogue { get; private set; }

	public new static WindowAgentDialogue BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowAgentDialogue sictAuswertGbsWindowAgentDialogue = new SictAuswertGbsWindowAgentDialogue(windowAst);
		sictAuswertGbsWindowAgentDialogue.Berecne();
		return sictAuswertGbsWindowAgentDialogue.ErgeebnisWindowAgentDialogue;
	}

	public static WindowAgentPane PaneAuswert(UINodeInfoInTree paneAst)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		if ((!(paneAst?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		UINodeInfoInTree uINodeInfoInTree = paneAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k?.PyObjTypNameEqualsIgnoreCase("Edit") ?? false);
		return new WindowAgentPane(paneAst?.AsUIElementIfVisible())
		{
			Html = uINodeInfoInTree?.SrHtmlstr
		};
	}

	public SictAuswertGbsWindowAgentDialogue(UINodeInfoInTree astFensterAgentDialogueWindow)
		: base(astFensterAgentDialogueWindow)
	{
	}

	public override void Berecne()
	{
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		base.Berecne();
		if (base.Ergeebnis != null)
		{
			LeftPaneAst = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("leftPane", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			RightPaneAst = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("rightPane", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			AstRightPaneTop = RightPaneAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("rightPaneTop", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			RightPaneBottomAst = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("rightPaneBottom", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			ErgeebnisWindowAgentDialogue = new WindowAgentDialogue(new WindowAgent((IWindow)(object)base.Ergeebnis))
			{
				LeftPane = (IWindowAgentPane)(object)PaneAuswert(LeftPaneAst),
				RightPane = (IWindowAgentPane)(object)PaneAuswert(RightPaneAst)
			};
		}
	}
}
