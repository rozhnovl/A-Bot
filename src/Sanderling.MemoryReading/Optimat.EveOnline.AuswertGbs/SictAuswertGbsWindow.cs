using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindow
{
	public readonly UINodeInfoInTree WindowNode;

	private static Regex HeaderButtonTypeRegex = "ButtonIcon".AlsRegexIgnoreCaseCompiled();

	public UINodeInfoInTree AstMainContainer { get; private set; }

	public UINodeInfoInTree AstMainContainerHeaderButtons { get; private set; }

	public UINodeInfoInTree[] AstMainContainerHeaderButtonsMengeKandidaatButton { get; private set; }

	public UINodeInfoInTree AstHeaderButtonClose { get; private set; }

	public UINodeInfoInTree AstHeaderButtonMinimize { get; private set; }

	public UINodeInfoInTree AstMainContainerMain { get; private set; }

	public UINodeInfoInTree AstMainContainerHeaderParent { get; private set; }

	public UINodeInfoInTree AstMainContainerHeaderParentCaptionParent { get; private set; }

	public UINodeInfoInTree MainContainerHeaderParentCaptionParentLabelAst { get; private set; }

	public UINodeInfoInTree MainContainerHeaderParentCaptionParentIcon { get; private set; }

	public string HeaderCaptionText { get; private set; }

	public Window Ergeebnis { get; private set; }

	public static Window BerecneFürWindowAst(UINodeInfoInTree windowNode)
	{
		if (windowNode == null)
		{
			return null;
		}
		SictAuswertGbsWindow sictAuswertGbsWindow = new SictAuswertGbsWindow(windowNode);
		sictAuswertGbsWindow.Berecne();
		return sictAuswertGbsWindow.Ergeebnis;
	}

	public SictAuswertGbsWindow(UINodeInfoInTree windowNode)
	{
		WindowNode = windowNode;
	}

	public virtual void Berecne()
	{
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_0471: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Expected O, but got Unknown
		UINodeInfoInTree windowNode = WindowNode;
		bool? flag = windowNode?.VisibleIncludingInheritance;
		if (!flag.HasValue || flag != true || true != windowNode.ListChild?.Any((UINodeInfoInTree kandidaat) => kandidaat?.VisibleIncludingInheritance ?? false))
		{
			return;
		}
		AstMainContainer = windowNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("__maincontainer", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		AstMainContainerHeaderButtons = AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("headerButtons", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		AstMainContainerHeaderButtonsMengeKandidaatButton = AstMainContainerHeaderButtons?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsButton(), null, 2, 1);
		AstHeaderButtonClose = AstMainContainerHeaderButtonsMengeKandidaatButton?.SuuceFlacMengeAstFrüheste((UINodeInfoInTree kandidaat) => string.Equals("close", kandidaat.Name ?? kandidaat.Hint, StringComparison.InvariantCultureIgnoreCase), 2, 0);
		AstHeaderButtonMinimize = AstMainContainerHeaderButtonsMengeKandidaatButton?.SuuceFlacMengeAstFrüheste((UINodeInfoInTree kandidaat) => string.Equals("minimize", kandidaat.Name ?? kandidaat.Hint, StringComparison.InvariantCultureIgnoreCase), 2, 0);
		AstMainContainerMain = AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("main", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		Container val = windowNode.AlsContainer();
		if (val != null)
		{
			AstMainContainerHeaderParent = AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("headerParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			AstMainContainerHeaderParentCaptionParent = AstMainContainerHeaderParent?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("captionParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			MainContainerHeaderParentCaptionParentLabelAst = AstMainContainerHeaderParentCaptionParent?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EveLabelSmall", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			MainContainerHeaderParentCaptionParentIcon = AstMainContainerHeaderParentCaptionParent?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Icon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) || string.Equals("EveIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			HeaderCaptionText = MainContainerHeaderParentCaptionParentLabelAst?.SetText ?? windowNode?.Caption;
			bool? headerButtonsVisible = AstMainContainerHeaderButtons?.VisibleIncludingInheritance;
			Sprite[] headerButton = AstMainContainerHeaderButtons?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree k) => (k?.VisibleIncludingInheritance ?? false) && k.PyObjTypNameMatchesRegex(HeaderButtonTypeRegex))?.Select((UINodeInfoInTree k) => k.AlsSprite())?.WhereNotDefault()?.ToArrayIfNotEmpty();
			Ergeebnis = new Window((IUIElement)(object)val)
			{
				isModal = windowNode?.isModal,
				Caption = HeaderCaptionText,
				HeaderButtonsVisible = headerButtonsVisible,
				HeaderButton = (ISprite[])(object)headerButton
			};
		}
	}
}
