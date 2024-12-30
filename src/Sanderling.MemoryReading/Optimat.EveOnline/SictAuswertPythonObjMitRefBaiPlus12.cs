using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjMitRefBaiPlus12 : SictAuswertPythonObj
{
	public long RefBaiOktet12 { get; private set; }

	public SictAuswertPythonObjMitRefBaiPlus12(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
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
		RefBaiOktet12 = SictAuswertObjMitAdrese.UInt32AusListeOktet(base.ListeOktet, 12) ?? 0;
	}
}
