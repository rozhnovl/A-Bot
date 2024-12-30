using System;
using System.Text;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32UnicodeZuusctand : SictAuswertPyObj32Zuusctand
{
	public readonly int LengthScranke = 65536;

	public int length { get; private set; }

	public long Ref_str { get; private set; }

	public int hash { get; private set; }

	public long Ref_defenc { get; private set; }

	public string WertString { get; private set; }

	public bool? LengthScrankeAingehalte { get; private set; }

	public SictAuswertPyObj32UnicodeZuusctand(long herkunftAdrese, long beginZait)
		: base(herkunftAdrese, beginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader ausProzesLeeser, out bool geändert, long zait, int? zuLeeseListeOktetAnzaal = null)
	{
		int num = SictAuswertPyObj32Zuusctand.ObjektBeginListeOktetAnzaal;
		if (zuLeeseListeOktetAnzaal.HasValue)
		{
			num = Math.Max(zuLeeseListeOktetAnzaal.Value, num);
		}
		base.Aktualisiire(ausProzesLeeser, out geändert, zait, num);
		SictPyObjAusrictCPython32Erwaitert1 objektBegin = base.ObjektBegin;
		int baiPlus8Int = objektBegin.BaiPlus8Int32;
		uint baiPlus12UInt = objektBegin.BaiPlus12UInt32;
		length = baiPlus8Int;
		Ref_str = baiPlus12UInt;
		hash = objektBegin.BaiPlus16Int32;
		Ref_defenc = objektBegin.BaiPlus20UInt32;
		string wertString = null;
		try
		{
			if (ausProzesLeeser != null)
			{
				int num2 = Math.Max(0, Math.Min(LengthScranke, baiPlus8Int));
				LengthScrankeAingehalte = num2 == baiPlus8Int;
				int num3 = num2 * 2;
				byte[] array = Extension.ListeOktetLeeseVonAdrese(ausProzesLeeser, (long)baiPlus12UInt, (long)num3, false);
				wertString = ((array == null) ? null : Encoding.Unicode.GetString(array));
			}
		}
		finally
		{
			WertString = wertString;
		}
	}
}
