using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32ListZuusctand : SictAuswertPyObj32VarZuusctand
{
	public int? ListeItemAnzaalScrankeMax;

	public long Ref_ob_item { get; private set; }

	public int allocated { get; private set; }

	public uint[] ListeItemRef { get; private set; }

	public SictAuswertPyObj32ListZuusctand(long herkunftAdrese, long beginZait)
		: base(herkunftAdrese, beginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader ausProzesLeeser, out bool geändert, long zait, int? zuLeeseListeOktetAnzaal = null)
	{
		uint[] listeItemRef = null;
		try
		{
			base.Aktualisiire(ausProzesLeeser, out geändert, zait, zuLeeseListeOktetAnzaal);
			uint baiPlus12UInt = base.ObjektBegin.BaiPlus12UInt32;
			Ref_ob_item = baiPlus12UInt;
			allocated = base.ObjektBegin.BaiPlus16Int32;
			if (ausProzesLeeser == null || baiPlus12UInt == 0 || 0 > base.ob_size)
			{
				return;
			}
			int num = base.ob_size;
			int? listeItemAnzaalScrankeMax = ListeItemAnzaalScrankeMax;
			if (listeItemAnzaalScrankeMax.HasValue)
			{
				num = Math.Min(num, listeItemAnzaalScrankeMax.Value);
			}
			if (4096 >= num)
			{
				int num2 = 4;
				byte[] array = Extension.ListeOktetLeeseVonAdrese(ausProzesLeeser, (long)baiPlus12UInt, (long)(num * num2), false);
				if (array != null)
				{
					int num3 = array.Length / num2;
					uint[] array2 = new uint[num3];
					Buffer.BlockCopy(array, 0, array2, 0, num3 * num2);
					listeItemRef = array2;
				}
			}
		}
		finally
		{
			ListeItemRef = listeItemRef;
		}
	}
}
