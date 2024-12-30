using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertNeocom
{
	public readonly UINodeInfoInTree NeocomAst;

	private static Regex EveMenuButtonPyTypeRegex = "ButtonEveMenu".AlsRegexIgnoreCaseCompiled();

	public UINodeInfoInTree NeocomMainContAst { get; private set; }

	public UINodeInfoInTree NeocomMainContButtonContAst { get; private set; }

	public UINodeInfoInTree NeocomClockLabelAst { get; private set; }

	public IUIElementText NeocomClockBescriftung { get; private set; }

	public UINodeInfoInTree NeocomCharContAst { get; private set; }

	public UINodeInfoInTree NeocomCharPicAst { get; private set; }

	public Neocom Ergeebnis { get; private set; }

	public SictAuswertNeocom(UINodeInfoInTree NeocomAst)
	{
		this.NeocomAst = NeocomAst;
	}

	public void Berecne()
	{
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Expected O, but got Unknown
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Expected O, but got Unknown
		if (NeocomAst == null || true != NeocomAst.VisibleIncludingInheritance)
		{
			return;
		}
		NeocomMainContAst = NeocomAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("mainCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		NeocomMainContButtonContAst = NeocomMainContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("buttonCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		IUIElement eveMenuButton = NeocomMainContAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameMatchesRegex(EveMenuButtonPyTypeRegex))?.AsUIElementIfVisible();
		IUIElement charButton = NeocomMainContAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsButton() && k.NameMatchesRegexPatternIgnoreCase("charSheet"))?.AsUIElementIfVisible();
		UINodeInfoInTree[] array = NeocomMainContButtonContAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsButton())?.ToArray();
		UIElementText[] array2 = array?.Where((UINodeInfoInTree ButtonAst) => ButtonAst?.VisibleIncludingInheritance ?? false)?.Select((Func<UINodeInfoInTree, UIElementText>)((UINodeInfoInTree ButtonAst) => new UIElementText(ButtonAst.AsUIElementIfVisible(), ButtonAst?.Name)))?.OrdnungLabel<UIElementText>()?.ToArray();
		Sprite[] button = array?.Select((UINodeInfoInTree ButtonAst) => ButtonAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsSprite()))?.WhereNotDefault()?.Select(Extension.AlsSprite)?.OrdnungLabel<Sprite>()?.ToArray();
		NeocomClockLabelAst = NeocomMainContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("clockLabel", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 5, 1);
		if (NeocomClockLabelAst != null)
		{
			NeocomClockBescriftung = (IUIElementText)new UIElementText(NeocomClockLabelAst.AsUIElementIfVisible(), NeocomClockLabelAst.LabelText()?.RemoveXmlTag());
		}
		NeocomCharContAst = NeocomMainContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("charCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);
		Ergeebnis = new Neocom(NeocomAst.AsUIElementIfVisible())
		{
			EveMenuButton = eveMenuButton,
			CharButton = charButton,
			Button = (ISprite[])(object)button,
			Clock = NeocomClockBescriftung
		};
	}
}
