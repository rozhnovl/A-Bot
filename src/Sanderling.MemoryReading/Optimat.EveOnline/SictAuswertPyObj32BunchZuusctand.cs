using System.Runtime.InteropServices;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32BunchZuusctand : SictAuswertPyObj32Zuusctand
{
	public const int AnnaameDictEntryAnzaalOktetIndex = 16;

	public const int AnnaameDictAdreseOktetIndex = 20;

	public int? ListeEntryAnzaalScrankeMax = 1024;

	public int? AnnaameDictEntryAnzaal { get; private set; }

	public long? AnnaameRefListeDictEntry { get; private set; }

	public SictPyDictEntry32[] ListeDictEntry { get; private set; }

	public SictAuswertPyObj32BunchZuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader AusProzesLeeser, out bool Geändert, long Zait, int? ZuLeeseListeOktetAnzaal = null)
	{
		ObjektListeOktetAnzaal = SictAuswertPyObj32Zuusctand.ObjektBeginListeOktetAnzaal;
		base.Aktualisiire(AusProzesLeeser, out Geändert, Zait, ZuLeeseListeOktetAnzaal);
		SictPyObjAusrictCPython32Erwaitert1 objektBegin = base.ObjektBegin;
		int baiPlus16Int = objektBegin.BaiPlus16Int32;
		uint baiPlus20UInt = objektBegin.BaiPlus20UInt32;
		AnnaameDictEntryAnzaal = baiPlus16Int;
		AnnaameRefListeDictEntry = baiPlus20UInt;
		SictPyDictEntry32[] listeDictEntry = null;
		try
		{
			if (AusProzesLeeser == null || baiPlus16Int < 0)
			{
				return;
			}
			if (0 < baiPlus16Int)
			{
			}
			if (ListeEntryAnzaalScrankeMax < baiPlus16Int)
			{
				return;
			}
			int num = 12;
			byte[] array = Extension.ListeOktetLeeseVonAdrese(AusProzesLeeser, (long)baiPlus20UInt, (long)(baiPlus16Int * num), false);
			if (array != null && 0 == 0)
			{
				int num2 = array.Length / num;
				SictPyDictEntry32[] array2 = new SictPyDictEntry32[num2];
				GCHandle gCHandle = GCHandle.Alloc(array2, GCHandleType.Pinned);
				try
				{
					Marshal.Copy(array, 0, gCHandle.AddrOfPinnedObject(), array2.Length * num);
				}
				finally
				{
					gCHandle.Free();
				}
				listeDictEntry = array2;
			}
		}
		finally
		{
			ListeDictEntry = listeDictEntry;
		}
	}
}
