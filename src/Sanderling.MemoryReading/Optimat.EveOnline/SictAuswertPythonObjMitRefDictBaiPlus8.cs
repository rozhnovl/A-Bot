using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjMitRefDictBaiPlus8 : SictAuswertPythonObjMitRefBaiPlus8, ISictAuswertPythonObjMitRefDict
{
	public long RefDict => base.RefBaiOktet8;

	public SictAuswertPythonObjDict Dict { get; set; }

	public SictAuswertPythonObjMitRefDictBaiPlus8(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 12);
	}
}
