using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjMitRefBaiPlus8 : SictAuswertPythonObj
{
	public long RefBaiOktet8 { get; private set; }

	public SictAuswertPythonObjMitRefBaiPlus8(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 12);
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		RefBaiOktet8 = SictAuswertObjMitAdrese.UInt32AusListeOktet(base.ListeOktet, 8) ?? 0;
	}
}
