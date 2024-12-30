using System.Collections.Generic;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32MitBaiPlus20RefDictZuusctand : SictAuswertPyObj32MitBaiPlus20RefZuusctand
{
	public int DictListeEntryAnzaalScrankeMax;

	public long VonDictEntryMemberAktualisatioonNootwendigLezteZait { get; private set; }

	public long RefDict => base.BaiPlus20Ref;

	public SictAuswertPyObj32DictZuusctand DictObj { get; private set; }

	public SictAuswertPyObj32MitBaiPlus20RefDictZuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
	}

	public void VonDictEntryMemberEntferneWelceRefTypeGeändert(long Zait)
	{
		KeyValuePair<string, SictZuMemberAusDictEntryInfo>[] array = SictProzesAuswertZuusctandScpezGbsBaum.ZuTypeMengeZuDictKeyFieldMemberInfoAusScatenscpaicerOderBerecne(GetType());
		if (array == null)
		{
			return;
		}
		KeyValuePair<string, SictZuMemberAusDictEntryInfo>[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			KeyValuePair<string, SictZuMemberAusDictEntryInfo> keyValuePair = array2[i];
			if (keyValuePair.Value.Getter.Invoke((object)this) is SictAuswertPyObj32Zuusctand { RefTypeGeändert: not false })
			{
				keyValuePair.Value.Setter.Invoke((object)this, (object)null);
				VonDictEntryMemberAktualisatioonNootwendigLezteZait = Zait;
			}
		}
	}

	public override void Aktualisiire(IMemoryReader AusProzesLeeser, out bool Geändert, long Zait, int? ZuLeeseListeOktetAnzaal = null)
	{
		Geändert = false;
		base.Aktualisiire(AusProzesLeeser, out bool Geändert2, Zait, ZuLeeseListeOktetAnzaal);
		Geändert = Geändert2;
		if (AusProzesLeeser != null)
		{
			SictAuswertPyObj32DictZuusctand sictAuswertPyObj32DictZuusctand = DictObj;
			long refDict = RefDict;
			if (sictAuswertPyObj32DictZuusctand != null && sictAuswertPyObj32DictZuusctand.HerkunftAdrese != refDict)
			{
				sictAuswertPyObj32DictZuusctand = null;
			}
			if (sictAuswertPyObj32DictZuusctand == null)
			{
				sictAuswertPyObj32DictZuusctand = new SictAuswertPyObj32DictZuusctand(refDict, Zait);
				sictAuswertPyObj32DictZuusctand.ListeEntryAnzaalScrankeMax = DictListeEntryAnzaalScrankeMax;
			}
			DictObj = sictAuswertPyObj32DictZuusctand;
		}
	}
}
