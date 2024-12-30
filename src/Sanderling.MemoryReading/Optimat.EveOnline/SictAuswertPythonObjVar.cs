using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjVar : SictAuswertPythonObj
{
	public int ob_size { get; private set; }

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 12);
	}

	public SictAuswertPythonObjVar(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		ob_size = SictAuswertObjMitAdrese.Int32AusListeOktet(base.ListeOktet, 8) ?? 0;
	}
}
