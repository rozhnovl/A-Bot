using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsListColumnHeader
{
	public static IColumnHeader Read(UINodeInfoInTree ColumnHeaderAst)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		if ((!(ColumnHeaderAst?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		Container val = ColumnHeaderAst?.AlsContainer(treatIconAsSprite: true);
		if (val == null)
		{
			return null;
		}
		return (IColumnHeader)new ColumnHeader((IContainer)(object)val);
	}
}
