using System;
using System.Collections.Generic;
using System.Linq;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjTuple : SictAuswertPythonObjVar
{
	public long[] ListeItemRef { get; private set; }

	public SictAuswertPythonObjTuple(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 4 * base.ob_size + 12);
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		byte[] listeOktet = base.ListeOktet;
		if (listeOktet != null)
		{
			byte[] array = (listeOktet ?? new byte[0]).Skip(12).ToArray();
			int num = array.Length / 4;
			uint[] array2 = new uint[num];
			Buffer.BlockCopy(listeOktet, 12, array2, 0, array2.Length * 4);
			ListeItemRef = ((IEnumerable<uint>)array2).Select((Func<uint, long>)((uint ItemRef) => ItemRef)).ToArray();
		}
	}
}
