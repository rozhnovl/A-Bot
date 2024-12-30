namespace Optimat.EveOnline;

public class SictAuswertPyObj32ColorZuusctand : SictAuswertPyObj32MitBaiPlus8RefDictZuusctand
{
	[SictInPyDictEntryKeyAttribut("_a")]
	public SictAuswertPyObj32Zuusctand DictEntryA;

	[SictInPyDictEntryKeyAttribut("_r")]
	public SictAuswertPyObj32Zuusctand DictEntryR;

	[SictInPyDictEntryKeyAttribut("_g")]
	public SictAuswertPyObj32Zuusctand DictEntryG;

	[SictInPyDictEntryKeyAttribut("_b")]
	public SictAuswertPyObj32Zuusctand DictEntryB;

	public SictAuswertPyObj32ColorZuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
		DictListeEntryAnzaalScrankeMax = 256;
	}
}
