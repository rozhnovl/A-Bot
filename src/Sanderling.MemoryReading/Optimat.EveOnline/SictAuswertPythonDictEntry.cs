using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonDictEntry : SictAuswertObjMitAdrese
{
	public const int EntryListeOktetAnzaal = 12;

	public SictAuswertPythonObj Key;

	public SictAuswertPythonObj Value;

	public int hash { get; private set; }

	public long ReferenzKey { get; private set; }

	public long ReferenzValue { get; private set; }

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 12);
	}

	public SictAuswertPythonDictEntry(long HerkunftAdrese, IMemoryReader DaatenKwele = null, byte[] ListeOktet = null)
		: base(HerkunftAdrese, DaatenKwele, ListeOktet)
	{
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		hash = SictAuswertObjMitAdrese.Int32AusListeOktet(base.ListeOktet, 0) ?? (-1);
		ReferenzKey = SictAuswertObjMitAdrese.UInt32AusListeOktet(base.ListeOktet, 4) ?? 0;
		ReferenzValue = SictAuswertObjMitAdrese.UInt32AusListeOktet(base.ListeOktet, 8) ?? 0;
	}
}
