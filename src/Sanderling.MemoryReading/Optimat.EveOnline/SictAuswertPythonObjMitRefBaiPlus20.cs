using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjMitRefBaiPlus20 : SictAuswertPythonObj
{
	public long RefBaiOktet20 { get; private set; }

	public SictAuswertPythonObjMitRefBaiPlus20(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
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
		RefBaiOktet20 = SictAuswertObjMitAdrese.UInt32AusListeOktet(base.ListeOktet, 20) ?? 0;
	}
}
