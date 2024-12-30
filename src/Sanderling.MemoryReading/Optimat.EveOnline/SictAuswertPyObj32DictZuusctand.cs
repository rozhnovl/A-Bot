using System.Collections.Generic;
using System.Runtime.InteropServices;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32DictZuusctand : SictAuswertPyObj32Zuusctand
{
	public int ListeEntryAnzaalScrankeMax;

	private SictAuswertPyObj32Zuusctand MaTableZwiscenscpaicer;

	public SictPyDictEntry32[] ListeDictEntry { get; private set; }

	public int ma_fill => base.ObjektBegin.BaiPlus8Int32;

	public int ma_used => base.ObjektBegin.BaiPlus12Int32;

	public int ma_mask => base.ObjektBegin.BaiPlus16Int32;

	public long Ref_ma_table => base.ObjektBegin.BaiPlus20UInt32;

	public SictAuswertPyObj32DictZuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
		ObjektListeOktetAnzaal = 32;
	}

	public override void Aktualisiire(IMemoryReader AusProzesLeeser, out bool Geändert, long Zait, int? ZuLeeseListeOktetAnzaal = null)
	{
		Geändert = false;
		int listeEntryAnzaalScrankeMax = ListeEntryAnzaalScrankeMax;
		SictPyObjAusrictCPython32Erwaitert1 objektBegin = base.ObjektBegin;
		long ref_ma_table = Ref_ma_table;
		base.Aktualisiire(AusProzesLeeser, out var _, Zait, ZuLeeseListeOktetAnzaal);
		SictPyObjAusrictCPython32Erwaitert1 objektBegin2 = base.ObjektBegin;
		long ref_ma_table2 = Ref_ma_table;
		int num = ma_mask;
		int num2 = ma_used;
		int num3 = ma_fill;
		SictPyDictEntry32[] listeDictEntry = null;
		try
		{
			if (AusProzesLeeser == null || num < num3 || ref_ma_table2 == 0)
			{
				return;
			}
			int num4 = num + 1;
			if (num4 < 0 || listeEntryAnzaalScrankeMax < num4)
			{
				return;
			}
			SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand = MaTableZwiscenscpaicer;
			if (sictAuswertPyObj32Zuusctand != null && sictAuswertPyObj32Zuusctand.HerkunftAdrese != ref_ma_table2)
			{
				sictAuswertPyObj32Zuusctand = null;
			}
			if (sictAuswertPyObj32Zuusctand == null)
			{
				sictAuswertPyObj32Zuusctand = new SictAuswertPyObj32Zuusctand(ref_ma_table2, Zait);
			}
			MaTableZwiscenscpaicer = sictAuswertPyObj32Zuusctand;
			int num5 = 12;
			sictAuswertPyObj32Zuusctand.Aktualisiire(AusProzesLeeser, out var _, Zait, num4 * num5);
			KeyValuePair<byte[], int> ausScpaicerLeeseLezteListeOktetUndAnzaal = sictAuswertPyObj32Zuusctand.AusScpaicerLeeseLezteListeOktetUndAnzaal;
			byte[] key = ausScpaicerLeeseLezteListeOktetUndAnzaal.Key;
			if (key != null && 0 == 0)
			{
				int num6 = ausScpaicerLeeseLezteListeOktetUndAnzaal.Value / num5;
				SictPyDictEntry32[] array = new SictPyDictEntry32[num6];
				GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				try
				{
					Marshal.Copy(key, 0, gCHandle.AddrOfPinnedObject(), array.Length * num5);
				}
				finally
				{
					gCHandle.Free();
				}
				listeDictEntry = array;
			}
		}
		finally
		{
			ListeDictEntry = listeDictEntry;
		}
	}
}
