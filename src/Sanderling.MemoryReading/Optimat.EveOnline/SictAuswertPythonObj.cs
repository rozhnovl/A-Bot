using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObj : SictAuswertObjMitAdrese
{
	public int RefCount { get; private set; }

	public long RefType { get; private set; }

	public SictAuswertPythonObjType ObjType { get; set; }

	public override int ListeOktetAnzaalBerecne()
	{
		int num = 8;
		SictAuswertPythonObjType objType = ObjType;
		if (objType != null)
		{
			num = Math.Max(num, objType.tp_basicsize);
		}
		return Math.Max(base.ListeOktetAnzaalBerecne(), num);
	}

	public SictAuswertPythonObj(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, DaatenKwele, ListeOktet)
	{
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		RefCount = SictAuswertObjMitAdrese.Int32AusListeOktet(base.ListeOktet, 0) ?? (-1);
		RefType = SictAuswertObjMitAdrese.UInt32AusListeOktet(base.ListeOktet, 4) ?? 0;
	}

	public virtual void LaadeReferenziirte(IMemoryReader DaatenKwele)
	{
	}
}
