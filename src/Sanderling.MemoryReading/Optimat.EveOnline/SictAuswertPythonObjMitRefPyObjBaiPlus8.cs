using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjMitRefPyObjBaiPlus8 : SictAuswertPythonObjMitRefBaiPlus8
{
	public SictAuswertPythonObj PyObjRefVonOktet8;

	public SictAuswertPythonObjMitRefPyObjBaiPlus8(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 12);
	}
}
