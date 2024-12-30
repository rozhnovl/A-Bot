using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsAgentEntry
{
	public readonly UINodeInfoInTree AstAgentEntry;

	private static string AusAgentEntryTextAgentTypUndLevelRegexPattern = "Level\\s+([IV]{1,3})\\s*" + Regex.Escape("-") + "\\s*(\\w+)";

	private static string[] AgentLevelString = new string[6] { null, "I", "II", "III", "IV", "V" };

	private static KeyValuePair<string, int?>[] ListeLevelStringMitLevel = AgentLevelString.Select((string String, int Index) => new KeyValuePair<string, int?>(String, Index)).ToArray();

	public UINodeInfoInTree TextContAst { get; private set; }

	public UINodeInfoInTree ButtonStartConversationAst { get; private set; }

	public IUIElement StartConversationButton { get; private set; }

	public UINodeInfoInTree AstTextContText { get; private set; }

	public UINodeInfoInTree[] AstTextContTextMengeLabel { get; private set; }

	public UINodeInfoInTree[] AstTextContTextMengeLabelLinx { get; private set; }

	public LobbyAgentEntry Ergeebnis { get; private set; }

	public SictAuswertGbsAgentEntry(UINodeInfoInTree AstAgentEntry)
	{
		this.AstAgentEntry = AstAgentEntry;
	}

	public static KeyValuePair<string, int>? AgentTypUndLevel(string AusAgentEntryText)
	{
		if (AusAgentEntryText == null)
		{
			return null;
		}
		Match match = Regex.Match(AusAgentEntryText, AusAgentEntryTextAgentTypUndLevelRegexPattern, RegexOptions.IgnoreCase);
		if (!match.Success)
		{
			return null;
		}
		string value = match.Groups[2].Value;
		string LevelSictString = match.Groups[1].Value;
		return new KeyValuePair<string, int>(value, ListeLevelStringMitLevel.FirstOrDefault((KeyValuePair<string, int?> Kandidaat) => string.Equals(Kandidaat.Key, LevelSictString, StringComparison.InvariantCultureIgnoreCase)).Value ?? (-1));
	}

	public void Berecne()
	{
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Expected O, but got Unknown
		if (AstAgentEntry == null || true != AstAgentEntry.VisibleIncludingInheritance)
		{
			return;
		}
		TextContAst = AstAgentEntry.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("textCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		ButtonStartConversationAst = AstAgentEntry.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("ButtonIcon", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && Regex.Match(Kandidaat.Hint ?? "", "Conversation", RegexOptions.IgnoreCase).Success, 2, 1);
		AstTextContText = TextContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("text", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		AstTextContTextMengeLabel = AstTextContText.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("EveLabelMedium", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2, 1);
		StartConversationButton = ButtonStartConversationAst.AsUIElementIfVisible();
		if (AstTextContTextMengeLabel != null)
		{
			AstTextContTextMengeLabel = AstTextContTextMengeLabel.OrderBy(delegate(UINodeInfoInTree Kandidaat)
			{
				Vektor2DSingle? vektor2DSingle = Kandidaat.LaagePlusVonParentErbeLaage();
				return (!vektor2DSingle.HasValue) ? (-1f) : vektor2DSingle.Value.B;
			}).ToArray();
		}
		if (AstTextContTextMengeLabel != null && AstTextContText != null && AstTextContText.Grööse.HasValue)
		{
			AstTextContTextMengeLabelLinx = AstTextContTextMengeLabel.Where((UINodeInfoInTree Kandidaat) => Kandidaat.LaageInParent.HasValue && (double)Kandidaat.LaageInParent.Value.A < (double)AstTextContText.Grööse.Value.A * 0.5).ToArray();
		}
		if (AstTextContTextMengeLabelLinx != null)
		{
			string text = null;
			string text2 = null;
			int? num = null;
			string ausAgentEntryText = null;
			if (AstTextContTextMengeLabelLinx != null && 1 < AstTextContTextMengeLabelLinx.Length)
			{
				text = AstTextContTextMengeLabelLinx.FirstOrDefault().SetText;
				ausAgentEntryText = AstTextContTextMengeLabelLinx.LastOrDefault().SetText;
			}
			KeyValuePair<string, int>? keyValuePair = AgentTypUndLevel(ausAgentEntryText);
			if (keyValuePair.HasValue)
			{
				text2 = keyValuePair.Value.Key;
				num = keyValuePair.Value.Value;
			}
			LobbyAgentEntry ergeebnis = new LobbyAgentEntry(TextContAst.AsUIElementIfVisible())
			{
				LabelText = AstAgentEntry?.ExtraktMengeLabelString()?.OrdnungLabel<IUIElementText>()?.ToArray(),
				StartConversationButton = StartConversationButton
			};
			Ergeebnis = ergeebnis;
		}
	}
}
