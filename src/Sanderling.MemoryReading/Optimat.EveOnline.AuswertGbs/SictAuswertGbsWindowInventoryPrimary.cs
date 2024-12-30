using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowInventoryPrimary : SictAuswertGbsWindow
{
	public static string BottomRightNumItemsLabelTextRegexPattern = "(\\d+)\\s*(|\\([^\\)]+\\))\\s*Items";

	public UINodeInfoInTree AstMainContainerMainDividerCont { get; private set; }

	public UINodeInfoInTree LinxScrollContainerTreeAst { get; private set; }

	public UINodeInfoInTree LinxTreeBehältnisAst { get; private set; }

	public UINodeInfoInTree[] LinxTreeBehältnisListeEntryAst { get; private set; }

	public SictAuswertTreeViewEntry[] LinxTreeBehältnisListeEntryAuswert { get; private set; }

	public UINodeInfoInTree AstMainContainerMainRightCont { get; private set; }

	public UINodeInfoInTree AstMainContainerMainRightContTopRight1 { get; private set; }

	public UINodeInfoInTree AstMainContainerMainRightContTopRight1SubCaptionCont { get; private set; }

	public UINodeInfoInTree AstMainContainerMainRightContTopRight1SubCaptionLabel { get; private set; }

	public string AuswaalReczObjektPfaadSictString { get; private set; }

	public UINodeInfoInTree AuswaalReczInventorySictAst { get; private set; }

	public UINodeInfoInTree[] AuswaalReczInventorySictMengeButtonAst { get; private set; }

	public IUIElement[] AuswaalReczInventorySictMengeButton { get; private set; }

	public UINodeInfoInTree AuswaalReczInventoryAst { get; private set; }

	public UINodeInfoInTree AuswaalReczTop2Ast { get; private set; }

	public UINodeInfoInTree AuswaalReczCapacityGaugeAst { get; private set; }

	public UINodeInfoInTree AuswaalReczCapacityGaugeLabelAst { get; private set; }

	public string AuswaalReczCapacityGaugeLabelText { get; private set; }

	public UINodeInfoInTree AuswaalReczFilterEditAst { get; private set; }

	public UINodeInfoInTree AuswaalReczFilterEditButtonClearAst { get; private set; }

	public UINodeInfoInTree AuswaalReczFilterEditAingaabeTextZiilAst { get; private set; }

	public UINodeInfoInTree AuswaalReczFilterEditLabelAst { get; private set; }

	public UINodeInfoInTree RightContBottomAst { get; private set; }

	public UINodeInfoInTree RightContNumItemsLabelAst { get; private set; }

	public string RightContNumItemsLabelText { get; private set; }

	public SictAuswertGbsInventory AuswaalReczInventoryAuswert { get; private set; }

	public int? AuswaalReczMengeItemAbgebildetAnzaal { get; private set; }

	public int? AuswaalReczMengeItemFilteredAnzaal { get; private set; }

	public WindowInventory ErgeebnisScpez { get; private set; }

	public new static WindowInventory BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowInventoryPrimary sictAuswertGbsWindowInventoryPrimary = new SictAuswertGbsWindowInventoryPrimary(windowAst);
		sictAuswertGbsWindowInventoryPrimary.Berecne();
		return sictAuswertGbsWindowInventoryPrimary.ErgeebnisScpez;
	}

	public SictAuswertGbsWindowInventoryPrimary(UINodeInfoInTree astFensterInventoryPrimary)
		: base(astFensterInventoryPrimary)
	{
	}

	public static int? AusBottomRightNumItemsLabelTextExtraktItemAnzaal(string bottomRightNumItemsLabelText, out int? filteredAnzaal)
	{
		filteredAnzaal = null;
		if (bottomRightNumItemsLabelText == null)
		{
			return null;
		}
		Match match = Regex.Match(bottomRightNumItemsLabelText, BottomRightNumItemsLabelTextRegexPattern, RegexOptions.IgnoreCase);
		if (!match.Success)
		{
			return null;
		}
		int value = int.Parse(match.Groups[1].Value.Trim());
		string value2 = match.Groups[2].Value;
		if (value2 != null && 0 < value2.Length)
		{
			Match match2 = Regex.Match(value2, "(\\d+)\\s*filtered", RegexOptions.IgnoreCase);
			if (!match2.Success)
			{
				return null;
			}
			filteredAnzaal = int.Parse(match2.Groups[1].Value);
		}
		return value;
	}

	public static string[] AuswaalReczObjektPfaadListeAstBerecneAusPfaadSictString(string pfaadSictString)
	{
		return pfaadSictString?.Split(new string[1] { ">" }, StringSplitOptions.RemoveEmptyEntries)?.Select((string ast) => ast.Trim())?.Where((string ast) => !ast.IsNullOrEmpty())?.ToArray();
	}

	public override void Berecne()
	{
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Expected O, but got Unknown
		//IL_0924: Unknown result type (might be due to invalid IL or missing references)
		//IL_0929: Unknown result type (might be due to invalid IL or missing references)
		//IL_093e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0943: Unknown result type (might be due to invalid IL or missing references)
		//IL_094c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0955: Unknown result type (might be due to invalid IL or missing references)
		//IL_0967: Unknown result type (might be due to invalid IL or missing references)
		//IL_0970: Unknown result type (might be due to invalid IL or missing references)
		//IL_0995: Unknown result type (might be due to invalid IL or missing references)
		//IL_099e: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_09cf: Expected O, but got Unknown
		base.Berecne();
		if (base.Ergeebnis == null)
		{
			return;
		}
		AstMainContainerMainDividerCont = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => "dividerCont".EqualsIgnoreCase(kandidaat.Name), 2, 1);
		LinxScrollContainerTreeAst = AstMainContainerMainDividerCont.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => ("ScrollContainerCore".EqualsIgnoreCase(kandidaat.PyObjTypName) || "ScrollContainer".EqualsIgnoreCase(kandidaat.PyObjTypName)) && "tree".EqualsIgnoreCase(kandidaat.Name), 3, 1);
		LinxTreeBehältnisAst = LinxScrollContainerTreeAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => "ContainerAutoSize".EqualsIgnoreCase(kandidaat.PyObjTypName) && "mainCont".EqualsIgnoreCase(kandidaat.Name), 3, 1);
		LinxTreeBehältnisListeEntryAst = LinxTreeBehältnisAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => Regex.Match(kandidaat.PyObjTypName ?? "", "TreeViewEntry", RegexOptions.IgnoreCase).Success, null, 2, 1, omitNodesBelowNodesMatchingPredicate: true);
		UINodeInfoInTree linxScrollContainerTreeAst = LinxScrollContainerTreeAst;
		UINodeInfoInTree uINodeInfoInTree = linxScrollContainerTreeAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsContainer() && k.NameMatchesRegexPatternIgnoreCase("clip"));
		UINodeInfoInTree uINodeInfoInTree2 = linxScrollContainerTreeAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsContainer() && k.NameMatchesRegexPatternIgnoreCase("handle"));
		UINodeInfoInTree uINodeInfoInTree3 = linxScrollContainerTreeAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameMatchesRegexPatternIgnoreCase("Scrollhandle"));
		Scroll leftTreeViewportScroll = ((((!(uINodeInfoInTree?.VisibleIncludingInheritance)) ?? true) && !(uINodeInfoInTree2?.VisibleIncludingInheritance ?? false) && !(uINodeInfoInTree3?.VisibleIncludingInheritance ?? false)) ? ((Scroll)null) : new Scroll(linxScrollContainerTreeAst.AsUIElementIfVisible())
		{
			Clipper = uINodeInfoInTree?.AsUIElementIfVisible(),
			ScrollHandleBound = uINodeInfoInTree2?.AsUIElementIfVisible(),
			ScrollHandle = uINodeInfoInTree3?.AsUIElementIfVisible()
		});
		LinxTreeBehältnisListeEntryAuswert = LinxTreeBehältnisListeEntryAst?.Select(delegate(UINodeInfoInTree ast)
		{
			SictAuswertTreeViewEntry sictAuswertTreeViewEntry = new SictAuswertTreeViewEntry(ast);
			sictAuswertTreeViewEntry.Berecne();
			return sictAuswertTreeViewEntry;
		}).ToArray();
		AstMainContainerMainRightCont = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("rightCont", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		AstMainContainerMainRightContTopRight1 = AstMainContainerMainRightCont.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("topRightcont1", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		AstMainContainerMainRightContTopRight1SubCaptionCont = AstMainContainerMainRightContTopRight1.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("subCaptionCont", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);
		AstMainContainerMainRightContTopRight1SubCaptionLabel = AstMainContainerMainRightContTopRight1SubCaptionCont.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Label", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);
		AuswaalReczInventorySictAst = AstMainContainerMainRightCont.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("InvContViewBtns", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase) || Regex.Match(kandidaat.PyObjTypName ?? "", "InvContViewBtns", RegexOptions.IgnoreCase).Success, 6, 1);
		AuswaalReczInventorySictMengeButtonAst = AuswaalReczInventorySictAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("ButtonIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 3, 1, omitNodesBelowNodesMatchingPredicate: true);
		AuswaalReczInventorySictMengeButton = AuswaalReczInventorySictMengeButtonAst?.Select((UINodeInfoInTree ast) => ast.AsUIElementIfVisible())?.ToArray();
		Sprite[] selectedRightControlViewButton = AuswaalReczInventorySictMengeButtonAst?.Select((UINodeInfoInTree ast) => ast?.MengeChildAstTransitiiveHüle()?.OfType<UINodeInfoInTree>().GröösteSpriteAst()?.AlsSprite())?.WhereNotDefault()?.OrdnungLabel<Sprite>()?.ToArray();
		AuswaalReczInventoryAst = AstMainContainerMainRightCont?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree c) => c != AstMainContainerMainRightCont && c?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree candidateScroll) => candidateScroll?.PyObjTypNameIsScroll() ?? false, 3) != null)?.LargestNodeInSubtree();
		AuswaalReczTop2Ast = AstMainContainerMainRightCont.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("topRightCont2", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 1);
		AuswaalReczCapacityGaugeAst = AuswaalReczTop2Ast.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("InvContCapacityGauge", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 1, 1);
		AuswaalReczCapacityGaugeLabelAst = AuswaalReczCapacityGaugeAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EveLabelSmall", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("capacityText", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 1);
		AuswaalReczFilterEditAst = AuswaalReczTop2Ast.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("SinglelineEdit", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);
		AuswaalReczFilterEditButtonClearAst = AuswaalReczFilterEditAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("ButtonIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);
		AuswaalReczFilterEditLabelAst = AuswaalReczFilterEditAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("edittext", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase) && kandidaat.GbsAstTypeIstLabel(), 3, 1);
		AuswaalReczFilterEditAingaabeTextZiilAst = ((AuswaalReczFilterEditLabelAst == null) ? null : AuswaalReczFilterEditAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && kandidaat.FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(AuswaalReczFilterEditLabelAst.PyObjAddress, 3) != null, 3, 1));
		RightContBottomAst = AstMainContainerMainRightCont.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("bottomRightcont", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);
		UINodeInfoInTree[] array = RightContBottomAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.GbsAstTypeIstLabel() && true == kandidaat.VisibleIncludingInheritance);
		if (array != null)
		{
			UINodeInfoInTree[] array2 = array;
			foreach (UINodeInfoInTree uINodeInfoInTree4 in array2)
			{
				string text = uINodeInfoInTree4.LabelText();
				if (text != null)
				{
					string bottomRightNumItemsLabelText = text.RemoveXmlTag();
					int? filteredAnzaal;
					int? auswaalReczMengeItemAbgebildetAnzaal = AusBottomRightNumItemsLabelTextExtraktItemAnzaal(bottomRightNumItemsLabelText, out filteredAnzaal);
					if (auswaalReczMengeItemAbgebildetAnzaal.HasValue)
					{
						RightContNumItemsLabelAst = uINodeInfoInTree4;
						RightContNumItemsLabelText = text;
						AuswaalReczMengeItemAbgebildetAnzaal = auswaalReczMengeItemAbgebildetAnzaal;
						AuswaalReczMengeItemFilteredAnzaal = filteredAnzaal;
					}
				}
			}
		}
		if (AuswaalReczCapacityGaugeLabelAst != null)
		{
			AuswaalReczCapacityGaugeLabelText = AuswaalReczCapacityGaugeLabelAst.LabelText();
		}
		if (AuswaalReczInventoryAst != null)
		{
			AuswaalReczInventoryAuswert = new SictAuswertGbsInventory(AuswaalReczInventoryAst);
			AuswaalReczInventoryAuswert.Berecne();
		}
		if (AstMainContainerMainRightContTopRight1SubCaptionLabel != null)
		{
			AuswaalReczObjektPfaadSictString = AstMainContainerMainRightContTopRight1SubCaptionLabel.SetText.RemoveXmlTag();
		}
		TreeViewEntry[] leftTreeListEntry = LinxTreeBehältnisListeEntryAuswert?.Select((SictAuswertTreeViewEntry auswert) => auswert.Ergeebnis)?.WhereNotDefault()?.ToArray();
		Inventory selectedRightInventory = AuswaalReczInventoryAuswert?.Ergeebnis;
		IUIElement val = AuswaalReczFilterEditAingaabeTextZiilAst.AsUIElementIfVisible();
		IUIElement selectedRightFilterButtonClear = AuswaalReczFilterEditButtonClearAst.AsUIElementIfVisible();
		string text2 = ((AuswaalReczFilterEditLabelAst == null) ? null : AuswaalReczFilterEditLabelAst.LabelText());
		string[] array3 = AuswaalReczObjektPfaadListeAstBerecneAusPfaadSictString(AuswaalReczObjektPfaadSictString);
		UIElementInputText selectedRightFilterTextBox = ((val == null) ? ((UIElementInputText)null) : new UIElementInputText(val, (string)null)
		{
			Text = text2
		});
		ErgeebnisScpez = new WindowInventory((IWindow)(object)base.Ergeebnis)
		{
			LeftTreeListEntry = (ITreeViewEntry[])(object)leftTreeListEntry,
			LeftTreeViewportScroll = (IScroll)(object)leftTreeViewportScroll,
			SelectedRightInventoryPathLabel = AstMainContainerMainRightContTopRight1SubCaptionLabel.AsUIElementTextIfTextNotEmpty(),
			SelectedRightInventory = (IInventory)(object)selectedRightInventory,
			SelectedRightInventoryCapacity = AuswaalReczCapacityGaugeAst?.ExtraktMengeLabelString()?.FirstOrDefault(),
			SelectedRightControlViewButton = (ISprite[])(object)selectedRightControlViewButton,
			SelectedRightFilterTextBox = (IUIElementInputText)(object)selectedRightFilterTextBox,
			SelectedRightFilterButtonClear = selectedRightFilterButtonClear,
			SelectedRightItemDisplayedCount = AuswaalReczMengeItemAbgebildetAnzaal,
			SelectedRightItemFilteredCount = AuswaalReczMengeItemFilteredAnzaal
		};
	}
}
