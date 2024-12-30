using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjPyOderUiChildrenList : SictAuswertPythonObjMitRefDictBaiPlus8
{
	public SictAuswertPythonObjPyOderUiChildrenList(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 16);
	}
}
