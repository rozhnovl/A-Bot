using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowStation : SictAuswertGbsWindow
{
	public string ButtonUndockLabelText { get; private set; }

	public UINodeInfoInTree AgentsPanelAst { get; private set; }

	public UINodeInfoInTree AgentsPanelScrollAst { get; private set; }

	public ScrollReader AgentsPanelScrollAuswert { get; private set; }

	public UINodeInfoInTree AgentsPanelScrollContentAst { get; private set; }

	public UINodeInfoInTree[] MengeAgentEntryHeaderKandidaatAst { get; private set; }

	public KeyValuePair<int, string>[] MengeAgentEntryHeaderLaageMitText { get; private set; }

	public UINodeInfoInTree[] MengeAgentEntryKandidaatAst { get; private set; }

	public SictAuswertGbsAgentEntry[] MengeAgentEntryKandidaatAuswert { get; private set; }

	public WindowStation ErgeebnisScpez { get; private set; }

	public new static WindowStation BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowStation sictAuswertGbsWindowStation = new SictAuswertGbsWindowStation(windowAst);
		sictAuswertGbsWindowStation.Berecne();
		return sictAuswertGbsWindowStation.ErgeebnisScpez;
	}

	public SictAuswertGbsWindowStation(UINodeInfoInTree astFensterStationLobby)
		: base(astFensterStationLobby)
	{
	}

	public override void Berecne()
	{
		//IL_05b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Expected O, but got Unknown
		base.Berecne();
		UINodeInfoInTree undockButtonNode = base.AstMainContainerMain?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree c) => c?.PyObjTypName?.RegexMatchSuccessIgnoreCase("UndockBtn") ?? false);
		UINodeInfoInTree uINodeInfoInTree = undockButtonNode?.LargestLabelInSubtree();
		UINodeInfoInTree uINodeInfoInTree2 = undockButtonNode?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree c) => c?.PyObjAddress != undockButtonNode?.PyObjAddress && (c?.PyObjTypNameIsContainer() ?? false))?.LargestNodeInSubtree();
		Sprite[] serviceButton = (base.AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsContainer() && k.NameMatchesRegexPatternIgnoreCase("service.*Button")))?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsButton())?.Select((UINodeInfoInTree buttonAst) => buttonAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree spriteAst) => spriteAst.PyObjTypNameIsSprite()))?.WhereNotDefault()?.Select(Extension.AlsSprite)?.OrdnungLabel<Sprite>()?.ToArray();
		AgentsPanelAst = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("agentsPanel", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		AgentsPanelScrollAst = AgentsPanelAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Scroll", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		AgentsPanelScrollAuswert = new ScrollReader(AgentsPanelScrollAst);
		AgentsPanelScrollAuswert.Read();
		AgentsPanelScrollContentAst = AgentsPanelScrollAuswert.ClipperContentNode;
		MengeAgentEntryHeaderKandidaatAst = AgentsPanelScrollContentAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Header", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2, 1);
		MengeAgentEntryKandidaatAst = AgentsPanelScrollContentAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("AgentEntry", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2, 1);
		if (MengeAgentEntryHeaderKandidaatAst != null)
		{
			MengeAgentEntryHeaderLaageMitText = (from kandidaat in MengeAgentEntryHeaderKandidaatAst.Select(delegate(UINodeInfoInTree gbsAst)
				{
					string value = null;
					int key = -1;
					RectInt? rectInt = Glob.FläceAusGbsAstInfoMitVonParentErbe(gbsAst);
					if ((rectInt?.Center()).HasValue)
					{
						key = (int)rectInt.Value.Center().B;
					}
					UINodeInfoInTree uINodeInfoInTree3 = gbsAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EveLabelMedium", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
					if (uINodeInfoInTree3 != null)
					{
						value = uINodeInfoInTree3.SetText;
					}
					return new KeyValuePair<int, string>(key, value);
				})
				where kandidaat.Value != null
				orderby kandidaat.Key
				select kandidaat).ToArray();
		}
		if (MengeAgentEntryKandidaatAst != null)
		{
			MengeAgentEntryKandidaatAuswert = MengeAgentEntryKandidaatAst.Select(delegate(UINodeInfoInTree gbsAst)
			{
				SictAuswertGbsAgentEntry sictAuswertGbsAgentEntry = new SictAuswertGbsAgentEntry(gbsAst);
				sictAuswertGbsAgentEntry.Berecne();
				return sictAuswertGbsAgentEntry;
			}).ToArray();
		}
		if (MengeAgentEntryKandidaatAuswert != null && MengeAgentEntryHeaderLaageMitText != null)
		{
			SictAuswertGbsAgentEntry[] mengeAgentEntryKandidaatAuswert = MengeAgentEntryKandidaatAuswert;
			foreach (SictAuswertGbsAgentEntry sictAuswertGbsAgentEntry2 in mengeAgentEntryKandidaatAuswert)
			{
				if (sictAuswertGbsAgentEntry2.Ergeebnis != null)
				{
					RectInt InGbsFläce = ((UIElement)sictAuswertGbsAgentEntry2.Ergeebnis).Region.Value;
					bool flag = false;
					KeyValuePair<int, string> keyValuePair = MengeAgentEntryHeaderLaageMitText.LastOrDefault((KeyValuePair<int, string> kandidaat) => kandidaat.Key < InGbsFläce.Center().B);
				}
			}
		}
		ButtonUndockLabelText = uINodeInfoInTree?.LabelText();
		LobbyAgentEntry[] agentEntry = MengeAgentEntryKandidaatAuswert?.Select((SictAuswertGbsAgentEntry auswert) => auswert.Ergeebnis)?.WhereNotDefault()?.OrdnungLabel<LobbyAgentEntry>()?.ToArray();
		IUIElementText[] agentEntryHeader = MengeAgentEntryHeaderKandidaatAst?.Select((UINodeInfoInTree headerAst) => headerAst?.LargestLabelInSubtree()?.AsUIElementTextIfTextNotEmpty())?.WhereNotDefault()?.OrdnungLabel<IUIElementText>()?.ToArray();
		IUIElement undockButton = uINodeInfoInTree2?.AsUIElementIfVisible();
		bool? flag2 = ((ButtonUndockLabelText == null) ? null : new bool?(Regex.Match(ButtonUndockLabelText, "abort undock", RegexOptions.IgnoreCase).Success || Regex.Match(ButtonUndockLabelText, "undocking", RegexOptions.IgnoreCase).Success));
		Window ergeebnis = base.Ergeebnis;
		IUIElementText[] aboveServicesLabel = ((ergeebnis == null) ? null : ((Container)ergeebnis).LabelText?.Where((IUIElementText k) => (float)((IUIElement)k).Region.Value.Center().B < undockButtonNode?.LaagePlusVonParentErbeLaageB() + undockButtonNode?.GrööseB)?.ToArray());
		WindowStation ergeebnisScpez = new WindowStation((IWindow)(object)base.Ergeebnis)
		{
			AboveServicesLabel = aboveServicesLabel,
			UndockButton = undockButton,
			ServiceButton = (ISprite[])(object)serviceButton,
			AgentEntry = agentEntry,
			AgentEntryHeader = agentEntryHeader
		};
		ErgeebnisScpez = ergeebnisScpez;
	}
}
