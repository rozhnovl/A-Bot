namespace Optimat.EveOnline;

public class SictAuswertPythonDictEntryAinfac
{
	public const int EntryListeOktetAnzaal = 12;

	public int hash;

	public long ReferenzKey;

	public long ReferenzValue;

	public SictAuswertPythonObj Key;

	public SictAuswertPythonObj Value;

	public SictAuswertPythonDictEntryAinfac(uint[] AusListeEntryListeInt, int EntryIndex)
	{
		if (AusListeEntryListeInt != null)
		{
			hash = (int)AusListeEntryListeInt[EntryIndex * 3];
			ReferenzKey = AusListeEntryListeInt[EntryIndex * 3 + 1];
			ReferenzValue = AusListeEntryListeInt[EntryIndex * 3 + 2];
		}
	}
}
