using System;
using System.Collections.Generic;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjDict : SictAuswertPythonObj
{
	public int ma_fill { get; private set; }

	public int ma_used { get; private set; }

	public int ma_mask { get; private set; }

	public long Ref_ma_table { get; private set; }

	public SictAuswertPythonDictEntryAinfac[] ListeDictEntry { get; private set; }

	public SictAuswertPythonObjDict(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 24);
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		ma_fill = Int32AusListeOktet(8) ?? 0;
		ma_used = Int32AusListeOktet(12) ?? 0;
		ma_mask = Int32AusListeOktet(16) ?? 0;
		Ref_ma_table = UInt32AusListeOktet(20) ?? 0;
	}

	public override void LaadeReferenziirte(IMemoryReader DaatenKwele)
	{
		LaadeReferenziirte(DaatenKwele, null);
	}

	public void LaadeReferenziirte(IMemoryReader DaatenKwele, int? ListeEntryAnzaalScrankeMax)
	{
		if (DaatenKwele == null)
		{
			return;
		}
		long ref_ma_table = Ref_ma_table;
		int num = ma_mask;
		int num2 = ma_used;
		int num3 = ma_fill;
		if (num < num3 || ref_ma_table == 0)
		{
			return;
		}
		int num4 = num + 1;
		if (num4 < 0 || ListeEntryAnzaalScrankeMax < num4)
		{
			return;
		}
		int num5 = 12;
		byte[] array = Extension.ListeOktetLeeseVonAdrese(DaatenKwele, ref_ma_table, (long)(num4 * num5), false);
		if (array != null)
		{
			List<SictAuswertPythonDictEntryAinfac> list = new List<SictAuswertPythonDictEntryAinfac>();
			num4 = (int)(array.LongLength / num5);
			uint[] array2 = new uint[array.Length / 4];
			Buffer.BlockCopy(array, 0, array2, 0, array2.Length * 4);
			for (int i = 0; i < num4; i++)
			{
				SictAuswertPythonDictEntryAinfac item = new SictAuswertPythonDictEntryAinfac(array2, i);
				list.Add(item);
			}
			ListeDictEntry = list.ToArray();
		}
	}
}
