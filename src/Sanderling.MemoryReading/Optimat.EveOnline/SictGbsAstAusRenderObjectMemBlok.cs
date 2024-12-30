using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;

namespace Optimat.EveOnline;

public class SictGbsAstAusRenderObjectMemBlok
{
	private static int OktetSictbarkaitAdrese = 72;

	public static int GbsBaumAstListeOktetAnzaal = 396;

	public static int Referenz0KweleAdreseDistanzVonBlokBegin = 192;

	public static int Referenz0ZiilAdreseDistanzVonBlokBegin = 80;

	public static int TestSingleDistanzVonBlokBegin = 28;

	public long Adrese;

	public byte[] ListeOktet;

	public SictGbsAstAusRenderObjectMemBlok[] MengeKandidaatAstEnthalte;

	public string StringTyp;

	public SictGbsAstAusRenderObjectMemBlok[] MengeAstEnthalte => MengeKandidaatAstEnthalte?.Where((SictGbsAstAusRenderObjectMemBlok Kandidaat) => Kandidaat.ListeOktet != null).ToArray();

	public long ReferenzAstEnthaltend => BitConverter.ToUInt32(ListeOktet, 20);

	public long ReferenzStringTyp => BitConverter.ToUInt32(ListeOktet, 44);

	public byte? OktetSictbarkaitWert
	{
		get
		{
			byte[] listeOktet = ListeOktet;
			if (listeOktet.Length <= OktetSictbarkaitAdrese)
			{
				return null;
			}
			return listeOktet[OktetSictbarkaitAdrese];
		}
	}

	public long AstEnthaltendMemBlokBeginAdrese => ReferenzAstEnthaltend - 8;

	public long? PyObject0Adrese
	{
		get
		{
			byte[] listeOktet = ListeOktet;
			if (listeOktet == null)
			{
				return null;
			}
			if (listeOktet.Length < 8)
			{
				return null;
			}
			return BitConverter.ToUInt32(listeOktet, 4);
		}
	}

	public long ArrayListeAstEnthaltenAdrese => BitConverter.ToUInt32(ListeOktet, 112);

	public float[] LaageUndGrööseListeSingle
	{
		get
		{
			byte[] ListeOktet = this.ListeOktet;
			if (ListeOktet == null)
			{
				return null;
			}
			int num = Math.Min(4, (ListeOktet.Length - TestSingleDistanzVonBlokBegin) / 4);
			if (num < 4)
			{
				return null;
			}
			return (from Index in Enumerable.Range(0, num)
				select BitConverter.ToSingle(ListeOktet, Index * 4 + TestSingleDistanzVonBlokBegin)).ToArray();
		}
	}

	public float[] Laage => LaageUndGrööseListeSingle?.Take(2).ToArray();

	public float[] Grööse => LaageUndGrööseListeSingle?.Skip(2).Take(2).ToArray();

	public SictGbsAstAusRenderObjectMemBlok(long Adrese, byte[] ListeOktet = null)
	{
		this.Adrese = Adrese;
		this.ListeOktet = ListeOktet;
	}

	public int ZääleAstAnzaalRekursiiv(int? DistanzScrankeMax = null)
	{
		SictGbsAstAusRenderObjectMemBlok[] mengeAstEnthalte = MengeAstEnthalte;
		if (mengeAstEnthalte == null)
		{
			return 0;
		}
		int[] source = mengeAstEnthalte.Select((SictGbsAstAusRenderObjectMemBlok Ast) => Ast?.ZääleAstAnzaalRekursiiv(DistanzScrankeMax - 1) ?? 0).ToArray();
		return source.Sum() + source.Count();
	}

	public bool BerecneObEnthältAstMitBeginAdrese(long AstBeginAdrese, int? TiifeScrankeMax = null)
	{
		if (Adrese == AstBeginAdrese)
		{
			return true;
		}
		if (TiifeScrankeMax < 1)
		{
			return false;
		}
		return MengeAstEnthalte?.Any((SictGbsAstAusRenderObjectMemBlok Ast) => Ast.BerecneObEnthältAstMitBeginAdrese(AstBeginAdrese, TiifeScrankeMax - 1)) ?? false;
	}

	public SictGbsAstAusRenderObjectMemBlok[] BerecneMengeEnthalteneAst(int? TiifeScrankeMax = null)
	{
		SictGbsAstAusRenderObjectMemBlok[] mengeAstEnthalte = MengeAstEnthalte;
		List<SictGbsAstAusRenderObjectMemBlok[]> list = new List<SictGbsAstAusRenderObjectMemBlok[]>();
		list.Add(new SictGbsAstAusRenderObjectMemBlok[1] { this });
		if (!(TiifeScrankeMax < 1) && mengeAstEnthalte != null)
		{
			SictGbsAstAusRenderObjectMemBlok[] array = mengeAstEnthalte;
			foreach (SictGbsAstAusRenderObjectMemBlok sictGbsAstAusRenderObjectMemBlok in array)
			{
				list.Add(sictGbsAstAusRenderObjectMemBlok.BerecneMengeEnthalteneAst(TiifeScrankeMax - 1));
			}
		}
		return Glob.ArrayAusListeFeldGeflact(list);
	}

	public SictGbsAstAusRenderObjectMemBlok[] BerecnePfaadZuAst(SictGbsAstAusRenderObjectMemBlok Ast)
	{
		if (Ast == null)
		{
			return null;
		}
		if (Ast.Adrese == Adrese)
		{
			return new SictGbsAstAusRenderObjectMemBlok[1] { this };
		}
		SictGbsAstAusRenderObjectMemBlok[] mengeAstEnthalte = MengeAstEnthalte;
		if (mengeAstEnthalte == null)
		{
			return null;
		}
		KeyValuePair<SictGbsAstAusRenderObjectMemBlok, SictGbsAstAusRenderObjectMemBlok[]>[] source = mengeAstEnthalte.Select((SictGbsAstAusRenderObjectMemBlok AstEnthalte) => new KeyValuePair<SictGbsAstAusRenderObjectMemBlok, SictGbsAstAusRenderObjectMemBlok[]>(AstEnthalte, AstEnthalte.BerecnePfaadZuAst(Ast))).ToArray();
		KeyValuePair<SictGbsAstAusRenderObjectMemBlok, SictGbsAstAusRenderObjectMemBlok[]> keyValuePair = source.OrderByDescending((KeyValuePair<SictGbsAstAusRenderObjectMemBlok, SictGbsAstAusRenderObjectMemBlok[]> Kandidaat) => (Kandidaat.Value != null) ? Kandidaat.Value.Length : 0).FirstOrDefault();
		if (keyValuePair.Value != null)
		{
			return new SictGbsAstAusRenderObjectMemBlok[1] { this }.Concat(keyValuePair.Value).ToArray();
		}
		return null;
	}

	public void InMengeAstEnthalteFüügeAinFallsPasend(SictGbsAstAusRenderObjectMemBlok[] ListeKandidaatAstEnthalte)
	{
		if (ListeKandidaatAstEnthalte == null)
		{
			return;
		}
		List<SictGbsAstAusRenderObjectMemBlok> list = new List<SictGbsAstAusRenderObjectMemBlok>();
		foreach (SictGbsAstAusRenderObjectMemBlok sictGbsAstAusRenderObjectMemBlok in ListeKandidaatAstEnthalte)
		{
			if (sictGbsAstAusRenderObjectMemBlok != this && sictGbsAstAusRenderObjectMemBlok.AstEnthaltendMemBlokBeginAdrese == Adrese)
			{
				list.Add(sictGbsAstAusRenderObjectMemBlok);
			}
		}
		SictGbsAstAusRenderObjectMemBlok[] mengeKandidaatAstEnthalte = MengeKandidaatAstEnthalte;
		MengeKandidaatAstEnthalte = (mengeKandidaatAstEnthalte ?? new SictGbsAstAusRenderObjectMemBlok[0]).Concat(list).ToArray();
	}
}
