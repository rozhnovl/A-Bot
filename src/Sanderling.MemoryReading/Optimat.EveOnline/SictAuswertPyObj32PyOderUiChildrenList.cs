namespace Optimat.EveOnline;

public class SictAuswertPyObj32PyOderUiChildrenList : SictAuswertPyObj32MitBaiPlus8RefDictZuusctand
{
	[SictInPyDictEntryKeyAttribut("_childrenObjects")]
	public SictAuswertPyObj32Zuusctand DictEntryListChildrenObj;

	public SictAuswertPyObj32PyOderUiChildrenList(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
		DictListeEntryAnzaalScrankeMax = 256;
	}
}
