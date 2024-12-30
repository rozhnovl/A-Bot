using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;

namespace BotEngine;

public class SictAstBinär13<BlatTyp>
{
	public readonly long Grenze;

	public readonly SictAstBinär13<BlatTyp> AstKlainer;

	public readonly SictAstBinär13<BlatTyp> AstGrööser;

	public readonly KeyValuePair<long, BlatTyp>[] ListeBlatWert;

	public long EnthalteneBlatAnzaal { get; private set; }

	public SictAstBinär13(long grenze, SictAstBinär13<BlatTyp> astKlainer, SictAstBinär13<BlatTyp> astGrööser)
	{
		Grenze = grenze;
		AstKlainer = astKlainer;
		AstGrööser = astGrööser;
		EnthalteneBlatAnzaal = (astKlainer?.EnthalteneBlatAnzaal ?? 0) + (astGrööser?.EnthalteneBlatAnzaal ?? 0);
	}

	public SictAstBinär13(KeyValuePair<long, BlatTyp>[] listeBlatWert)
	{
		ListeBlatWert = listeBlatWert;
		EnthalteneBlatAnzaal = ((listeBlatWert != null) ? listeBlatWert.LongLength : 0);
	}

	public BlatTyp[] ListeBlatWertFürSclüsel(long sclüsel)
	{
		KeyValuePair<long, BlatTyp>[] source = ListeBlatSclüselMitWertÜberscnaidendSclüselRegioon(sclüsel, sclüsel);
		return source.Select((KeyValuePair<long, BlatTyp> sclüselMitWert) => sclüselMitWert.Value).ToArray();
	}

	public KeyValuePair<long, BlatTyp>[] ListeBlatSclüselMitWertÜberscnaidendSclüselRegioon(long? sclüselScrankeMin, long? sclüselScrankeMax)
	{
		IEnumerable<SictAstBinär13<BlatTyp>> enumerable = ListeBlatAstÜberscnaidendSclüselRegioon(sclüselScrankeMin, sclüselScrankeMax);
		if (enumerable == null)
		{
			return null;
		}
		List<KeyValuePair<long, BlatTyp>> list = new List<KeyValuePair<long, BlatTyp>>();
		if (enumerable != null)
		{
			foreach (SictAstBinär13<BlatTyp> item2 in enumerable)
			{
				KeyValuePair<long, BlatTyp>[] listeBlatWert = item2.ListeBlatWert;
				KeyValuePair<long, BlatTyp>[] array = listeBlatWert;
				for (int i = 0; i < array.Length; i++)
				{
					KeyValuePair<long, BlatTyp> item = array[i];
					if (!(item.Key < sclüselScrankeMin) && !(sclüselScrankeMax < item.Key))
					{
						list.Add(item);
					}
				}
			}
		}
		return list.ToArray();
	}

	public IEnumerable<SictAstBinär13<BlatTyp>> ListeBlatAstÜberscnaidendSclüselRegioon(long? sclüselScrankeMin, long? sclüselScrankeMax)
	{
		SictAstBinär13<BlatTyp> sictAstBinär = null;
		if (ListeBlatWert != null && ListeBlatWert.Length != 0)
		{
			long key = ListeBlatWert[0].Key;
			long key2 = ListeBlatWert[ListeBlatWert.Length - 1].Key;
			long? num = sclüselScrankeMax;
			long num2 = key;
			if (num.GetValueOrDefault() >= num2 || !num.HasValue || !(key2 < sclüselScrankeMin))
			{
				sictAstBinär = this;
			}
		}
		IEnumerable<SictAstBinär13<BlatTyp>> enumerable = null;
		if (sictAstBinär != null)
		{
			enumerable = new SictAstBinär13<BlatTyp>[1] { sictAstBinär };
		}
		IEnumerable<SictAstBinär13<BlatTyp>> enumerable2 = null;
		IEnumerable<SictAstBinär13<BlatTyp>> enumerable3 = null;
		if (AstKlainer != null && !(Grenze < sclüselScrankeMin))
		{
			enumerable2 = AstKlainer.ListeBlatAstÜberscnaidendSclüselRegioon(sclüselScrankeMin, sclüselScrankeMax);
		}
		if (AstGrööser != null && !(sclüselScrankeMax < Grenze))
		{
			enumerable3 = AstGrööser.ListeBlatAstÜberscnaidendSclüselRegioon(sclüselScrankeMin, sclüselScrankeMax);
		}
		return new IEnumerable<SictAstBinär13<BlatTyp>>[3] { enumerable, enumerable2, enumerable3 }.ListeEnumerableAgregiirt();
	}

	public BlatTyp BlatWertFürSclüselOderNääxtklainere(long sclüsel)
	{
		KeyValuePair<long, BlatTyp>? keyValuePair = BlatSclüselUndWertFürSclüselOderNääxtklainere(sclüsel);
		if (!keyValuePair.HasValue)
		{
			return default(BlatTyp);
		}
		return keyValuePair.Value.Value;
	}

	public KeyValuePair<long, BlatTyp>? BlatSclüselUndWertFürSclüselOderNääxtklainere(long sclüsel)
	{
		KeyValuePair<long, BlatTyp>[] array = BlatAstFürSclüselOderNääcstklainere(sclüsel)?.ListeBlatWert;
		if (array == null)
		{
			return null;
		}
		for (int i = 0; i < array.Length - 1; i++)
		{
			KeyValuePair<long, BlatTyp> value = array[i];
			KeyValuePair<long, BlatTyp> keyValuePair = array[i + 1];
			if (sclüsel <= value.Key)
			{
				return value;
			}
			if (sclüsel < keyValuePair.Key)
			{
				return value;
			}
		}
		return array.LastOrDefault();
	}

	public SictAstBinär13<BlatTyp> BlatAstFürSclüselOderNääcstklainere(long sclüsel)
	{
		return ListeBlatAstÜberscnaidendSclüselRegioon(sclüsel, sclüsel)?.LastOrDefault((SictAstBinär13<BlatTyp> kandidaat) => kandidaat.ListeBlatWert?.Any((KeyValuePair<long, BlatTyp> blatWert) => blatWert.Key <= sclüsel) ?? false);
	}

	public static SictAstBinär13<BlatTyp> ErscteleBaumAusListe(IEnumerable<BlatTyp> listeBlatWert, Func<BlatTyp, long> funktioonSclüselAusBlatWert, int? baumTiifeScrankeMax = null)
	{
		List<KeyValuePair<long, BlatTyp>> list = new List<KeyValuePair<long, BlatTyp>>();
		foreach (BlatTyp item in listeBlatWert.EmptyIfNull())
		{
			long key = funktioonSclüselAusBlatWert?.Invoke(item) ?? (-1);
			list.Add(new KeyValuePair<long, BlatTyp>(key, item));
		}
		return ErscteleBaumAusListe(list.ToArray(), baumTiifeScrankeMax);
	}

	public static SictAstBinär13<BlatTyp> ErscteleBaumAusListe(IEnumerable<KeyValuePair<long, BlatTyp>> listeBlat, int? baumTiifeScrankeMax = null)
	{
		if (listeBlat == null)
		{
			return null;
		}
		KeyValuePair<long, BlatTyp>[] listeBlatOrdnet = listeBlat.OrderBy((KeyValuePair<long, BlatTyp> blat) => blat.Key).ToArray();
		return ErscteleBaumAusListeFürAnnaameListeOrdnet(listeBlatOrdnet, baumTiifeScrankeMax);
	}

	public static SictAstBinär13<BlatTyp> ErscteleBaumAusListeFürAnnaameListeOrdnet(KeyValuePair<long, BlatTyp>[] listeBlatOrdnet, int? baumTiifeScrankeMax = null)
	{
		ArraySegment<KeyValuePair<long, BlatTyp>> listeBlatOrdnet2 = new ArraySegment<KeyValuePair<long, BlatTyp>>(listeBlatOrdnet);
		return ErscteleBaumAusListeFürAnnaameListeOrdnet(listeBlatOrdnet2, baumTiifeScrankeMax);
	}

	public static SictAstBinär13<BlatTyp> ErscteleBaumAusListeFürAnnaameListeOrdnet(ArraySegment<KeyValuePair<long, BlatTyp>> listeBlatOrdnet, int? baumTiifeScrankeMax = null)
	{
		if (listeBlatOrdnet.Count < 2 || baumTiifeScrankeMax < 1)
		{
			return new SictAstBinär13<BlatTyp>(listeBlatOrdnet.ToArray());
		}
		int num = listeBlatOrdnet.Count / 2;
		ArraySegment<KeyValuePair<long, BlatTyp>> listeBlatOrdnet2 = new ArraySegment<KeyValuePair<long, BlatTyp>>(listeBlatOrdnet.ToArray(), 0, num);
		ArraySegment<KeyValuePair<long, BlatTyp>> arraySegment = new ArraySegment<KeyValuePair<long, BlatTyp>>(listeBlatOrdnet.ToArray(), num, listeBlatOrdnet.Count - num);
		SictAstBinär13<BlatTyp> astKlainer = ErscteleBaumAusListeFürAnnaameListeOrdnet(listeBlatOrdnet2, baumTiifeScrankeMax - 1);
		SictAstBinär13<BlatTyp> astGrööser = ErscteleBaumAusListeFürAnnaameListeOrdnet(arraySegment, baumTiifeScrankeMax - 1);
		return new SictAstBinär13<BlatTyp>(arraySegment.First().Key, astKlainer, astGrööser);
	}
}
