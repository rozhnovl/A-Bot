using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowRegionalMarket : SictAuswertGbsWindow
{
	public WindowRegionalMarket ErgeebnisScpez;

	public new static WindowRegionalMarket BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowRegionalMarket sictAuswertGbsWindowRegionalMarket = new SictAuswertGbsWindowRegionalMarket(windowAst);
		sictAuswertGbsWindowRegionalMarket.Berecne();
		return sictAuswertGbsWindowRegionalMarket.ErgeebnisScpez;
	}

	public SictAuswertGbsWindowRegionalMarket(UINodeInfoInTree windowNode)
		: base(windowNode)
	{
	}

	public static MarketOrderEntry MarketOrderEntryKonstrukt(UINodeInfoInTree entryAst, IColumnHeader[] listeScrollHeader, RectInt? regionConstraint)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		if ((!(entryAst?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		GbsAstInfo[] array = entryAst.MengeChildAstTransitiiveHüle()?.ToArray();
		SictAuswertGbsListEntry sictAuswertGbsListEntry = new SictAuswertGbsListEntry(entryAst, listeScrollHeader, regionConstraint, ListEntryTrenungZeleTypEnum.InLabelTab);
		sictAuswertGbsListEntry.Berecne();
		IListEntry ergeebnisListEntry = sictAuswertGbsListEntry.ErgeebnisListEntry;
		if (ergeebnisListEntry == null)
		{
			return null;
		}
		return new MarketOrderEntry(ergeebnisListEntry);
	}

	public override void Berecne()
	{
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0628: Unknown result type (might be due to invalid IL or missing references)
		//IL_062d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_0669: Unknown result type (might be due to invalid IL or missing references)
		//IL_0671: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Expected O, but got Unknown
		base.Berecne();
		Window ergeebnis = base.Ergeebnis;
		if (ergeebnis != null)
		{
			UINodeInfoInTree[][] array = WindowNode.ListPathToNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => Regex.Match(k.LabelText() ?? "", "iron charge L", RegexOptions.IgnoreCase).Success);
			UINodeInfoInTree[][] array2 = WindowNode.ListPathToNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => Regex.Match(k.LabelText() ?? "", "motsu VII - Moon 6", RegexOptions.IgnoreCase).Success);
			UINodeInfoInTree[][] array3 = WindowNode.ListPathToNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => Regex.Match(k.LabelText() ?? "", "Moon 10 - CONCORD", RegexOptions.IgnoreCase).Success);
			UINodeInfoInTree[] array4 = WindowNode?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree k) => Regex.Match(k.PyObjTypName ?? "", "TabGroup", RegexOptions.IgnoreCase).Success)?.ToArray();
			UINodeInfoInTree[] array5 = WindowNode?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsScroll())?.ToArray();
			UINodeInfoInTree uINodeInfoInTree = array4?.OrderBy((UINodeInfoInTree k) => k.LaagePlusVonParentErbeLaageA())?.FirstOrDefault();
			UINodeInfoInTree tabGroupAst = array4?.Except(uINodeInfoInTree.Yield())?.OrderByDescending((UINodeInfoInTree k) => k.LaagePlusVonParentErbeLaageA())?.FirstOrDefault();
			UINodeInfoInTree listViewNode = array5?.OrderBy((UINodeInfoInTree k) => k.LaagePlusVonParentErbeLaageA())?.FirstOrDefault();
			UINodeInfoInTree uINodeInfoInTree2 = base.AstMainContainerMain?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsContainer() && Regex.Match(k.Name ?? "", "details", RegexOptions.IgnoreCase).Success);
			UINodeInfoInTree uINodeInfoInTree3 = uINodeInfoInTree2?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => Regex.Match(k.PyObjTypName ?? "", "MarketData", RegexOptions.IgnoreCase).Success);
			UINodeInfoInTree listViewNode2 = uINodeInfoInTree3?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsScroll() && Regex.Match(k.Name ?? "", "buy", RegexOptions.IgnoreCase).Success);
			UINodeInfoInTree listViewNode3 = uINodeInfoInTree3?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsScroll() && Regex.Match(k.Name ?? "", "sell", RegexOptions.IgnoreCase).Success);
			UINodeInfoInTree uINodeInfoInTree4 = (base.AstMainContainerMain?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree k) => Regex.Match(k.PyObjTypName ?? "", "MarketOrder", RegexOptions.IgnoreCase).Success))?.FirstOrDefault((UINodeInfoInTree node) => !(node?.Name?.RegexMatchSuccessIgnoreCase("corp") ?? false));
			UINodeInfoInTree listViewNode4 = uINodeInfoInTree4?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsScroll() && k.NameMatchesRegexPatternIgnoreCase("sell"));
			UINodeInfoInTree listViewNode5 = uINodeInfoInTree4?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsScroll() && k.NameMatchesRegexPatternIgnoreCase("buy"));
			SictAuswertGbsTabGroup sictAuswertGbsTabGroup = new SictAuswertGbsTabGroup(uINodeInfoInTree);
			SictAuswertGbsTabGroup sictAuswertGbsTabGroup2 = new SictAuswertGbsTabGroup(tabGroupAst);
			sictAuswertGbsTabGroup.Berecne();
			sictAuswertGbsTabGroup2.Berecne();
			SictAuswertGbsListViewport<IListEntry> sictAuswertGbsListViewport = new SictAuswertGbsListViewport<IListEntry>(listViewNode, SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);
			sictAuswertGbsListViewport.Read();
			SictAuswertGbsListViewport<IListEntry> sictAuswertGbsListViewport2 = new SictAuswertGbsListViewport<IListEntry>(listViewNode2, MarketOrderEntryKonstrukt);
			SictAuswertGbsListViewport<IListEntry> sictAuswertGbsListViewport3 = new SictAuswertGbsListViewport<IListEntry>(listViewNode3, MarketOrderEntryKonstrukt);
			sictAuswertGbsListViewport2.Read();
			sictAuswertGbsListViewport3.Read();
			IUIElement val = uINodeInfoInTree2.AsUIElementIfVisible();
			IUIElement val2 = uINodeInfoInTree3.AsUIElementIfVisible();
			Container val3 = uINodeInfoInTree4.AlsContainer();
			SictAuswertGbsListViewport<IListEntry> sictAuswertGbsListViewport4 = new SictAuswertGbsListViewport<IListEntry>(listViewNode4, MarketOrderEntryKonstrukt);
			SictAuswertGbsListViewport<IListEntry> sictAuswertGbsListViewport5 = new SictAuswertGbsListViewport<IListEntry>(listViewNode5, MarketOrderEntryKonstrukt);
			sictAuswertGbsListViewport4.Read();
			sictAuswertGbsListViewport5.Read();
			MarketItemTypeDetailsMarketData marketData = ((val2 == null) ? ((MarketItemTypeDetailsMarketData)null) : new MarketItemTypeDetailsMarketData(val2)
			{
				SellerView = (IListViewAndControl)(object)sictAuswertGbsListViewport2?.Result,
				BuyerView = (IListViewAndControl)(object)sictAuswertGbsListViewport3?.Result
			});
			MarketMyOrders myOrders = ((val3 == null) ? ((MarketMyOrders)null) : new MarketMyOrders((IContainer)(object)val3)
			{
				SellOrderView = (IListViewAndControl)(object)sictAuswertGbsListViewport4?.Result,
				BuyOrderView = (IListViewAndControl)(object)sictAuswertGbsListViewport5?.Result
			});
			MarketItemTypeDetails selectedItemTypeDetails = ((val == null) ? ((MarketItemTypeDetails)null) : new MarketItemTypeDetails(val)
			{
				MarketData = marketData
			});
			ErgeebnisScpez = new WindowRegionalMarket((IWindow)(object)ergeebnis)
			{
				LeftTabGroup = sictAuswertGbsTabGroup?.Ergeebnis,
				RightTabGroup = sictAuswertGbsTabGroup2?.Ergeebnis,
				QuickbarView = (IListViewAndControl)(object)sictAuswertGbsListViewport?.Result,
				SelectedItemTypeDetails = selectedItemTypeDetails,
				MyOrders = myOrders
			};
		}
	}
}
