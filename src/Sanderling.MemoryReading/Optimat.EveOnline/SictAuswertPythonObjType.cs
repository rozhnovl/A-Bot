using System;
using System.Linq;
using System.Text;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjType : SictAuswertPythonObjVar
{
	public long ReferenzTp_name;

	public int tp_basicsize;

	public int tp_itemsize;

	public string tp_name;

	public SictAuswertPythonObjType(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 1024);
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		ReferenzTp_name = SictAuswertObjMitAdrese.UInt32AusListeOktet(base.ListeOktet, 12) ?? 0;
		tp_basicsize = SictAuswertObjMitAdrese.Int32AusListeOktet(base.ListeOktet, 16) ?? 0;
		tp_itemsize = SictAuswertObjMitAdrese.Int32AusListeOktet(base.ListeOktet, 20) ?? 0;
	}

	public override void LaadeReferenziirte(IMemoryReader DaatenKwele)
	{
		base.LaadeReferenziirte(DaatenKwele);
		if (DaatenKwele == null)
		{
			return;
		}
		long referenzTp_name = ReferenzTp_name;
		if (referenzTp_name == 0)
		{
			return;
		}
		byte[] array = Extension.ListeOktetLeeseVonAdrese(DaatenKwele, referenzTp_name, 256L, false);
		if (array != null)
		{
			string @string = Encoding.ASCII.GetString(array.TakeWhile((byte t) => t != 0).ToArray());
			tp_name = @string;
		}
	}
}
