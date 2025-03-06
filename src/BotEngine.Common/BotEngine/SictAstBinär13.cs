using System;
using System.Collections.Generic;
using System.Linq;

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
			enumerable = [sictAstBinär];
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
		return new IEnumerable<SictAstBinär13<BlatTyp>>[3] { enumerable, enumerable2, enumerable3 }.SelectMany(e => e);
	}


	public SictAstBinär13<BlatTyp> BlatAstFürSclüselOderNääcstklainere(long sclüsel)
	{
		return ListeBlatAstÜberscnaidendSclüselRegioon(sclüsel, sclüsel)?.LastOrDefault((SictAstBinär13<BlatTyp> kandidaat) => kandidaat.ListeBlatWert?.Any((KeyValuePair<long, BlatTyp> blatWert) => blatWert.Key <= sclüsel) ?? false);
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
