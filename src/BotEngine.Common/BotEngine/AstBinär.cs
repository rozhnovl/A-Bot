using System.Collections.Generic;
using System.Linq;

namespace BotEngine;

public class AstBinär<BlatTyp>
{
	public readonly long Grenze;

	public readonly AstBinär<BlatTyp> Ast0;

	public readonly AstBinär<BlatTyp> Ast1;

	public readonly BlatTyp Blat;

	public AstBinär(long grenze, AstBinär<BlatTyp> ast0, AstBinär<BlatTyp> ast1)
	{
		Grenze = grenze;
		Ast0 = ast0;
		Ast1 = ast1;
	}

	public AstBinär(BlatTyp blat)
	{
		Blat = blat;
	}

	public AstBinär<BlatTyp> BlatAstFürWert(long wert)
	{
		AstBinär<BlatTyp> astBinär = this;
		while (astBinär.Ast0 != null)
		{
			astBinär = ((wert >= astBinär.Grenze) ? astBinär.Ast1 : astBinär.Ast0);
		}
		return astBinär;
	}

	public static AstBinär<BlatTyp> ErscteleBaumAusListe(KeyValuePair<long, BlatTyp>[] listeBlat)
	{
		KeyValuePair<long, BlatTyp>[] array = listeBlat.OrderBy((KeyValuePair<long, BlatTyp> blat) => blat.Key).ToArray();
		if (array.Length < 2)
		{
			return new AstBinär<BlatTyp>(array.FirstOrDefault().Value);
		}
		KeyValuePair<long, BlatTyp>[] array2 = array.Take(array.Length / 2).ToArray();
		KeyValuePair<long, BlatTyp>[] array3 = array.Skip(array2.Length).ToArray();
		AstBinär<BlatTyp> ast = ErscteleBaumAusListe(array2);
		AstBinär<BlatTyp> ast2 = ErscteleBaumAusListe(array3);
		return new AstBinär<BlatTyp>(array3.First().Key, ast, ast2);
	}
}
