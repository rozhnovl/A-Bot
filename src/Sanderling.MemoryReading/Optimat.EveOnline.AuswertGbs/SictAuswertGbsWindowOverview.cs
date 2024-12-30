using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Sanderling.Interface.MemoryStruct;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowOverview : SictAuswertGbsWindow
{
	private static readonly string AusHeaderCaptionTextTypeSelectionNameRegexPattern = Regex.Escape("Overview (") + "([^" + Regex.Escape(")") + "]+)";

	public static readonly NumberFormatInfo OverviewDistanceNumberFormatInfo = OverviewDistanceNumberFormatInfoErsctele();

	private const string TabNuzbarNitRegexPattern = "^\\s*\\+\\s*$";

	public string TypeSelectionName { get; private set; }

	public UINodeInfoInTree TabGroupAst { get; private set; }

	public SictAuswertGbsTabGroup TabGroupAuswert { get; private set; }

	public UINodeInfoInTree ScrollAst { get; private set; }

	public UINodeInfoInTree ViewportOverallLabelAst { get; private set; }

	public WindowOverView ErgeebnisScpez { get; private set; }

	public new static WindowOverView BerecneFürWindowAst(UINodeInfoInTree windowNode)
	{
		if (windowNode == null)
		{
			return null;
		}
		SictAuswertGbsWindowOverview sictAuswertGbsWindowOverview = new SictAuswertGbsWindowOverview(windowNode);
		sictAuswertGbsWindowOverview.Berecne();
		return sictAuswertGbsWindowOverview.ErgeebnisScpez;
	}

	public SictAuswertGbsWindowOverview(UINodeInfoInTree windowNode)
		: base(windowNode)
	{
	}

	public static NumberFormatInfo OverviewDistanceNumberFormatInfoErsctele()
	{
		NumberFormatInfo numberFormatInfo = CultureInfo.InvariantCulture.NumberFormat.Clone() as NumberFormatInfo;
		numberFormatInfo.NumberGroupSeparator = ".";
		numberFormatInfo.NumberDecimalSeparator = ",";
		return numberFormatInfo;
	}

	public static KeyValuePair<UINodeInfoInTree, T>[] MengeGbsAstZuScpalteIdentBerecneAusMengeGbsAstLaageUndMengeScpalteTitelUndLaage<T>(IEnumerable<KeyValuePair<T, KeyValuePair<int, int>>> mengeScpalteIdentUndLaage, IEnumerable<UINodeInfoInTree> mengeLabelAst)
	{
		if (mengeLabelAst == null || mengeScpalteIdentUndLaage == null)
		{
			return null;
		}
		List<KeyValuePair<UINodeInfoInTree, T>> list = new List<KeyValuePair<UINodeInfoInTree, T>>();
		foreach (UINodeInfoInTree item in mengeLabelAst)
		{
			if (item == null)
			{
				continue;
			}
			Vektor2DSingle? laageInParent = item.LaageInParent;
			Vektor2DSingle? grööse = item.Grööse;
			if (laageInParent.HasValue && grööse.HasValue)
			{
				int num = (int)laageInParent.Value.A;
				int num2 = (int)(laageInParent + grööse).Value.A;
				int zeleBraite = num2 - num;
				KeyValuePair<T, int>? keyValuePair = BerecneÜberlapungGrööste(mengeScpalteIdentUndLaage, num, zeleBraite);
				if (keyValuePair.HasValue && 0 < keyValuePair.Value.Value)
				{
					list.Add(new KeyValuePair<UINodeInfoInTree, T>(item, keyValuePair.Value.Key));
				}
			}
		}
		return list.ToArray();
	}

	public static KeyValuePair<string, KeyValuePair<int, int>>[] MengeSortHeaderTitelUndLaageBerecneAusSortHeaderAst(UINodeInfoInTree inTabSortHeadersAst)
	{
		if (inTabSortHeadersAst == null)
		{
			return null;
		}
		List<KeyValuePair<string, KeyValuePair<int, int>>> list = new List<KeyValuePair<string, KeyValuePair<int, int>>>();
		UINodeInfoInTree[] array = inTabSortHeadersAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer(), null, 3, 1);
		UINodeInfoInTree[] array2 = array;
		foreach (UINodeInfoInTree uINodeInfoInTree in array2)
		{
			Vektor2DSingle? laageInParent = uINodeInfoInTree.LaageInParent;
			Vektor2DSingle? grööse = uINodeInfoInTree.Grööse;
			if (laageInParent.HasValue && grööse.HasValue)
			{
				int num = (int)laageInParent.Value.A;
				int num2 = (int)(laageInParent + grööse).Value.A;
				int value = num2 - num;
				string text = uINodeInfoInTree.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaatLabelAst) => kandidaatLabelAst.GbsAstTypeIstLabel() && true == kandidaatLabelAst.VisibleIncludingInheritance, 1)?.LabelText();
				if (!text.IsNullOrEmpty())
				{
					list.Add(new KeyValuePair<string, KeyValuePair<int, int>>(text, new KeyValuePair<int, int>(num, value)));
				}
			}
		}
		return list.ToArray();
	}

	public static KeyValuePair<T, int>? BerecneÜberlapungGrööste<T>(IEnumerable<KeyValuePair<T, KeyValuePair<int, int>>> mengeScpalteIdentUndLaageLinxUndBraite, int zeleLaageLinx, int zeleBraite)
	{
		if (mengeScpalteIdentUndLaageLinxUndBraite == null)
		{
			return null;
		}
		int val = zeleLaageLinx + zeleBraite;
		KeyValuePair<T, int>? result = null;
		foreach (KeyValuePair<T, KeyValuePair<int, int>> item in mengeScpalteIdentUndLaageLinxUndBraite)
		{
			int key = item.Value.Key;
			int val2 = key + item.Value.Value;
			int num = Math.Max(zeleLaageLinx, key);
			int num2 = Math.Min(val, val2);
			int num3 = num2 - num;
			if (num3 >= 1 && (!result.HasValue || result.Value.Value < num3))
			{
				result = new KeyValuePair<T, int>(item.Key, num3);
			}
		}
		return result;
	}

	public override void Berecne()
	{
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Expected O, but got Unknown
		base.Berecne();
		if (base.Ergeebnis != null)
		{
			Match match = Regex.Match(base.HeaderCaptionText ?? "", AusHeaderCaptionTextTypeSelectionNameRegexPattern, RegexOptions.IgnoreCase);
			if (match.Success)
			{
				TypeSelectionName = match.Groups[1].Value;
			}
			TabGroupAst = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("TabGroup", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("tabparent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			if (TabGroupAst != null)
			{
				TabGroupAuswert = new SictAuswertGbsTabGroup(TabGroupAst);
				TabGroupAuswert.Berecne();
			}
			TabGroup val = ((TabGroupAuswert == null) ? null : TabGroupAuswert.Ergeebnis);
			ScrollAst = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("BasicDynamicScroll", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("overviewscroll2", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			SictAuswertGbsListViewport<IOverviewEntry> sictAuswertGbsListViewport = new SictAuswertGbsListViewport<IOverviewEntry>(ScrollAst, SictAuswertGbsWindowOverviewZaile.OverviewEntryKonstrukt);
			sictAuswertGbsListViewport.Read();
			ViewportOverallLabelAst = sictAuswertGbsListViewport.ScrollClipperContentNode.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => Glob.GbsAstTypeIstEveCaption(kandidaat));
			string viewportOverallLabelString = (((!(ViewportOverallLabelAst?.VisibleIncludingInheritance)) ?? true) ? null : ViewportOverallLabelAst?.LabelText());
			WindowOverView ergeebnisScpez = new WindowOverView((IWindow)(object)base.Ergeebnis)
			{
				PresetTab = val?.ListTab,
				ListView = (IListViewAndControl<IOverviewEntry>)(object)sictAuswertGbsListViewport?.Result,
				ViewportOverallLabelString = viewportOverallLabelString
			};
			ErgeebnisScpez = ergeebnisScpez;
		}
	}

	public static IEnumerable<Tab> ListeTabFiltertNuzbar(IEnumerable<Tab> mengeTab)
	{
		return mengeTab?.Where(delegate(Tab kandidaat)
		{
			IUIElementText label = kandidaat.Label;
			string text = ((label != null) ? label.Text : null);
			if (text.IsNullOrEmpty())
			{
				return false;
			}
			return !Regex.Match(text, "^\\s*\\+\\s*$", RegexOptions.IgnoreCase).Success;
		});
	}
}
