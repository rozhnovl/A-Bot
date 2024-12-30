using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using Bib3.RefNezDiferenz;
using Bib3.RefNezDiferenz.NewtonsoftJson;
using Optimat.EveOnline;
using Optimat.EveOnline.AuswertGbs;

namespace Sanderling.MemoryReading.Production;

public static class Extension
{
	private static readonly SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer KonvertGbsAstInfoRictliinieMitScatescpaicer = new SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer(SictMengeTypeBehandlungRictliinieNewtonsoftJson.KonstruktMengeTypeBehandlungRictliinie(new KeyValuePair<Type, Type>[2]
	{
		new KeyValuePair<Type, Type>(typeof(GbsAstInfo), typeof(UINodeInfoInTree)),
		new KeyValuePair<Type, Type>(typeof(GbsAstInfo[]), typeof(UINodeInfoInTree[]))
	}));

	public static IEnumerable<T[]> SuuceFlacMengeAstMitPfaad<T>(this T SuuceWurzel) where T : GbsAstInfo
	{
		return SuuceWurzel.EnumeratePathToNodeFromTree((T Ast) => Ast.GetListChild()?.OfType<T>());
	}

	public static T[] SuuceFlacMengeAstMitPfaadFrüheste<T>(this T SuuceWurzel, Func<T, bool> Prädikaat) where T : GbsAstInfo
	{
		return SuuceWurzel.SuuceFlacMengeAstMitPfaad()?.Where((T[] pfaad) => Prädikaat((pfaad != null) ? pfaad.LastOrDefault() : null))?.FirstOrDefault();
	}
}
