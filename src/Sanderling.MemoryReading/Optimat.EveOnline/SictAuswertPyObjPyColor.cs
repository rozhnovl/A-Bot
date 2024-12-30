using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObjPyColor : SictAuswertPythonObjMitRefDictBaiPlus8
{
	public SictAuswertPythonObj AusDictA;

	public int? AusDictWertAMilli;

	public SictAuswertPythonObj AusDictR;

	public int? AusDictWertRMilli;

	public SictAuswertPythonObj AusDictG;

	public int? AusDictWertGMilli;

	public SictAuswertPythonObj AusDictB;

	public int? AusDictWertBMilli;

	public SictAuswertPyObjPyColor(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
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
	}
}
