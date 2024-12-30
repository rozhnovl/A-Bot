using System.Collections.Generic;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs;

public class UINodeInfoInTree : GbsAstInfo
{
	public int? InParentListChildIndex;

	public int? InTreeIndex;

	public int? ChildLastInTreeIndex;

	public new UINodeInfoInTree[] ListChild;

	public Vektor2DSingle? FromParentLocation;

	public override IEnumerable<GbsAstInfo> GetListChild()
	{
		return ListChild;
	}
}
