using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjFloat : SictAuswertPythonObj
{
	public double? Float { get; private set; }

	public SictAuswertPythonObjFloat(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
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
		Float = SictAuswertObjMitAdrese.DoubleAusListeOktet(base.ListeOktet, 8);
	}
}
