using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsInventory
{
	public readonly UINodeInfoInTree InventoryAst;

	public UINodeInfoInTree ListAst { get; private set; }

	public SictAuswertGbsListViewport<IListEntry> ListAuswert { get; private set; }

	public bool? SictwaiseScaintGeseztAufListNict { get; private set; }

	public Inventory Ergeebnis { get; private set; }

	public SictAuswertGbsInventory(UINodeInfoInTree InventoryAst)
	{
		this.InventoryAst = InventoryAst;
	}

	public void Berecne()
	{
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		ListAst = InventoryAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => "Scroll".EqualsIgnoreCase(Kandidaat.PyObjTypName), 1, 1);
		UINodeInfoInTree[] array = InventoryAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => true == Kandidaat.VisibleIncludingInheritance && "InvItem".EqualsIgnoreCase(Kandidaat.PyObjTypName));
		if (array != null)
		{
			UINodeInfoInTree[] array2 = array;
			foreach (UINodeInfoInTree uINodeInfoInTree in array2)
			{
				if (uINodeInfoInTree != null)
				{
					Vektor2DSingle? grööse = uINodeInfoInTree.Grööse;
					if (grööse.HasValue && 44f < grööse.Value.B)
					{
						SictwaiseScaintGeseztAufListNict = true;
						break;
					}
				}
			}
		}
		if (ListAst != null)
		{
			ListAuswert = new SictAuswertGbsListViewport<IListEntry>(ListAst, SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);
			ListAuswert.Read();
			ListViewAndControl<IListEntry> result = ListAuswert.Result;
			if (result != null)
			{
				Inventory ergeebnis = new Inventory(InventoryAst.AsUIElementIfVisible())
				{
					ListView = (IListViewAndControl)(object)result
				};
				Ergeebnis = ergeebnis;
			}
		}
	}
}
