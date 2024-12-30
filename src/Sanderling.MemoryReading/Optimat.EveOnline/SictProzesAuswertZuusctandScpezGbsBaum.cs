using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bib3;
using BotEngine.Common;
using BotEngine.Interface;
using Fasterflect;
using Sanderling.MemoryReading.Production;
using Extension = BotEngine.Interface.Extension;

namespace Optimat.EveOnline;

public class SictProzesAuswertZuusctandScpezGbsBaum
{
	private class SictInBerecnungAst
	{
		public SictInBerecnungAst Parent;

		public SictAuswertPyObj32GbsAstZuusctand PyObj;

		public SictGbsAstAusRenderObjectMemBlok GbsAstAusRenderObjectMemoryBlok;

		public int? TiifeScrankeMax;

		public int? GbsAstAnzaalScrankeMax;

		public GbsAstInfo Info;

		public SictInBerecnungAst(SictInBerecnungAst Parent = null, SictAuswertPyObj32GbsAstZuusctand PyObj = null, int? TiifeScrankeMax = null, int? GbsAstAnzaalScrankeMax = null, SictGbsAstAusRenderObjectMemBlok GbsFensterBlok = null, GbsAstInfo Info = null)
		{
			this.Parent = Parent;
			this.PyObj = PyObj;
			this.TiifeScrankeMax = TiifeScrankeMax;
			this.GbsAstAnzaalScrankeMax = GbsAstAnzaalScrankeMax;
			GbsAstAusRenderObjectMemoryBlok = GbsFensterBlok;
			this.Info = Info;
		}
	}

	private readonly object Lock = new object();

	public long Zait;

	public readonly long[] SuuceMengeWurzelAdrese;

	public readonly long[] MengeAstSuuceFortsazFraigaabe;

	public long AstAlterScrankeMax;

	public int? AstListeChildAnzaalScrankeMax;

	public int? MengeAstAnzaalScrankeMax;

	public int? SuuceTiifeScrankeMax;

	public readonly SictProzesAuswertZuusctand ProzesAuswertZuusctandBaasis;

	public readonly IMemoryReader MemoryReader;

	private readonly Dictionary<long, SictAuswertPyObj32Zuusctand> DictZuHerkunftAdreseObjekt = new Dictionary<long, SictAuswertPyObj32Zuusctand>();

	private readonly Dictionary<SictAuswertPyObj32Zuusctand, WertZuZaitpunktStruct<SictPyDictEntry32[]>> DictZuPyObjPropagatioonDictEntryNaacMemberLezteListeDictEntry = new Dictionary<SictAuswertPyObj32Zuusctand, WertZuZaitpunktStruct<SictPyDictEntry32[]>>();

	private SictAuswertPyObj32GbsAstZuusctand[] GbsMengeWurzel;

	private static SictScatenscpaicerDict<Type, ConstructorInfo> ScatescpaicerZuTypeKonstruktorHerkunftAdreseUndBeginZait = new SictScatenscpaicerDict<Type, ConstructorInfo>();

	public long DebugBrecpunktGbsAstHerkunftAdrese = -1L;

	private static SictScatenscpaicerDict<Type, KeyValuePair<string, SictZuMemberAusDictEntryInfo>[]> ScatescpaicerZuTypeMengeZuDictKeyFieldMemberInfo = new SictScatenscpaicerDict<Type, KeyValuePair<string, SictZuMemberAusDictEntryInfo>[]>();

	public long ScritLezteBeginZaitMili { get; private set; }

	public long ScritLezteEndeZaitMili { get; private set; }

	public long ScritLezteDauerMili => ScritLezteEndeZaitMili - ScritLezteBeginZaitMili;

	public GbsAstInfo[] MengeGbsWurzelInfo { get; private set; }

	public GbsAstInfo GbsWurzelHauptInfo => MengeGbsWurzelInfo?.OrderByDescending((GbsAstInfo Wurzel) => Wurzel.MengeChildAstTransitiiveHüle()?.Count() ?? 0)?.FirstOrDefault();

	public GbsAstInfo[] ScritLezteMengeAstGeändert { get; private set; }

	private void _16_Erwaiterung_Manuel(long zait, IMemoryReader ausProzesLeeser, SictProzesAuswertZuusctand prozesAuswertZuusctand, SictAuswertPyObj32GbsAstZuusctand gbsAst, GbsAstInfo ziilAstInfo)
	{
		if (ziilAstInfo == null)
		{
			return;
		}
		SictPyDictEntry32[] listeDictEntry = gbsAst.DictObj?.ListeDictEntry;
		Func<string, SictAuswertPyObj32Zuusctand> func = delegate(string keyString)
		{
			foreach (SictPyDictEntry32 item in listeDictEntry.EmptyIfNull())
			{
				string text = (ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(item.ReferenzKey, ausProzesLeeser, prozesAuswertZuusctand, Zait) as SictAuswertPyObj32StrZuusctand)?.WertString;
				if (keyString == text)
				{
					SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(item.ReferenzValue, ausProzesLeeser, prozesAuswertZuusctand, Zait);
					if (sictAuswertPyObj32Zuusctand != null)
					{
						sictAuswertPyObj32Zuusctand?.Aktualisiire(ausProzesLeeser, out var _, zait);
						return sictAuswertPyObj32Zuusctand;
					}
				}
			}
			return (SictAuswertPyObj32Zuusctand)null;
		};
		Lazy<string[]> lazy = new Lazy<string[]>(() => listeDictEntry?.Select(delegate(SictPyDictEntry32 dictEntry)
		{
			string text2 = (ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(dictEntry.ReferenzKey, ausProzesLeeser, prozesAuswertZuusctand, Zait) as SictAuswertPyObj32StrZuusctand)?.WertString;
			if (text2 == null)
			{
				return (string)null;
			}
			SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand2 = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(dictEntry.ReferenzValue, ausProzesLeeser, prozesAuswertZuusctand, Zait);
			if (sictAuswertPyObj32Zuusctand2 == null)
			{
				return (string)null;
			}
			sictAuswertPyObj32Zuusctand2.Aktualisiire(ausProzesLeeser, out var _, Zait);
			SictAuswertPythonObj sictAuswertPythonObj = prozesAuswertZuusctand.MengeFürHerkunftAdrPyObj?.TryGetValueOrDefault(sictAuswertPyObj32Zuusctand2.RefType);
			string text3 = (sictAuswertPythonObj as SictAuswertPythonObjType)?.tp_name;
			if (!(0 < text3?.Length))
			{
				return (string)null;
			}
			return ("NoneType" == text3) ? null : text2;
		})?.WhereNotDefault()?.ToArrayIfNotEmpty());
		if ("SpaceObjectIcon" == ziilAstInfo.PyObjTypName)
		{
			ziilAstInfo.DictListKeyStringValueNotEmpty = lazy.Value;
		}
		if ("FightersHealthGauge" == ziilAstInfo.PyObjTypName)
		{
			SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand3 = func("squadronSize");
			SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand4 = func("squadronMaxSize");
			ziilAstInfo.SquadronSize = (sictAuswertPyObj32Zuusctand3 as SictAuswertPyObj32Int32Zuusctand)?.WertInt32;
			ziilAstInfo.SquadronMaxSize = (sictAuswertPyObj32Zuusctand4 as SictAuswertPyObj32Int32Zuusctand)?.WertInt32;
		}
		if ("ModuleButton" == ziilAstInfo.PyObjTypName || "AbilityIcon" == ziilAstInfo.PyObjTypName)
		{
			SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand5 = func("ramp_active");
			ziilAstInfo.RampActive = (sictAuswertPyObj32Zuusctand5 as SictAuswertPyObj32BoolZuusctand)?.WertBool;
		}
	}

	public SictProzesAuswertZuusctandScpezGbsBaum(IMemoryReader MemoryReader, SictProzesAuswertZuusctand ProzesAuswertZuusctandBaasis, int? AstListeChildAnzaalScrankeMax = null, int? MengeAstAnzaalScrankeMax = null, int? SuuceTiifeScrankeMax = null, long[] SuuceMengeWurzelAdrese = null, IEnumerable<long> MengeAstSuuceFortsazFraigaabe = null)
	{
		this.MemoryReader = MemoryReader;
		this.ProzesAuswertZuusctandBaasis = ProzesAuswertZuusctandBaasis;
		this.AstListeChildAnzaalScrankeMax = AstListeChildAnzaalScrankeMax;
		this.MengeAstAnzaalScrankeMax = MengeAstAnzaalScrankeMax;
		this.SuuceTiifeScrankeMax = SuuceTiifeScrankeMax;
		this.SuuceMengeWurzelAdrese = SuuceMengeWurzelAdrese;
		this.MengeAstSuuceFortsazFraigaabe = MengeAstSuuceFortsazFraigaabe?.ToArray();
	}

	public void ZuusctandLeere()
	{
		MengeGbsWurzelInfo = null;
		DictZuHerkunftAdreseObjekt.Clear();
		DictZuPyObjPropagatioonDictEntryNaacMemberLezteListeDictEntry.Clear();
	}

	private static ConstructorInfo ZuTypeKonstruktorHerkunftAdreseUndBeginZaitBerecne(Type ziilType)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (null == ziilType)
		{
			return null;
		}
		IList<ConstructorInfo> list = AttributeExtensions.ConstructorsWith(ziilType, Flags.InstancePublicDeclaredOnly, (Type[])null);
		Type[] second = new Type[2]
		{
			typeof(long),
			typeof(long)
		};
		foreach (ConstructorInfo item in list)
		{
			ParameterInfo[] parameters = item.GetParameters();
			if (parameters == null || !parameters.Select((ParameterInfo parameter) => parameter.ParameterType).SequenceEqual(second))
			{
				continue;
			}
			return item;
		}
		return null;
	}

	public static SictProzesAuswertZuusctand FürAuswertScritKopiiErsctele(SictProzesAuswertZuusctand ProzesAuswertZuusctandZuKopiire)
	{
		if (ProzesAuswertZuusctandZuKopiire == null)
		{
			return null;
		}
		SictProzesAuswertZuusctand sictProzesAuswertZuusctand = new SictProzesAuswertZuusctand();
		sictProzesAuswertZuusctand.KopiireVon(ProzesAuswertZuusctandZuKopiire);
		sictProzesAuswertZuusctand.AusMengePyObjEntferne((SictAuswertPythonObj PyObj) => !(PyObj is SictAuswertPythonObjType) && !(PyObj is SictAuswertPythonObjStr));
		sictProzesAuswertZuusctand.MengePyObjSezeNaacScpaicer(sictProzesAuswertZuusctand.GbsMengeWurzelObj);
		return sictProzesAuswertZuusctand;
	}

	public static SictProzesAuswertZuusctandScpezGbsBaum BerecneScrit(SictProzesAuswertZuusctandScpezGbsBaum zuusctand)
	{
		zuusctand?.BerecneScrit();
		return zuusctand;
	}

	private static long? WertMiliAusPyObj(SictAuswertPyObj32Zuusctand pyObj)
	{
		if (pyObj is SictAuswertPyObj32Int32Zuusctand sictAuswertPyObj32Int32Zuusctand)
		{
			return sictAuswertPyObj32Int32Zuusctand.WertInt32 * 1000;
		}
		if (pyObj is SictAuswertPyObj32Float64Zuusctand { WertFloat64: var wertFloat } sictAuswertPyObj32Float64Zuusctand)
		{
			return (long)(sictAuswertPyObj32Float64Zuusctand.WertFloat64 * 1000.0);
		}
		return null;
	}

	public void BerecneScrit()
	{
		try
		{
			IMemoryReader memoryReader = MemoryReader;
			if (memoryReader == null)
			{
				return;
			}
			SictMesungZaitraumAusStopwatch sictMesungZaitraumAusStopwatch = new SictMesungZaitraumAusStopwatch(beginJezt: true);
			long num = sictMesungZaitraumAusStopwatch.BeginZaitMikro ?? 0;
			long BeginZaitMili = num / 1000;
			long num2 = BeginZaitMili;
			try
			{
				long num3 = 0L;
				lock (Lock)
				{
					int? astListeChildAnzaalScrankeMax = AstListeChildAnzaalScrankeMax;
					int? mengeAstAnzaalScrankeMax = MengeAstAnzaalScrankeMax;
					int? suuceTiifeScrankeMax = SuuceTiifeScrankeMax;
					long[] array = SuuceMengeWurzelAdrese;
					long[] mengeAstSuuceFortsazFraigaabe = MengeAstSuuceFortsazFraigaabe;
					SictProzesAuswertZuusctand sictProzesAuswertZuusctand = FürAuswertScritKopiiErsctele(ProzesAuswertZuusctandBaasis);
					if (sictProzesAuswertZuusctand == null)
					{
						return;
					}
					SictAuswertPyObj32GbsAstZuusctand[] array2 = GbsMengeWurzel;
					if (array == null)
					{
						//todo here it fails
						SictAuswertPyObjGbsAst[] gbsMengeWurzelObj = sictProzesAuswertZuusctand.GbsMengeWurzelObj;
						if (gbsMengeWurzelObj != null)
						{
							array = gbsMengeWurzelObj.Select((SictAuswertPyObjGbsAst KweleWurzelObj) => KweleWurzelObj.HerkunftAdrese).ToArray();
						}
					}
					if (array != null)
					{
						array2 = array.Select((long WurzelAdrese) => new SictAuswertPyObj32GbsAstZuusctand(WurzelAdrese, BeginZaitMili)).ToArray();
					}
					if (array2 == null)
					{
						return;
					}
					List<GbsAstInfo> list = new List<GbsAstInfo>();
					SictAuswertPyObjGbsAst[] gbsMengeWurzelObj2 = sictProzesAuswertZuusctand.GbsMengeWurzelObj;
					List<GbsAstInfo> list2 = new List<GbsAstInfo>();
					try
					{
						SictAuswertPyObj32GbsAstZuusctand[] array3 = array2;
						foreach (SictAuswertPyObj32GbsAstZuusctand sictAuswertPyObj32GbsAstZuusctand in array3)
						{
							if (mengeAstAnzaalScrankeMax < num3)
							{
								break;
							}
							Dictionary<long, bool> dictionary = new Dictionary<long, bool>();
							List<SictInBerecnungAst> list3 = new List<SictInBerecnungAst>();
							SictInBerecnungAst sictInBerecnungAst = new SictInBerecnungAst(null, sictAuswertPyObj32GbsAstZuusctand, suuceTiifeScrankeMax);
							list3.Add(sictInBerecnungAst);
							while (0 < list3.Count)
							{
								SictInBerecnungAst sictInBerecnungAst2 = list3[0];
								list3.RemoveAt(0);
								num3++;
								if (mengeAstAnzaalScrankeMax < num3)
								{
									break;
								}
								SictAuswertPyObj32GbsAstZuusctand pyObj = sictInBerecnungAst2.PyObj;
								if (pyObj == null)
								{
									continue;
								}
								if (pyObj.HerkunftAdrese == DebugBrecpunktGbsAstHerkunftAdrese)
								{
								}
								dictionary[pyObj.HerkunftAdrese] = true;
								if (mengeAstSuuceFortsazFraigaabe != null && pyObj.HerkunftAdrese != sictAuswertPyObj32GbsAstZuusctand.HerkunftAdrese && !mengeAstSuuceFortsazFraigaabe.Contains(pyObj.HerkunftAdrese))
								{
									continue;
								}
								sictInBerecnungAst2.Info = pyObj.AstInfo;
								pyObj.Aktualisiire(memoryReader, out var _, BeginZaitMili);
								if (pyObj.TypeObjektKlas == null)
								{
									pyObj.TypeObjektKlas = sictProzesAuswertZuusctand.PyObjFürHerkunftAdreseAusScpaicer(pyObj.RefType) as SictAuswertPythonObjType;
								}
								SictAuswertPyObj32DictZuusctand dictObj = pyObj.DictObj;
								if (dictObj == null)
								{
									continue;
								}
								dictObj.Aktualisiire(memoryReader, out var Geändert2, BeginZaitMili);
								AstInfoScteleSicerAktuelOoneListeChild(pyObj, memoryReader, sictProzesAuswertZuusctand, BeginZaitMili, null);
								GbsAstInfo astInfo = pyObj.AstInfo;
								SictAuswertPyObj32Zuusctand dictEntryRenderObject = pyObj.DictEntryRenderObject;
								if (dictEntryRenderObject != null)
								{
									long num4 = dictEntryRenderObject.ObjektBegin.BaiPlus8UInt32 + -8;
									if (16 < num4 && !"UIRoot".EqualsIgnoreCase(astInfo.PyObjTypName))
									{
										byte[] listeOktet = Extension.ListeOktetLeeseVonAdrese(memoryReader, num4, (long)SictGbsAstAusRenderObjectMemBlok.GbsBaumAstListeOktetAnzaal, false);
										SictGbsAstAusRenderObjectMemBlok gbsAstAusRenderObjectMemoryBlok = new SictGbsAstAusRenderObjectMemBlok(num4, listeOktet);
										sictInBerecnungAst2.GbsAstAusRenderObjectMemoryBlok = gbsAstAusRenderObjectMemoryBlok;
									}
								}
								byte? b = null;
								SictGbsAstAusRenderObjectMemBlok gbsAstAusRenderObjectMemoryBlok2 = sictInBerecnungAst2.GbsAstAusRenderObjectMemoryBlok;
								if (gbsAstAusRenderObjectMemoryBlok2 != null)
								{
									b = gbsAstAusRenderObjectMemoryBlok2.OktetSictbarkaitWert;
									float[] laage = gbsAstAusRenderObjectMemoryBlok2.Laage;
									float[] grööse = gbsAstAusRenderObjectMemoryBlok2.Grööse;
									if (laage != null)
									{
										astInfo.LaageInParent = new Vektor2DSingle(laage[0], laage[1]);
									}
									if (grööse != null)
									{
										astInfo.Grööse = new Vektor2DSingle(grööse[0], grööse[1]);
									}
								}
								bool Geändert3;
								if (pyObj.DictEntry_Sr is SictAuswertPyObj32BunchZuusctand sictAuswertPyObj32BunchZuusctand)
								{
									SictObjektDictEntryAusSrBunch sictObjektDictEntryAusSrBunch = new SictObjektDictEntryAusSrBunch();
									ScraibeDictEntryNaacMember(sictObjektDictEntryAusSrBunch, sictAuswertPyObj32BunchZuusctand.ListeDictEntry, memoryReader, sictProzesAuswertZuusctand, num2);
									if (sictObjektDictEntryAusSrBunch.DictEntryHtmlstr is SictAuswertPyObj32UnicodeZuusctand sictAuswertPyObj32UnicodeZuusctand)
									{
										sictAuswertPyObj32UnicodeZuusctand.Aktualisiire(memoryReader, out Geändert3, num2);
										astInfo.SrHtmlstr = sictAuswertPyObj32UnicodeZuusctand.WertString;
									}
									if (sictObjektDictEntryAusSrBunch.DictEntryNode is SictAuswertPyObj32BunchZuusctand sictAuswertPyObj32BunchZuusctand2)
									{
										sictAuswertPyObj32BunchZuusctand2.Aktualisiire(memoryReader, out Geändert3, num2);
										SictObjektDictEntryAusSrBunchNode sictObjektDictEntryAusSrBunchNode = new SictObjektDictEntryAusSrBunchNode();
										ScraibeDictEntryNaacMember(sictObjektDictEntryAusSrBunchNode, sictAuswertPyObj32BunchZuusctand2.ListeDictEntry, memoryReader, sictProzesAuswertZuusctand, num2);
										if (sictObjektDictEntryAusSrBunchNode.DictEntryGlyphString is SictAuswertPyObj32Tr2GlyphStringZuusctand sictAuswertPyObj32Tr2GlyphStringZuusctand)
										{
											sictAuswertPyObj32Tr2GlyphStringZuusctand.Aktualisiire(memoryReader, out Geändert3, num2);
											SictAuswertPyObj32DictZuusctand dictObj2 = sictAuswertPyObj32Tr2GlyphStringZuusctand.DictObj;
											if (dictObj2 != null)
											{
												dictObj2.ListeEntryAnzaalScrankeMax = 64;
												dictObj2.Aktualisiire(memoryReader, out Geändert3, num2);
												SictObjektDictEntryAusTr2GlyphStringDict sictObjektDictEntryAusTr2GlyphStringDict = new SictObjektDictEntryAusTr2GlyphStringDict();
												ScraibeDictEntryNaacMember(sictObjektDictEntryAusTr2GlyphStringDict, dictObj2.ListeDictEntry, memoryReader, sictProzesAuswertZuusctand, num2);
												if (sictObjektDictEntryAusTr2GlyphStringDict.DictEntryText is SictAuswertPyObj32UnicodeZuusctand sictAuswertPyObj32UnicodeZuusctand2)
												{
													sictAuswertPyObj32UnicodeZuusctand2.Aktualisiire(memoryReader, out Geändert3, num2);
													astInfo.EditTextlineCoreText = sictAuswertPyObj32UnicodeZuusctand2.WertString;
												}
											}
										}
									}
								}
								if (pyObj.DictEntryTexture is SictAuswertPyObj32TextureZuusctand sictAuswertPyObj32TextureZuusctand)
								{
									uint num5 = sictAuswertPyObj32TextureZuusctand.BaiPlus8Ref - 8;
									byte[] array4 = Extension.ListeOktetLeeseVonAdrese(memoryReader, (long)num5, 256L, true);
									int num6 = 80;
									if (num6 + 4 <= array4?.Count())
									{
										uint num7 = BitConverter.ToUInt32(array4, num6);
										astInfo.TextureIdent0 = num7;
									}
								}
								astInfo.VisibleIncludingInheritance = true;
								if (sictInBerecnungAst2.Parent != null)
								{
									if (1 != b)
									{
										astInfo.VisibleIncludingInheritance = false;
									}
									GbsAstInfo info = sictInBerecnungAst2.Parent.Info;
									if (info != null)
									{
										bool? visibleIncludingInheritance = info.VisibleIncludingInheritance;
										if (visibleIncludingInheritance != true || !visibleIncludingInheritance.HasValue || true == info.Minimized)
										{
											astInfo.VisibleIncludingInheritance = false;
										}
										if (true != info.VisibleIncludingInheritance)
										{
											astInfo.VisibleIncludingInheritance = false;
										}
									}
								}
								if (true != astInfo.VisibleIncludingInheritance)
								{
									astInfo.ListChild = null;
									continue;
								}
								if (pyObj.DictEntryChildren is SictAuswertPyObj32PyOderUiChildrenList sictAuswertPyObj32PyOderUiChildrenList)
								{
									if (BeginZaitMili > sictAuswertPyObj32PyOderUiChildrenList.AktualisLezteZait)
									{
										sictAuswertPyObj32PyOderUiChildrenList.Aktualisiire(memoryReader, out Geändert3, BeginZaitMili);
									}
									sictAuswertPyObj32PyOderUiChildrenList.DictObj?.Aktualisiire(memoryReader, out Geändert2, BeginZaitMili);
									MemberAusDictEntryScteleSicerAktuel(sictAuswertPyObj32PyOderUiChildrenList, memoryReader, sictProzesAuswertZuusctand, num2, null, MemberAktualisiire: true);
									if (!(sictAuswertPyObj32PyOderUiChildrenList.DictEntryListChildrenObj is SictAuswertPyObj32ListZuusctand sictAuswertPyObj32ListZuusctand))
									{
										pyObj.ListeChild.Clear();
									}
									else
									{
										sictAuswertPyObj32ListZuusctand.ListeItemAnzaalScrankeMax = 256;
										if (num2 > sictAuswertPyObj32ListZuusctand.AktualisLezteZait)
										{
											sictAuswertPyObj32ListZuusctand.Aktualisiire(memoryReader, out Geändert3, num2);
										}
										uint[] listeItemRef = sictAuswertPyObj32ListZuusctand.ListeItemRef;
										if (listeItemRef == null)
										{
											pyObj.ListeChild.Clear();
										}
										else
										{
											int num8 = Math.Min(astListeChildAnzaalScrankeMax ?? int.MaxValue, listeItemRef.Length);
											for (int j = 0; j < num8; j++)
											{
												uint num9 = listeItemRef[j];
												SictAuswertPyObj32GbsAstZuusctand sictAuswertPyObj32GbsAstZuusctand2 = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer<SictAuswertPyObj32GbsAstZuusctand>(num9, memoryReader, sictProzesAuswertZuusctand, num2);
												if (dictionary.ContainsKey(num9))
												{
													sictAuswertPyObj32GbsAstZuusctand2 = null;
												}
												if (pyObj.ListeChild.Count <= j)
												{
													pyObj.ListeChild.Add(sictAuswertPyObj32GbsAstZuusctand2);
												}
												else
												{
													pyObj.ListeChild[j] = sictAuswertPyObj32GbsAstZuusctand2;
												}
											}
											int num10 = pyObj.ListeChild.Count - num8;
											if (0 < num10)
											{
												pyObj.ListeChild.RemoveRange(num8, num10);
											}
										}
									}
								}
								pyObj.ListeChildPropagiireNaacInfoObjekt();
								foreach (SictAuswertPyObj32GbsAstZuusctand item in pyObj.ListeChild)
								{
									list3.Add(new SictInBerecnungAst(sictInBerecnungAst2, item, sictInBerecnungAst2.TiifeScrankeMax - 1));
								}
							}
							list.Add(sictInBerecnungAst.Info);
						}
					}
					finally
					{
						MengeGbsWurzelInfo = list.ToArray();
						ScritLezteMengeAstGeändert = list2.ToArray();
					}
				}
			}
			finally
			{
				ScritLezteBeginZaitMili = (sictMesungZaitraumAusStopwatch.BeginZaitMikro ?? 0) / 1000;
				sictMesungZaitraumAusStopwatch.EndeSezeJezt();
				ScritLezteEndeZaitMili = (sictMesungZaitraumAusStopwatch.EndeZaitMikro ?? 0) / 1000;
			}
		}
		finally
		{
		}
	}

	private static Type AssemblyTypeFürPyTypeObjAdrese(long PyTypeObjAdrese, SictProzesAuswertZuusctand ProzesAuswertZuusctandKlas)
	{
		Type type = ProzesAuswertZuusctandKlas.GibAssemblyTypFürPythonTypeMitHerkunftAdrese(PyTypeObjAdrese);
		if (null != type)
		{
			if (typeof(SictAuswertPythonObjInt) == type)
			{
				return typeof(SictAuswertPyObj32Int32Zuusctand);
			}
			if (typeof(SictAuswertPythonObjBool) == type)
			{
				return typeof(SictAuswertPyObj32BoolZuusctand);
			}
			if (typeof(SictAuswertPythonObjFloat) == type)
			{
				return typeof(SictAuswertPyObj32Float64Zuusctand);
			}
			if (typeof(SictAuswertPythonObjStr) == type)
			{
				return typeof(SictAuswertPyObj32StrZuusctand);
			}
			if (typeof(SictAuswertPythonObjLong) == type)
			{
				return typeof(SictAuswertPyObj32LongZuusctand);
			}
			if (typeof(SictAuswertPythonObjInstance) == type)
			{
				return typeof(SictAuswertPyObj32InstanceZuusctand);
			}
			if (typeof(SictAuswertPythonObjUnicode) == type)
			{
				return typeof(SictAuswertPyObj32UnicodeZuusctand);
			}
			if (typeof(SictAuswertPythonObjPyOderUiChildrenList) == type)
			{
				return typeof(SictAuswertPyObj32PyOderUiChildrenList);
			}
			if (typeof(SictAuswertPyObjGbsAst) == type)
			{
				return typeof(SictAuswertPyObj32GbsAstZuusctand);
			}
			if (typeof(SictAuswertPythonObjList) == type)
			{
				return typeof(SictAuswertPyObj32ListZuusctand);
			}
			if (typeof(SictAuswertPythonObjBunch) == type)
			{
				return typeof(SictAuswertPyObj32BunchZuusctand);
			}
			if (typeof(SictAuswertPythonObjTrinityTr2Sprite2dTexture) == type)
			{
				return typeof(SictAuswertPyObj32TextureZuusctand);
			}
			if (typeof(SictAuswertPythonObjTr2GlyphString) == type)
			{
				return typeof(SictAuswertPyObj32Tr2GlyphStringZuusctand);
			}
			if (typeof(SictAuswertPyObjPyColor) == type)
			{
				return typeof(SictAuswertPyObj32ColorZuusctand);
			}
		}
		return null;
	}

	public SictAuswertPyObj32Zuusctand ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(long HerkunftAdrese, IMemoryReader AusProzesLeeser, SictProzesAuswertZuusctand ProzesAuswertZuusctandKlas, long Zait)
	{
		return ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer<SictAuswertPyObj32Zuusctand>(HerkunftAdrese, AusProzesLeeser, ProzesAuswertZuusctandKlas, Zait, TypeIgnoriire: true);
	}

	public T ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer<T>(long HerkunftAdrese, IMemoryReader AusProzesLeeser, SictProzesAuswertZuusctand ProzesAuswertZuusctandKlas, long Zait, bool TypeIgnoriire = false) where T : SictAuswertPyObj32Zuusctand
	{
		SictAuswertPyObj32Zuusctand value = null;
		DictZuHerkunftAdreseObjekt.TryGetValue(HerkunftAdrese, out value);
		if (value != null && value.RefTypeGeändert)
		{
			DictZuHerkunftAdreseObjekt.Remove(HerkunftAdrese);
			value = null;
		}
		T val = value as T;
		if (val == null)
		{
			val = ((!TypeIgnoriire) ? ObjektFürHerkunftAdreseErsctele<T>(HerkunftAdrese, Zait) : (ObjektFürHerkunftAdreseErsctele(HerkunftAdrese, AusProzesLeeser, ProzesAuswertZuusctandKlas, Zait) as T));
			DictZuHerkunftAdreseObjekt[HerkunftAdrese] = val;
		}
		return val;
	}

	private SictAuswertPyObj32Zuusctand ObjektFürHerkunftAdreseErsctele(long herkunftAdrese, IMemoryReader ausProzesLeeser, SictProzesAuswertZuusctand prozesAuswertZuusctandKlas, long zait)
	{
		Type assemblyType = typeof(SictAuswertPyObj32Zuusctand);
		if (prozesAuswertZuusctandKlas != null)
		{
			SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand = new SictAuswertPyObj32Zuusctand(herkunftAdrese, zait);
			sictAuswertPyObj32Zuusctand.Aktualisiire(ausProzesLeeser, out var _, zait);
			long refType = sictAuswertPyObj32Zuusctand.RefType;
			Type type = AssemblyTypeFürPyTypeObjAdrese(refType, prozesAuswertZuusctandKlas);
			if (null != type)
			{
				assemblyType = type;
			}
		}
		return ObjektFürHerkunftAdreseErsctele(herkunftAdrese, assemblyType, zait);
	}

	private T ObjektFürHerkunftAdreseErsctele<T>(long herkunftAdrese, long zait) where T : SictAuswertPyObj32Zuusctand
	{
		return ObjektFürHerkunftAdreseErsctele(herkunftAdrese, typeof(T), zait) as T;
	}

	private SictAuswertPyObj32Zuusctand ObjektFürHerkunftAdreseErsctele(long herkunftAdrese, Type assemblyType, long zait)
	{
		if (null == assemblyType)
		{
			return null;
		}
		ConstructorInfo constructorInfo = ScatescpaicerZuTypeKonstruktorHerkunftAdreseUndBeginZait.ValueFürKey(assemblyType, ZuTypeKonstruktorHerkunftAdreseUndBeginZaitBerecne);
		if (null == constructorInfo)
		{
			return null;
		}
		object obj = constructorInfo.Invoke(new object[2] { herkunftAdrese, zait });
		return obj as SictAuswertPyObj32Zuusctand;
	}

	public static KeyValuePair<string, SictZuMemberAusDictEntryInfo>[] ZuTypeMengeZuDictKeyFieldMemberInfoAusScatenscpaicerOderBerecne(Type ziilType)
	{
		return ScatescpaicerZuTypeMengeZuDictKeyFieldMemberInfo.ValueFürKey(ziilType, ZuTypeMengeZuDictKeyFieldMemberInfoBerecne);
	}

	public static KeyValuePair<string, SictZuMemberAusDictEntryInfo>[] ZuTypeMengeZuDictKeyFieldMemberInfoBerecne(Type ziilType)
	{
		if (null == ziilType)
		{
			return null;
		}
		List<KeyValuePair<string, SictZuMemberAusDictEntryInfo>> list = new List<KeyValuePair<string, SictZuMemberAusDictEntryInfo>>();
		FieldInfo[] fields = ziilType.GetFields();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(SictInPyDictEntryKeyAttribut), inherit: true);
			if (customAttributes == null || !typeof(SictAuswertPyObj32Zuusctand).IsAssignableFrom(fieldInfo.FieldType))
			{
				continue;
			}
			object[] array2 = customAttributes;
			foreach (object obj in array2)
			{
				if (obj is SictInPyDictEntryKeyAttribut { DictEntryKeyString: var dictEntryKeyString })
				{
					MemberSetter setter = FieldExtensions.DelegateForSetFieldValue(ziilType, fieldInfo.Name);
					MemberGetter getter = FieldExtensions.DelegateForGetFieldValue(ziilType, fieldInfo.Name);
					list.Add(new KeyValuePair<string, SictZuMemberAusDictEntryInfo>(dictEntryKeyString, new SictZuMemberAusDictEntryInfo(fieldInfo.FieldType, setter, getter)));
				}
			}
		}
		return list.ToArray();
	}

	public void ScraibeDictEntryNaacMember(object ziilObjekt, SictAuswertPyObj32DictZuusctand dict, IMemoryReader ausProzesLeeser, SictProzesAuswertZuusctand prozesAuswertZuusctandKlas, long zait, long? objektErhaltungBeginZaitScrankeMin = null, bool memberAktualisiire = false)
	{
		SictPyDictEntry32[] listeDictEntry = dict?.ListeDictEntry;
		ScraibeDictEntryNaacMember(ziilObjekt, listeDictEntry, ausProzesLeeser, prozesAuswertZuusctandKlas, zait, objektErhaltungBeginZaitScrankeMin, memberAktualisiire);
	}

	public void ScraibeDictEntryNaacMember(object ZiilObjekt, SictPyDictEntry32[] ListeDictEntry, IMemoryReader AusProzesLeeser, SictProzesAuswertZuusctand ProzesAuswertZuusctandKlas, long Zait, long? ObjektErhaltungBeginZaitScrankeMin = null, bool MemberAktualisiire = false)
	{
		if (ZiilObjekt == null)
		{
			return;
		}
		Type type = ZiilObjekt.GetType();
		KeyValuePair<string, SictZuMemberAusDictEntryInfo>[] array = ZuTypeMengeZuDictKeyFieldMemberInfoAusScatenscpaicerOderBerecne(type);
		if (array.IsNullOrEmpty())
		{
			return;
		}
		long[] array2 = new long[array.Length];
		if (ListeDictEntry != null)
		{
			for (int i = 0; i < ListeDictEntry.Length; i++)
			{
				SictPyDictEntry32 sictPyDictEntry = ListeDictEntry[i];
				uint referenzKey = sictPyDictEntry.ReferenzKey;
				uint referenzValue = sictPyDictEntry.ReferenzValue;
				if (referenzKey == 0)
				{
					continue;
				}
				SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(referenzKey, AusProzesLeeser, ProzesAuswertZuusctandKlas, Zait);
				if (!(sictAuswertPyObj32Zuusctand is SictAuswertPyObj32StrZuusctand sictAuswertPyObj32StrZuusctand))
				{
					continue;
				}
				if (Zait > sictAuswertPyObj32StrZuusctand.AktualisLezteZait)
				{
					sictAuswertPyObj32StrZuusctand.Aktualisiire(AusProzesLeeser, out var _, Zait);
				}
				string wertString = sictAuswertPyObj32StrZuusctand.WertString;
				if (wertString == null)
				{
					continue;
				}
				for (int j = 0; j < array.Length; j++)
				{
					KeyValuePair<string, SictZuMemberAusDictEntryInfo> keyValuePair = array[j];
					try
					{
						if (!string.Equals(wertString, keyValuePair.Key))
						{
							continue;
						}
						array2[j] = referenzValue;
						break;
					}
					finally
					{
					}
				}
			}
		}
		for (int k = 0; k < array.Length; k++)
		{
			KeyValuePair<string, SictZuMemberAusDictEntryInfo> keyValuePair2 = array[k];
			object obj = keyValuePair2.Value.Getter.Invoke(ZiilObjekt);
			long num = array2[k];
			SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand2 = null;
			try
			{
				if (num == 0)
				{
					continue;
				}
				if (obj is SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand3)
				{
					if (sictAuswertPyObj32Zuusctand3.RefTypeGeändert)
					{
					}
					if (sictAuswertPyObj32Zuusctand3.HerkunftAdrese == num && !sictAuswertPyObj32Zuusctand3.RefTypeGeändert)
					{
						sictAuswertPyObj32Zuusctand2 = sictAuswertPyObj32Zuusctand3;
					}
				}
				if (sictAuswertPyObj32Zuusctand2 == null)
				{
					sictAuswertPyObj32Zuusctand2 = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(num, AusProzesLeeser, ProzesAuswertZuusctandKlas, Zait);
				}
			}
			finally
			{
				if (sictAuswertPyObj32Zuusctand2 != null)
				{
					if (MemberAktualisiire)
					{
						if (Zait > sictAuswertPyObj32Zuusctand2.AktualisLezteZait)
						{
							sictAuswertPyObj32Zuusctand2.Aktualisiire(AusProzesLeeser, out var _, Zait);
						}
					}
					else if (Zait <= sictAuswertPyObj32Zuusctand2.AktualisLezteZait)
					{
					}
				}
				keyValuePair2.Value.Setter.Invoke(ZiilObjekt, (object)sictAuswertPyObj32Zuusctand2);
			}
		}
	}

	public void AstInfoScteleSicerAktuelOoneListeChild(SictAuswertPyObj32GbsAstZuusctand GbsAst, IMemoryReader AusProzesLeeser, SictProzesAuswertZuusctand ProzesAuswertZuusctand, long Zait, long? ObjektErhaltungBeginZaitScrankeMin)
	{
		if (AusProzesLeeser == null || GbsAst == null)
		{
			return;
		}
		GbsAst.VonDictEntryMemberEntferneWelceRefTypeGeändert(Zait);
		MemberAusDictEntryScteleSicerAktuel(GbsAst, AusProzesLeeser, ProzesAuswertZuusctand, Zait, ObjektErhaltungBeginZaitScrankeMin, MemberAktualisiire: true);
		KeyValuePair<string, SictZuMemberAusDictEntryInfo>[] array = ZuTypeMengeZuDictKeyFieldMemberInfoAusScatenscpaicerOderBerecne(GbsAst.GetType());
		if (array != null)
		{
			KeyValuePair<string, SictZuMemberAusDictEntryInfo>[] array2 = array;
			foreach (KeyValuePair<string, SictZuMemberAusDictEntryInfo> keyValuePair in array2)
			{
				if (keyValuePair.Value.Getter.Invoke((object)GbsAst) is SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand && Zait > sictAuswertPyObj32Zuusctand.AktualisLezteZait)
				{
					sictAuswertPyObj32Zuusctand.Aktualisiire(AusProzesLeeser, out var _, Zait);
				}
			}
		}
		GbsAstInfo astInfo = GbsAst.AstInfo;
		if (GbsAst == null || astInfo == null)
		{
			return;
		}
		SictAuswertPythonObjType typeObjektKlas = GbsAst.TypeObjektKlas;
		if (typeObjektKlas != null)
		{
			astInfo.PyObjTypName = typeObjektKlas.tp_name;
		}
		_16_Erwaiterung_Manuel(Zait, AusProzesLeeser, ProzesAuswertZuusctand, GbsAst, astInfo);
		SictAuswertPyObj32Zuusctand dictEntryName = GbsAst.DictEntryName;
		if (dictEntryName is SictAuswertPyObj32StrZuusctand sictAuswertPyObj32StrZuusctand)
		{
			astInfo.Name = sictAuswertPyObj32StrZuusctand.WertString;
		}
		SictAuswertPyObj32Zuusctand dictEntryHint = GbsAst.DictEntryHint;
		if (dictEntryHint is SictAuswertPyObj32StrZuusctand sictAuswertPyObj32StrZuusctand2)
		{
			astInfo.Hint = sictAuswertPyObj32StrZuusctand2.WertString;
		}
		if (dictEntryHint is SictAuswertPyObj32UnicodeZuusctand sictAuswertPyObj32UnicodeZuusctand)
		{
			astInfo.Hint = sictAuswertPyObj32UnicodeZuusctand.WertString;
		}
		SictAuswertPyObj32Zuusctand dictEntryWindowId = GbsAst.DictEntryWindowId;
		if (dictEntryWindowId is SictAuswertPyObj32StrZuusctand sictAuswertPyObj32StrZuusctand3)
		{
			astInfo.WindowID = sictAuswertPyObj32StrZuusctand3.WertString;
		}
		SictAuswertPyObj32Zuusctand dictEntryCapacitorLevel = GbsAst.DictEntryCapacitorLevel;
		if (dictEntryCapacitorLevel is SictAuswertPyObj32Float64Zuusctand sictAuswertPyObj32Float64Zuusctand)
		{
			astInfo.CapacitorLevel = (float)sictAuswertPyObj32Float64Zuusctand.WertFloat64;
		}
		SictAuswertPyObj32Zuusctand dictEntryShieldLevel = GbsAst.DictEntryShieldLevel;
		if (dictEntryShieldLevel is SictAuswertPyObj32Float64Zuusctand sictAuswertPyObj32Float64Zuusctand2)
		{
			astInfo.ShieldLevel = (float)sictAuswertPyObj32Float64Zuusctand2.WertFloat64;
		}
		SictAuswertPyObj32Zuusctand dictEntryArmorLevel = GbsAst.DictEntryArmorLevel;
		if (dictEntryArmorLevel is SictAuswertPyObj32Float64Zuusctand sictAuswertPyObj32Float64Zuusctand3)
		{
			astInfo.ArmorLevel = (float)sictAuswertPyObj32Float64Zuusctand3.WertFloat64;
		}
		SictAuswertPyObj32Zuusctand dictEntryStructureLevel = GbsAst.DictEntryStructureLevel;
		if (dictEntryStructureLevel is SictAuswertPyObj32Float64Zuusctand sictAuswertPyObj32Float64Zuusctand4)
		{
			astInfo.StructureLevel = (float)sictAuswertPyObj32Float64Zuusctand4.WertFloat64;
		}
		SictAuswertPyObj32Zuusctand dictEntrySpeed = GbsAst.DictEntrySpeed;
		if (dictEntrySpeed is SictAuswertPyObj32Float64Zuusctand sictAuswertPyObj32Float64Zuusctand5)
		{
			astInfo.Speed = (float)sictAuswertPyObj32Float64Zuusctand5.WertFloat64;
		}
		SictAuswertPyObj32Zuusctand dictEntryText = GbsAst.DictEntryText;
		if (dictEntryText != null)
		{
		}
		SictAuswertPyObj32StrZuusctand sictAuswertPyObj32StrZuusctand4 = dictEntryText as SictAuswertPyObj32StrZuusctand;
		SictAuswertPyObj32UnicodeZuusctand sictAuswertPyObj32UnicodeZuusctand2 = dictEntryText as SictAuswertPyObj32UnicodeZuusctand;
		if (sictAuswertPyObj32StrZuusctand4 != null)
		{
			astInfo.Text = sictAuswertPyObj32StrZuusctand4.WertString;
		}
		else if (sictAuswertPyObj32UnicodeZuusctand2 != null)
		{
			astInfo.Text = sictAuswertPyObj32UnicodeZuusctand2.WertString;
		}
		SictAuswertPyObj32Zuusctand dictEntrySetText = GbsAst.DictEntrySetText;
		if (dictEntrySetText != null)
		{
		}
		SictAuswertPyObj32StrZuusctand sictAuswertPyObj32StrZuusctand5 = dictEntrySetText as SictAuswertPyObj32StrZuusctand;
		SictAuswertPyObj32UnicodeZuusctand sictAuswertPyObj32UnicodeZuusctand3 = dictEntrySetText as SictAuswertPyObj32UnicodeZuusctand;
		string setText = null;
		if (sictAuswertPyObj32StrZuusctand5 != null)
		{
			setText = sictAuswertPyObj32StrZuusctand5.WertString;
		}
		else if (sictAuswertPyObj32UnicodeZuusctand3 != null)
		{
			setText = sictAuswertPyObj32UnicodeZuusctand3.WertString;
		}
		else if (dictEntrySetText is SictAuswertPyObj32Int32Zuusctand { WertInt32: var wertInt })
		{
			setText = wertInt.ToString();
		}
		astInfo.SetText = setText;
		SictAuswertPyObj32Zuusctand dictEntryCaption = GbsAst.DictEntryCaption;
		SictAuswertPyObj32StrZuusctand sictAuswertPyObj32StrZuusctand6 = dictEntryCaption as SictAuswertPyObj32StrZuusctand;
		SictAuswertPyObj32UnicodeZuusctand sictAuswertPyObj32UnicodeZuusctand4 = dictEntryCaption as SictAuswertPyObj32UnicodeZuusctand;
		if (sictAuswertPyObj32StrZuusctand6 != null)
		{
			astInfo.Caption = sictAuswertPyObj32StrZuusctand6.WertString;
		}
		else if (sictAuswertPyObj32UnicodeZuusctand4 != null)
		{
			astInfo.Caption = sictAuswertPyObj32UnicodeZuusctand4.WertString;
		}
		SictAuswertPyObj32Zuusctand dictEntryLinkText = GbsAst.DictEntryLinkText;
		if (dictEntryLinkText != null)
		{
		}
		SictAuswertPyObj32StrZuusctand sictAuswertPyObj32StrZuusctand7 = dictEntryLinkText as SictAuswertPyObj32StrZuusctand;
		SictAuswertPyObj32UnicodeZuusctand sictAuswertPyObj32UnicodeZuusctand5 = dictEntryLinkText as SictAuswertPyObj32UnicodeZuusctand;
		if (sictAuswertPyObj32StrZuusctand7 != null)
		{
			astInfo.LinkText = sictAuswertPyObj32StrZuusctand7.WertString;
		}
		else if (sictAuswertPyObj32UnicodeZuusctand5 != null)
		{
			astInfo.LinkText = sictAuswertPyObj32UnicodeZuusctand5.WertString;
		}
		SictAuswertPyObj32Zuusctand dictEntryLastState = GbsAst.DictEntryLastState;
		if (dictEntryLastState != null)
		{
		}
		if (dictEntryLastState is SictAuswertPyObj32Float64Zuusctand sictAuswertPyObj32Float64Zuusctand6)
		{
			astInfo.LastStateFloat = sictAuswertPyObj32Float64Zuusctand6.WertFloat64;
		}
		SictAuswertPyObj32Zuusctand dictEntryLastSetCapacitor = GbsAst.DictEntryLastSetCapacitor;
		if (dictEntryLastSetCapacitor is SictAuswertPyObj32Float64Zuusctand sictAuswertPyObj32Float64Zuusctand7)
		{
			astInfo.LastSetCapacitorFloat = sictAuswertPyObj32Float64Zuusctand7.WertFloat64;
		}
		SictAuswertPyObj32Zuusctand dictEntryLastValue = GbsAst.DictEntryLastValue;
		if (dictEntryLastValue != null)
		{
		}
		if (dictEntryLastValue is SictAuswertPyObj32Float64Zuusctand sictAuswertPyObj32Float64Zuusctand8)
		{
			astInfo.LastValueFloat = sictAuswertPyObj32Float64Zuusctand8.WertFloat64;
		}
		SictAuswertPyObj32Zuusctand dictEntryRotation = GbsAst.DictEntryRotation;
		if (dictEntryRotation != null)
		{
		}
		if (dictEntryRotation is SictAuswertPyObj32Float64Zuusctand sictAuswertPyObj32Float64Zuusctand9)
		{
			astInfo.RotationFloat = sictAuswertPyObj32Float64Zuusctand9.WertFloat64;
		}
		SictAuswertPyObj32Zuusctand dictEntryMinimized = GbsAst.DictEntryMinimized;
		if (dictEntryMinimized is SictAuswertPyObj32BoolZuusctand sictAuswertPyObj32BoolZuusctand)
		{
			astInfo.Minimized = sictAuswertPyObj32BoolZuusctand.WertBool;
		}
		SictAuswertPyObj32Zuusctand dictEntryIsModal = GbsAst.DictEntryIsModal;
		if (dictEntryIsModal is SictAuswertPyObj32BoolZuusctand sictAuswertPyObj32BoolZuusctand2)
		{
			astInfo.isModal = sictAuswertPyObj32BoolZuusctand2.WertBool;
		}
		SictAuswertPyObj32Zuusctand sictAuswertPyObj32Zuusctand2 = GbsAst.DictEntryOverviewEntryIsSelected ?? GbsAst.DictEntryTreeViewEntryIsSelected;
		if (sictAuswertPyObj32Zuusctand2 is SictAuswertPyObj32BoolZuusctand sictAuswertPyObj32BoolZuusctand3)
		{
			astInfo.isSelected = sictAuswertPyObj32BoolZuusctand3.WertBool;
		}
		SictAuswertPyObj32Zuusctand dictEntryTexturePath = GbsAst.DictEntryTexturePath;
		if (dictEntryTexturePath is SictAuswertPyObj32StrZuusctand sictAuswertPyObj32StrZuusctand8)
		{
			astInfo.texturePath = sictAuswertPyObj32StrZuusctand8.WertString;
		}
		bool Geändert2;
		if (GbsAst.DictEntryColor is SictAuswertPyObj32ColorZuusctand { DictObj: { } dictObj } sictAuswertPyObj32ColorZuusctand)
		{
			if (Zait > dictObj.AktualisLezteZait)
			{
				dictObj.Aktualisiire(AusProzesLeeser, out Geändert2, Zait);
			}
			MemberAusDictEntryScteleSicerAktuel(sictAuswertPyObj32ColorZuusctand, AusProzesLeeser, ProzesAuswertZuusctand, Zait, null, MemberAktualisiire: true);
			SictAuswertPyObj32Zuusctand dictEntryA = sictAuswertPyObj32ColorZuusctand.DictEntryA;
			SictAuswertPyObj32Zuusctand dictEntryR = sictAuswertPyObj32ColorZuusctand.DictEntryR;
			SictAuswertPyObj32Zuusctand dictEntryG = sictAuswertPyObj32ColorZuusctand.DictEntryG;
			SictAuswertPyObj32Zuusctand dictEntryB = sictAuswertPyObj32ColorZuusctand.DictEntryB;
			if (dictEntryA != null || dictEntryR != null || dictEntryG != null || dictEntryB != null)
			{
				astInfo.ColorAMili = (int?)WertMiliAusPyObj(dictEntryA);
				astInfo.ColorRMili = (int?)WertMiliAusPyObj(dictEntryR);
				astInfo.ColorGMili = (int?)WertMiliAusPyObj(dictEntryG);
				astInfo.ColorBMili = (int?)WertMiliAusPyObj(dictEntryB);
			}
		}
		SictAuswertPyObj32Zuusctand dictEntryBackgroundList = GbsAst.DictEntryBackgroundList;
		if (!(dictEntryBackgroundList is SictAuswertPyObj32PyOderUiChildrenList sictAuswertPyObj32PyOderUiChildrenList))
		{
			return;
		}
		sictAuswertPyObj32PyOderUiChildrenList.Aktualisiire(AusProzesLeeser, out Geändert2, Zait);
		sictAuswertPyObj32PyOderUiChildrenList.DictObj?.Aktualisiire(AusProzesLeeser, out Geändert2, Zait);
		MemberAusDictEntryScteleSicerAktuel(sictAuswertPyObj32PyOderUiChildrenList, AusProzesLeeser, ProzesAuswertZuusctand, Zait, null, MemberAktualisiire: true);
		if (!(sictAuswertPyObj32PyOderUiChildrenList.DictEntryListChildrenObj is SictAuswertPyObj32ListZuusctand sictAuswertPyObj32ListZuusctand))
		{
			return;
		}
		sictAuswertPyObj32ListZuusctand.ListeItemAnzaalScrankeMax = 16;
		sictAuswertPyObj32ListZuusctand.Aktualisiire(AusProzesLeeser, out Geändert2, Zait);
		List<GbsAstInfo> list = new List<GbsAstInfo>();
		foreach (uint item in sictAuswertPyObj32ListZuusctand.ListeItemRef.EmptyIfNull())
		{
			SictAuswertPyObj32GbsAstZuusctand sictAuswertPyObj32GbsAstZuusctand = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer<SictAuswertPyObj32GbsAstZuusctand>(item, AusProzesLeeser, ProzesAuswertZuusctand, Zait);
			sictAuswertPyObj32GbsAstZuusctand.Aktualisiire(AusProzesLeeser, out Geändert2, Zait);
			SictAuswertPyObj32DictZuusctand dictObj2 = sictAuswertPyObj32GbsAstZuusctand.DictObj;
			if (dictObj2 != null)
			{
				dictObj2.Aktualisiire(AusProzesLeeser, out Geändert2, Zait);
				AstInfoScteleSicerAktuelOoneListeChild(sictAuswertPyObj32GbsAstZuusctand, AusProzesLeeser, ProzesAuswertZuusctand, Zait, null);
				list.Add(sictAuswertPyObj32GbsAstZuusctand.AstInfo);
			}
		}
		astInfo.BackgroundList = list.ToArray();
	}

	public void MemberAusDictEntryScteleSicerAktuel(SictAuswertPyObj32MitBaiPlus8RefDictZuusctand pyObjMitDict, IMemoryReader AusProzesLeeser, SictProzesAuswertZuusctand ProzesAuswertZuusctand, long Zait, long? ObjektErhaltungBeginZaitScrankeMin = null, bool MemberAktualisiire = false)
	{
		if (AusProzesLeeser != null && pyObjMitDict != null)
		{
			WertZuZaitpunktStruct<SictPyDictEntry32[]> wertZuZaitpunktStruct = DictZuPyObjPropagatioonDictEntryNaacMemberLezteListeDictEntry.TryGetValueOrDefault(pyObjMitDict);
			SictAuswertPyObj32DictZuusctand dictObj = pyObjMitDict.DictObj;
			SictPyDictEntry32[] array = dictObj?.ListeDictEntry;
			bool flag = false;
			if (wertZuZaitpunktStruct.Wert == array)
			{
				flag = true;
			}
			else if (wertZuZaitpunktStruct.Wert != null && array != null)
			{
				flag = array.SequenceEqual(wertZuZaitpunktStruct.Wert);
			}
			if (!flag || array != null)
			{
			}
			bool flag2 = false;
			if (wertZuZaitpunktStruct.Zait <= pyObjMitDict.VonDictEntryMemberAktualisatioonNootwendigLezteZait)
			{
				flag2 = true;
			}
			flag2 = true;
			if (!flag || flag2)
			{
				DictZuPyObjPropagatioonDictEntryNaacMemberLezteListeDictEntry[pyObjMitDict] = new WertZuZaitpunktStruct<SictPyDictEntry32[]>(array, Zait);
				ScraibeDictEntryNaacMember(pyObjMitDict, dictObj, AusProzesLeeser, ProzesAuswertZuusctand, Zait, ObjektErhaltungBeginZaitScrankeMin, MemberAktualisiire);
			}
		}
	}

	public static IEnumerable<KeyValuePair<InGbsPfaad, GbsAstInfo[]>> MengeGbsAstSuuceNaacPfaad(IEnumerable<InGbsPfaad> MengeNaacGbsAstPfaad, int ProcessId, SictProzesAuswertZuusctand GbsSuuceWurzel)
	{
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		InGbsPfaad[] array = MengeNaacGbsAstPfaad?.ToArray();
		if (array == null)
		{
			return null;
		}
		if (GbsSuuceWurzel == null)
		{
			return null;
		}
		List<KeyValuePair<InGbsPfaad, long>> list = (from NaacGbsAstPfaad in array
			select new KeyValuePair<InGbsPfaad, long?>(NaacGbsAstPfaad, NaacGbsAstPfaad.WurzelAstAdrese) into PfaadUndSuuceWurzelAdrese
			where PfaadUndSuuceWurzelAdrese.Value.HasValue
			select new KeyValuePair<InGbsPfaad, long>(PfaadUndSuuceWurzelAdrese.Key, PfaadUndSuuceWurzelAdrese.Value.Value)).ToList();
		long[] array2 = list.Select((KeyValuePair<InGbsPfaad, long> PfaadUndSuuceWurzelAdrese) => PfaadUndSuuceWurzelAdrese.Value).ToArray();
		List<long> list2 = new List<long>();
		foreach (KeyValuePair<InGbsPfaad, long> item in list)
		{
			InGbsPfaad key = item.Key;
			if (key != null)
			{
				long[] listeAstAdrese = key.ListeAstAdrese;
				if (listeAstAdrese != null)
				{
					list2.AddRange(listeAstAdrese);
				}
			}
		}
		GbsAstInfo[] array3 = null;
		if (!array2.IsNullOrEmpty())
		{
			SictProzesAuswertZuusctandScpezGbsBaum sictProzesAuswertZuusctandScpezGbsBaum = new SictProzesAuswertZuusctandScpezGbsBaum((IMemoryReader)new ProcessMemoryReader(ProcessId), GbsSuuceWurzel, 1111, 11111, null, array2, list2);
			sictProzesAuswertZuusctandScpezGbsBaum.BerecneScrit();
			array3 = sictProzesAuswertZuusctandScpezGbsBaum.MengeGbsWurzelInfo;
		}
		KeyValuePair<InGbsPfaad, GbsAstInfo[]>[] array4 = new KeyValuePair<InGbsPfaad, GbsAstInfo[]>[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			InGbsPfaad NaacGbsAstPfaad2 = array[i];
			long[] listeAstAdrese2 = NaacGbsAstPfaad2.ListeAstAdrese;
			GbsAstInfo[] value = null;
			try
			{
				int index = list.FindIndex((KeyValuePair<InGbsPfaad, long> Kandidaat) => Kandidaat.Key == NaacGbsAstPfaad2);
				GbsAstInfo gbsAstInfo = array3?.ElementAtOrDefault(index);
				if (gbsAstInfo != null)
				{
					if (listeAstAdrese2.IsNullOrEmpty())
					{
						value = new GbsAstInfo[1] { gbsAstInfo };
					}
					long PfaadBlatAdrese = listeAstAdrese2.LastOrDefault();
					value = gbsAstInfo.SuuceFlacMengeAstMitPfaadFrüheste((GbsAstInfo KandidaatBlat) => KandidaatBlat.PyObjAddress == PfaadBlatAdrese);
				}
			}
			finally
			{
				array4[i] = new KeyValuePair<InGbsPfaad, GbsAstInfo[]>(NaacGbsAstPfaad2, value);
			}
		}
		return array4;
	}
}
