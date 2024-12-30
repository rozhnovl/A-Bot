using System.Collections.Generic;

namespace Optimat.EveOnline;

public class SictNaacHerkunftAdrese : IComparer<SictAuswertObjMitAdrese>
{
	public int Compare(SictAuswertObjMitAdrese a, SictAuswertObjMitAdrese b)
	{
		if (a == null && b == null)
		{
			return 0;
		}
		if (a == null)
		{
			return -1;
		}
		if (b == null)
		{
			return 1;
		}
		long herkunftAdrese = a.HerkunftAdrese;
		long herkunftAdrese2 = b.HerkunftAdrese;
		if (herkunftAdrese == herkunftAdrese2)
		{
			return 0;
		}
		return (herkunftAdrese >= herkunftAdrese2) ? 1 : (-1);
	}
}
