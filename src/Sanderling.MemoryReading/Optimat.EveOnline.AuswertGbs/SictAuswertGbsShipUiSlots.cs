using System;
using System.Linq;
using Bib3;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsShipUiSlots
{
	public readonly UINodeInfoInTree shipUiSlotsNode;

	public UINodeInfoInTree[] MengeKandidaatSlotFenster { get; private set; }

	public SictAuswertGbsShipUiSlotsSlot[] MengeKandidaatSlotAuswert { get; private set; }

	public ShipUiModule[] ListModuleButton { get; private set; }

	public SictAuswertGbsShipUiSlots(UINodeInfoInTree shipUiSlotsNode)
	{
		this.shipUiSlotsNode = shipUiSlotsNode;
	}

	public void Berecne()
	{
		if (shipUiSlotsNode?.VisibleIncludingInheritance ?? false)
		{
			MengeKandidaatSlotFenster = shipUiSlotsNode?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree node) => string.Equals("ShipSlot", node.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2, 1, omitNodesBelowNodesMatchingPredicate: true);
			MengeKandidaatSlotAuswert = MengeKandidaatSlotFenster?.Select(delegate(UINodeInfoInTree kandidaatSlotFenster)
			{
				SictAuswertGbsShipUiSlotsSlot sictAuswertGbsShipUiSlotsSlot = new SictAuswertGbsShipUiSlotsSlot(kandidaatSlotFenster);
				sictAuswertGbsShipUiSlotsSlot.Berecne();
				return sictAuswertGbsShipUiSlotsSlot;
			}).ToArray();
			ListModuleButton = (from slot in MengeKandidaatSlotAuswert?.Select((SictAuswertGbsShipUiSlotsSlot slotAuswert) => slotAuswert.ModuleRepr).WhereNotDefault()
				orderby (slot == null) ? null : Sanderling.Interface.MemoryStruct.Extension.RegionCenter((IUIElement)(object)slot)?.A
				select slot).ToArray();
		}
	}
}
