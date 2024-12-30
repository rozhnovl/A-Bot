using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowOverviewZaile
{
	public const string MainIconPyTypeName = "SpaceObjectIcon";

	public readonly UINodeInfoInTree WindowOverviewZaile;

	public readonly string WindowOverviewTypeSelectionName;

	public readonly IEnumerable<KeyValuePair<string, KeyValuePair<int, int>>> MengeSortHeaderTitelUndLaage;

	private const string MainIconIndicatorKeyRegexPattern = "\\w+Indicator";

	public UINodeInfoInTree AstIconContainer { get; private set; }

	public UINodeInfoInTree[] MengeFillAst { get; private set; }

	public UINodeInfoInTree AstIconContainerIconMain { get; private set; }

	public UINodeInfoInTree RightAlignedIconContainerAst { get; private set; }

	public UINodeInfoInTree[] RightAlignedIconContainerMengeIconAst { get; private set; }

	public UINodeInfoInTree AstIconContainerIconTargetingIndicator { get; private set; }

	public UINodeInfoInTree AstIconContainerIconAttackingMe { get; private set; }

	public UINodeInfoInTree AstIconContainerIconHostile { get; private set; }

	public UINodeInfoInTree AstIconContainerIconMyActiveTargetIndicator { get; private set; }

	public UINodeInfoInTree AstIconContainerIconTargetedByMeIndicator { get; private set; }

	public OverviewEntry Ergeebnis { get; private set; }

	public SictAuswertGbsWindowOverviewZaile(UINodeInfoInTree FensterOverviewZaile, string WindowOverviewTypeSelectionName, IEnumerable<KeyValuePair<string, KeyValuePair<int, int>>> MengeSortHeaderTitelUndLaage)
	{
		WindowOverviewZaile = FensterOverviewZaile;
		this.WindowOverviewTypeSelectionName = WindowOverviewTypeSelectionName;
		this.MengeSortHeaderTitelUndLaage = MengeSortHeaderTitelUndLaage;
	}

	public void Berecne()
	{
		UINodeInfoInTree windowOverviewZaile = WindowOverviewZaile;
		if (windowOverviewZaile == null || true != windowOverviewZaile.VisibleIncludingInheritance)
		{
			return;
		}
		UINodeInfoInTree[] listChild = windowOverviewZaile.ListChild;
		if (listChild == null)
		{
			return;
		}
		UINodeInfoInTree[] mengeLabelAst = (from Kandidaat in listChild.Where(delegate(UINodeInfoInTree Kandidaat)
			{
				if (Kandidaat == null)
				{
					return false;
				}
				string pyObjTypName = Kandidaat.PyObjTypName;
				return string.Equals("OverviewLabel", pyObjTypName, StringComparison.InvariantCultureIgnoreCase);
			})
			where Kandidaat.LaageInParent.HasValue
			orderby Kandidaat.LaageInParent.Value.A
			select Kandidaat).ToArray();
		MengeFillAst = windowOverviewZaile.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => (string.Equals("PyFill", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) || string.Equals("Fill", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)) && true == Kandidaat.VisibleIncludingInheritance && Kandidaat.Color.HasValue, null, 2, 1);
		AstIconContainer = windowOverviewZaile.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("SpaceObjectIcon", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2);
		RightAlignedIconContainerAst = windowOverviewZaile.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("rightAlignedIconContainer", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		RightAlignedIconContainerMengeIconAst = RightAlignedIconContainerAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("Icon", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) || string.Equals("EveIcon", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2, 1);
		AstIconContainerIconMain = AstIconContainer.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("iconSprite", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		AstIconContainerIconTargetingIndicator = AstIconContainer.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("targeting", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		AstIconContainerIconTargetedByMeIndicator = AstIconContainer.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("targetedByMeIndicator", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		AstIconContainerIconMyActiveTargetIndicator = AstIconContainer.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("myActiveTargetIndicator", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		AstIconContainerIconAttackingMe = AstIconContainer.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("attackingMe", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		AstIconContainerIconHostile = AstIconContainer.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("hostile", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		KeyValuePair<string, string>[] array = SictAuswertGbsWindowOverview.MengeGbsAstZuScpalteIdentBerecneAusMengeGbsAstLaageUndMengeScpalteTitelUndLaage(MengeSortHeaderTitelUndLaage, mengeLabelAst)?.Select((KeyValuePair<UINodeInfoInTree, string> ZeleGbsAstUndScpalteTitel) => new KeyValuePair<string, string>(ZeleGbsAstUndScpalteTitel.Key.Text, ZeleGbsAstUndScpalteTitel.Value))?.ToArray();
		long? num = null;
		ColorORGB val = null;
		if (AstIconContainerIconMain != null)
		{
			num = AstIconContainerIconMain.TextureIdent0;
			val = ColorORGB.VonVal(AstIconContainerIconMain.Color);
		}
		long[] array2 = null;
		if (RightAlignedIconContainerMengeIconAst != null)
		{
			array2 = (from IconAst in RightAlignedIconContainerMengeIconAst
				select IconAst.TextureIdent0 into Kandidaat
				where Kandidaat.HasValue
				select Kandidaat.Value).ToArray();
		}
	}

	public static IOverviewEntry OverviewEntryKonstrukt(UINodeInfoInTree EntryAst, IColumnHeader[] ListeScrollHeader, RectInt? regionConstraint)
	{
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Expected O, but got Unknown
		if ((!(EntryAst?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		SictAuswertGbsListEntry sictAuswertGbsListEntry = new SictAuswertGbsListEntry(EntryAst, ListeScrollHeader, regionConstraint, ListEntryTrenungZeleTypEnum.Ast);
		sictAuswertGbsListEntry.Berecne();
		IListEntry ergeebnisListEntry = sictAuswertGbsListEntry.ErgeebnisListEntry;
		if (ergeebnisListEntry == null)
		{
			return null;
		}
		UINodeInfoInTree uINodeInfoInTree = EntryAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree c) => c?.PyObjTypNameMatchesRegexPatternIgnoreCase("SpaceObjectIcon") ?? false);
		Sprite[] rightIcon = (EntryAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsContainer() && Regex.Match(k?.Name ?? "", "right.*icon", RegexOptions.IgnoreCase).Success))?.MatchingNodesFromSubtreeBreadthFirst(Glob.PyObjTypNameIsIcon)?.Select(Extension.AlsSprite)?.WhereNotDefault()?.OrdnungLabel<Sprite>()?.ToArray();
		string[] mainIconSetIndicatorName = uINodeInfoInTree?.DictListKeyStringValueNotEmpty?.Where((string key) => key.RegexMatchSuccessIgnoreCase("\\w+Indicator"))?.ToArrayIfNotEmpty();
		return (IOverviewEntry)new OverviewEntry(ergeebnisListEntry)
		{
			MainIconSetIndicatorName = mainIconSetIndicatorName,
			RightIcon = (ISprite[])(object)rightIcon
		};
	}
}
