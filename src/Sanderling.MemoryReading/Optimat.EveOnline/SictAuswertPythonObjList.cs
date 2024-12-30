using System;
using System.Collections.Generic;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjList : SictAuswertPythonObjVar
{
	public long Ref_ob_item { get; private set; }

	public int allocated { get; private set; }

	public long[] ListeItemRef { get; private set; }

	public SictAuswertPythonObjList(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 20);
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		Ref_ob_item = UInt32AusListeOktet(12) ?? 0;
		allocated = Int32AusListeOktet(16) ?? 0;
	}

	public override void LaadeReferenziirte(IMemoryReader DaatenKwele)
	{
		if (DaatenKwele == null)
		{
			return;
		}
		long ref_ob_item = Ref_ob_item;
		if (ref_ob_item != 0)
		{
			int num = base.ob_size;
			int num2 = 4;
			byte[] array = Extension.ListeOktetLeeseVonAdrese(DaatenKwele, ref_ob_item, (long)(num * num2), false);
			List<long> list = new List<long>();
			for (int i = 0; i < array.LongLength / num2; i++)
			{
				int beginOktetIndex = i * num2;
				uint num3 = SictAuswertObjMitAdrese.UInt32AusListeOktet(array, beginOktetIndex) ?? 0;
				list.Add(num3);
			}
			ListeItemRef = list.ToArray();
		}
	}
}
