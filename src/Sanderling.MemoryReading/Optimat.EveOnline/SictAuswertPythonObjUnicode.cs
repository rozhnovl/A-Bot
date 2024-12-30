using System;
using System.Text;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjUnicode : SictAuswertPythonObj
{
	public readonly int LengthScranke = 65536;

	public int length { get; private set; }

	public long Ref_str { get; private set; }

	public int hash { get; private set; }

	public long Ref_defenc { get; private set; }

	public string String { get; private set; }

	public bool? LengthScrankeAingehalte { get; private set; }

	public SictAuswertPythonObjUnicode(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
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
		length = Int32AusListeOktet(8) ?? 0;
		Ref_str = Int32AusListeOktet(12) ?? 0;
		hash = Int32AusListeOktet(16) ?? 0;
		Ref_defenc = UInt32AusListeOktet(20) ?? 0;
	}

	public override void LaadeReferenziirte(IMemoryReader DaatenKwele)
	{
		if (DaatenKwele != null)
		{
			long ref_str = Ref_str;
			int num = length;
			if (ref_str != 0)
			{
				int num2 = Math.Max(0, Math.Min(LengthScranke, num));
				LengthScrankeAingehalte = num2 == num;
				int num3 = num2 * 2;
				byte[] bytes = Extension.ListeOktetLeeseVonAdrese(DaatenKwele, ref_str, (long)num3, false);
				String = Encoding.Unicode.GetString(bytes);
			}
		}
	}
}
