using System;
using System.Collections.Generic;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjBunch : SictAuswertPythonObj
{
	public const int AnnaameDictEntryAnzaalOktetIndex = 16;

	public const int AnnaameDictAdreseOktetIndex = 20;

	public int? AnnaameDictEntryAnzaal { get; private set; }

	public long? AnnaameRefListeDictEntry { get; private set; }

	public SictAuswertPythonDictEntryAinfac[] ListeEntry { get; private set; }

	public SictAuswertPythonObjBunch(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 132);
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		AnnaameDictEntryAnzaal = Int32AusListeOktet(16);
		AnnaameRefListeDictEntry = UInt32AusListeOktet(20);
	}

	public void LaadeReferenziirte(IMemoryReader DaatenKwele, int? ListeEntryAnzaalScrankeMax)
	{
		if (DaatenKwele == null)
		{
			return;
		}
		long? annaameRefListeDictEntry = AnnaameRefListeDictEntry;
		int? annaameDictEntryAnzaal = AnnaameDictEntryAnzaal;
		if (!annaameRefListeDictEntry.HasValue || !annaameDictEntryAnzaal.HasValue)
		{
			return;
		}
		int value = annaameDictEntryAnzaal.Value;
		if (value < 0 || ListeEntryAnzaalScrankeMax < value)
		{
			return;
		}
		int num = 12;
		byte[] array = Extension.ListeOktetLeeseVonAdrese(DaatenKwele, annaameRefListeDictEntry.Value, (long)(value * num), false);
		if (array != null)
		{
			List<SictAuswertPythonDictEntryAinfac> list = new List<SictAuswertPythonDictEntryAinfac>();
			value = (int)(array.LongLength / num);
			uint[] array2 = new uint[array.Length / 4];
			Buffer.BlockCopy(array, 0, array2, 0, array2.Length * 4);
			for (int i = 0; i < value; i++)
			{
				SictAuswertPythonDictEntryAinfac item = new SictAuswertPythonDictEntryAinfac(array2, i);
				list.Add(item);
			}
			ListeEntry = list.ToArray();
		}
	}
}
