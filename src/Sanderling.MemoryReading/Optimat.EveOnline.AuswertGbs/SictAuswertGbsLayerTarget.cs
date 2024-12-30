using System;
using System.Linq;
using Bib3;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsLayerTarget
{
	public readonly UINodeInfoInTree LayerTargetNode;

	public UINodeInfoInTree[] SetTargetNode { get; private set; }

	public SictAuswertGbsTarget[] MengeFensterTargetAuswert { get; private set; }

	public ShipUiTarget[] SetTarget { get; private set; }

	public SictAuswertGbsLayerTarget(UINodeInfoInTree layerTargetNode)
	{
		LayerTargetNode = layerTargetNode;
	}

	public void Berecne()
	{
		if (LayerTargetNode?.VisibleIncludingInheritance ?? false)
		{
			SetTargetNode = LayerTargetNode.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("TargetInBar", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 4);
			MengeFensterTargetAuswert = SetTargetNode.Select((UINodeInfoInTree targetNode) => new SictAuswertGbsTarget(targetNode)).ToArray();
			SictAuswertGbsTarget[] mengeFensterTargetAuswert = MengeFensterTargetAuswert;
			foreach (SictAuswertGbsTarget sictAuswertGbsTarget in mengeFensterTargetAuswert)
			{
				sictAuswertGbsTarget.Berecne();
			}
			SetTarget = MengeFensterTargetAuswert?.Select((SictAuswertGbsTarget targetAuswert) => targetAuswert.Ergeebnis)?.WhereNotDefault()?.ToArray();
		}
	}
}
