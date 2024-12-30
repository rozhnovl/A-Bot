using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsModuleButtonTooltip
{
	private static readonly string CellAstNameRegexPattern = "Row(\\d+)_Col(\\d+)";

	public UINodeInfoInTree ModuleButtonHintAst { get; private set; }

	public UINodeInfoInTree[] MengeCellAst { get; private set; }

	public UINodeInfoInTree[][] ListeZaileMengeCellAst { get; private set; }

	public IContainer Ergeebnis { get; private set; }

	public SictAuswertGbsModuleButtonTooltip(UINodeInfoInTree moduleButtonHintNode)
	{
		ModuleButtonHintAst = moduleButtonHintNode;
	}

	public static UINodeInfoInTree[][] ListeZaileMengeCellAstBerecneAusMengeAus(IEnumerable<UINodeInfoInTree> setNode)
	{
		if (setNode == null)
		{
			return null;
		}
		List<KeyValuePair<UINodeInfoInTree, KeyValuePair<int, int>>> list = new List<KeyValuePair<UINodeInfoInTree, KeyValuePair<int, int>>>();
		foreach (UINodeInfoInTree item in setNode)
		{
			if (item == null)
			{
				continue;
			}
			Match match = Regex.Match(item.Name ?? "", CellAstNameRegexPattern, RegexOptions.IgnoreCase);
			if (match.Success)
			{
				int? num = match.Groups[1].Value?.TryParseInt();
				int? num2 = match.Groups[2].Value?.TryParseInt();
				if (num.HasValue && num2.HasValue)
				{
					list.Add(new KeyValuePair<UINodeInfoInTree, KeyValuePair<int, int>>(item, new KeyValuePair<int, int>(num.Value, num2.Value)));
				}
			}
		}
		return (from kandidaat in list
			group kandidaat by kandidaat.Value.Key into grupeRow
			orderby grupeRow.Key
			select grupeRow).ToArray()?.Select((IGrouping<int, KeyValuePair<UINodeInfoInTree, KeyValuePair<int, int>>> grupeRow) => grupeRow.Select((KeyValuePair<UINodeInfoInTree, KeyValuePair<int, int>> cellUndIndex) => cellUndIndex.Key)?.ToArray())?.ToArray();
	}

	public static KeyValuePair<long, string>? AusGbsAstIconMitTextIconIdentUndText(UINodeInfoInTree gbsAst)
	{
		if (gbsAst == null)
		{
			return null;
		}
		UINodeInfoInTree uINodeInfoInTree = gbsAst?.FirstMatchingNodeFromSubtreeBreadthFirst(Glob.GbsAstTypeIstEveIcon, 2, 1);
		UINodeInfoInTree uINodeInfoInTree2 = gbsAst?.FirstMatchingNodeFromSubtreeBreadthFirst(Glob.GbsAstTypeIstEveLabel, 2, 1);
		long? num = uINodeInfoInTree?.PyObjAddress;
		if (!num.HasValue)
		{
			return null;
		}
		string value = uINodeInfoInTree2?.LabelText();
		return new KeyValuePair<long, string>(num.Value, value);
	}

	public void Berecne()
	{
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Expected O, but got Unknown
		if (ModuleButtonHintAst?.VisibleIncludingInheritance ?? false)
		{
			Sprite[] sprite = ModuleButtonHintAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsIcon())?.Select((UINodeInfoInTree k) => k.AlsSprite())?.ToArray();
			Sprite[] array = ((ModuleButtonHintAst.ListPathToNodeFromSubtreeBreadthFirst((UINodeInfoInTree t) => true)?.ToArray())?.Where((UINodeInfoInTree[] path) => path?.LastOrDefault()?.PyObjTypNameIsIcon() ?? false)?.ToArray())?.Select((UINodeInfoInTree[] path) => path?.LastOrDefault()?.AlsSprite())?.ToArray();
			IUIElementText[] array2 = ModuleButtonHintAst?.ExtraktMengeLabelString()?.OrdnungLabel<IUIElementText>()?.ToArrayIfNotEmpty();
			MengeCellAst = ModuleButtonHintAst?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree t) => true, null, 2, 1, omitNodesBelowNodesMatchingPredicate: true);
			ListeZaileMengeCellAst = ListeZaileMengeCellAstBerecneAusMengeAus(MengeCellAst);
			Container val = ModuleButtonHintAst.AlsContainer();
			IUIElementText[] labelText = ModuleButtonHintAst.ExtraktMengeLabelString()?.OrdnungLabel<IUIElementText>()?.ToArray();
			Ergeebnis = (IContainer)new Container(ModuleButtonHintAst.AsUIElementIfVisible())
			{
				LabelText = labelText,
				Sprite = (IEnumerable<ISprite>)(object)sprite
			};
		}
	}
}
