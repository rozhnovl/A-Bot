using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjInt : SictAuswertPythonObj
{
	public int? Int { get; private set; }

	public SictAuswertPythonObjInt(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 16);
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		Int = Int32AusListeOktet(8);
	}
}
