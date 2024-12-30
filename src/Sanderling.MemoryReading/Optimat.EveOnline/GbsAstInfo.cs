using System.Collections.Generic;
using BotEngine.Interface;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline;

public class GbsAstInfo
{
	public long? PyObjAddress;

	public string PyObjTypName;

	public bool? VisibleIncludingInheritance;

	public string Name;

	public string Text;

	public string SetText;

	public string LinkText;

	public string Hint;

	public float? LaageInParentA;

	public float? LaageInParentB;

	public float? GrööseA;

	public float? GrööseB;

	public string Caption;

	public bool? Minimized;

	public bool? isModal;

	public bool? isSelected;

	public string WindowID;

	public double? LastStateFloat;

	public double? LastSetCapacitorFloat;

	public double? LastValueFloat;

	public double? RotationFloat;

	public string SrHtmlstr;

	public string EditTextlineCoreText;

	public int? ColorAMili;

	public int? ColorRMili;

	public int? ColorGMili;

	public int? ColorBMili;

	public long? TextureIdent0;

	public string texturePath;

	public float? Speed;

	public float? CapacitorLevel;

	public float? ShieldLevel;

	public float? ArmorLevel;

	public float? StructureLevel;

	public GbsAstInfo[] ListChild;

	public GbsAstInfo[] BackgroundList;

	public string[] DictListKeyStringValueNotEmpty;

	public int? SquadronSize;

	public int? SquadronMaxSize;

	public bool? RampActive;

	public ColorORGBVal? Color
	{
		get
		{
			return KomponenteZuColorARGBVal(ColorAMili, ColorRMili, ColorGMili, ColorBMili);
		}
		set
		{
			ZuuwaisungNaacKomponente(Color, ref ColorAMili, ref ColorRMili, ref ColorGMili, ref ColorBMili);
		}
	}

	public Vektor2DSingle? LaageInParent
	{
		get
		{
			return KomponenteZuVektorSingle(LaageInParentA, LaageInParentB);
		}
		set
		{
			ZuuwaisungNaacKomponente(value, ref LaageInParentA, ref LaageInParentB);
		}
	}

	public Vektor2DSingle? Grööse
	{
		get
		{
			return KomponenteZuVektorSingle(GrööseA, GrööseB);
		}
		set
		{
			ZuuwaisungNaacKomponente(value, ref GrööseA, ref GrööseB);
		}
	}

	public GbsAstInfo()
	{
	}

	public GbsAstInfo(long? inProzesHerkunftAdrese)
	{
		PyObjAddress = inProzesHerkunftAdrese;
	}

	public virtual IEnumerable<GbsAstInfo> GetListChild()
	{
		return ListChild;
	}

	public static void ZuuwaisungNaacKomponente(Vektor2DSingle? vektor, ref float? komponenteA, ref float? komponenteB)
	{
		komponenteA = vektor?.A;
		komponenteB = vektor?.B;
	}

	public static Vektor2DSingle? KomponenteZuVektorSingle(float? a, float? b)
	{
		if (!a.HasValue || !b.HasValue)
		{
			return null;
		}
		return new Vektor2DSingle(a.Value, b.Value);
	}

	public static void ZuuwaisungNaacKomponente(ColorORGBVal? farbe, ref int? komponenteAMili, ref int? komponenteRMili, ref int? komponenteGMili, ref int? komponenteBMili)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		komponenteAMili = farbe?.OMilli;
		komponenteRMili = farbe?.RMilli;
		komponenteGMili = farbe?.GMilli;
		komponenteBMili = farbe?.BMilli;
	}

	public static ColorORGBVal? KomponenteZuColorARGBVal(int? aMilli, int? rMilli, int? gMilli, int? bMilli)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return new ColorORGBVal(aMilli, rMilli, gMilli, bMilli);
	}

	public IEnumerable<GbsAstInfo> MengeChildAstTransitiiveHüle(int? tiifeScrankeMax = null)
	{
		IEnumerable<GbsAstInfo> listChild = GetListChild();
		if (tiifeScrankeMax <= 0)
		{
			return null;
		}
		if (listChild == null)
		{
			return null;
		}
		List<GbsAstInfo> list = new List<GbsAstInfo>();
		foreach (GbsAstInfo item in listChild)
		{
			if (item != null)
			{
				list.Add(item);
				IEnumerable<GbsAstInfo> enumerable = item.MengeChildAstTransitiiveHüle(tiifeScrankeMax - 1);
				if (enumerable != null)
				{
					list.AddRange(enumerable);
				}
			}
		}
		return list;
	}

	public long[] MengeSelbsctUndChildAstHerkunftAdreseTransitiiveHüleBerecne(int? tiifeScrankeMax = null)
	{
		List<long> list = new List<long>();
		MengeSelbsctUndChildAstHerkunftAdreseTransitiiveHüleFüügeAinNaacListe(list, tiifeScrankeMax);
		return list.ToArray();
	}

	public void MengeSelbsctUndChildAstHerkunftAdreseTransitiiveHüleFüügeAinNaacListe(IList<long> ziilListe, int? tiifeScrankeMax = null)
	{
		if (ziilListe == null)
		{
			return;
		}
		long? pyObjAddress = PyObjAddress;
		if (pyObjAddress.HasValue)
		{
			ziilListe.Add(pyObjAddress.Value);
		}
		IEnumerable<GbsAstInfo> listChild = GetListChild();
		if (tiifeScrankeMax <= 0 || listChild == null)
		{
			return;
		}
		List<long> list = new List<long>();
		foreach (GbsAstInfo item in listChild)
		{
			item?.MengeSelbsctUndChildAstHerkunftAdreseTransitiiveHüleFüügeAinNaacListe(ziilListe, tiifeScrankeMax - 1);
		}
	}
}
