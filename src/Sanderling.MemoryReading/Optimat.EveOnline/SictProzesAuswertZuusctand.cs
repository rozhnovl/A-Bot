using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bib3;
using BotEngine.Interface;
using Fasterflect;
using Extension = BotEngine.Interface.Extension;

namespace Optimat.EveOnline;

public class SictProzesAuswertZuusctand
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class SictInPyDictEntryKeyAttribut : Attribute
	{
		public readonly string DictEntryKeyString;

		public SictInPyDictEntryKeyAttribut(string DictEntryKeyString)
		{
			this.DictEntryKeyString = DictEntryKeyString;
		}
	}

	public class SictSuuceMengeDictEntryScpezGbsAstErgeebnis
	{
		[SictInPyDictEntryKeyAttribut("_parentRef")]
		public SictAuswertPythonDictEntryAinfac DictEntryParentRef;

		[SictInPyDictEntryKeyAttribut("children")]
		public SictAuswertPythonDictEntryAinfac DictEntryChildren;

		[SictInPyDictEntryKeyAttribut("renderObject")]
		public SictAuswertPythonDictEntryAinfac DictEntryRenderObject;

		[SictInPyDictEntryKeyAttribut("_name")]
		public SictAuswertPythonDictEntryAinfac DictEntryName;

		[SictInPyDictEntryKeyAttribut("lastState")]
		public SictAuswertPythonDictEntryAinfac DictEntryLastState;

		[SictInPyDictEntryKeyAttribut("_rotation")]
		public SictAuswertPythonDictEntryAinfac DictEntryRotation;

		[SictInPyDictEntryKeyAttribut("_texture")]
		public SictAuswertPythonDictEntryAinfac DictEntryTexture;

		[SictInPyDictEntryKeyAttribut("_color")]
		public SictAuswertPythonDictEntryAinfac DictEntryColor;

		[SictInPyDictEntryKeyAttribut("_hint")]
		public SictAuswertPythonDictEntryAinfac DictEntryHint;

		[SictInPyDictEntryKeyAttribut("_sr")]
		public SictAuswertPythonDictEntryAinfac DictEntry_Sr;

		[SictInPyDictEntryKeyAttribut("_text")]
		public SictAuswertPythonDictEntryAinfac DictEntryText;

		[SictInPyDictEntryKeyAttribut("linkText")]
		public SictAuswertPythonDictEntryAinfac DictEntryLinkText;

		[SictInPyDictEntryKeyAttribut("_setText")]
		public SictAuswertPythonDictEntryAinfac DictEntrySetText;

		[SictInPyDictEntryKeyAttribut("isModal")]
		public SictAuswertPythonDictEntryAinfac DictEntryIsModal;

		[SictInPyDictEntryKeyAttribut("_caption")]
		public SictAuswertPythonDictEntryAinfac DictEntryCaption;

		[SictInPyDictEntryKeyAttribut("_minimized")]
		public SictAuswertPythonDictEntryAinfac DictEntryMinimized;

		[SictInPyDictEntryKeyAttribut("isDialog")]
		public SictAuswertPythonDictEntryAinfac DictEntryIsDialog;

		[SictInPyDictEntryKeyAttribut("windowID")]
		public SictAuswertPythonDictEntryAinfac DictEntryWindowId;

		[SictInPyDictEntryKeyAttribut("_pinned")]
		public SictAuswertPythonDictEntryAinfac DictEntryPinned;
	}

	protected SictMesungZaitraumAusStopwatch InternDauer;

	public Dictionary<long, SictAuswertPythonObj> MengeFürHerkunftAdrPyObj = new Dictionary<long, SictAuswertPythonObj>();

	public const string GbsPyChildrenListSclüselOwnerRefString = "_ownerRef";

	public const string PyChildrenListSclüselChildrenObjectsString = "_childrenObjects";

	public const string GbsAstSclüselParentRefString = "_parentRef";

	public const string GbsAstSclüselChildrenString = "children";

	public const string GbsAstSclüselRenderObjectString = "renderObject";

	public const string GbsAstSclüselNameString = "_name";

	public const string GbsAstSclüselLastValueString = "_lastValue";

	public const string GbsAstSclüselLastStateString = "lastState";

	public const string GbsAstSclüselRotationString = "_rotation";

	public const string GbsAstSclüselTextureString = "_texture";

	public const string GbsAstSclüselColorString = "_color";

	public const string GbsAstSclüselHintString = "_hint";

	public const string GbsAstSclüsel_SrString = "_sr";

	public const string GbsAstSclüselLastRenderDataString = "_lastRenderData";

	public const string GbsAstLabelSclüselTextString = "_text";

	public const string GbsAstLabelSclüselLinkTextString = "linkText";

	public const string GbsAstLabelSclüselSetTextString = "_setText";

	public const string GbsAstWindowSclüselIsModalString = "isModal";

	public const string GbsAstWindowSclüselBackgroundListString = "_backgroundlist";

	public const string OverviewScrollEntrySclüselIsSelectedString = "_isSelected";

	public const string TreeViewEntrySclüselIsSelectedString = "isSelected";

	public const string GbsAstWindowSclüselCaptionString = "_caption";

	public const string GbsAstWindowSclüselMinimizedString = "_minimized";

	public const string GbsAstWindowSclüselIsDialogString = "isDialog";

	public const string GbsAstWindowSclüselWindowIdString = "windowID";

	public const string GbsAstWindowSclüselPinnedString = "_pinned";

	public const string GbsAstWindowSclüselTexturePathString = "texturePath";

	public const string GbsAstSrBunchSclüselHtmlstrString = "htmlstr";

	public const string GbsAstSrBunchSclüselNodeString = "node";

	public const string GbsAstShipUISclüselShieldFülsctandString = "lastShield";

	public const string GbsAstShipUISclüselArmorFülsctandString = "lastArmor";

	public const string GbsAstShipUISclüselStructureFülsctandString = "lastStructure";

	public const string GbsAstShipUISclüselCapacitorFülsctandString = "lastsetcapacitor";

	public const string GbsAstShipUISclüselSpeedString = "lastSpeed";

	public static readonly string[] GbsListeRenderObjectTyp = new string[6] { "trinity.Tr2Sprite2dScene", "trinity.Tr2Sprite2dContainer", "trinity.Tr2Sprite2dTransform", "EveLabelSmall", "EveLabelMedium", "EveLabelLarge" };

	public SictAuswertPythonObjStr PyObjStrGbsAstEntryChildren;

	private static Dictionary<Type, KeyValuePair<string, MemberSetter>[]> DictZuTypeMengeZuDictKeyFieldSetter = new Dictionary<Type, KeyValuePair<string, MemberSetter>[]>();

	public SictMesungZaitraumAusStopwatch Dauer => InternDauer;

	public SictAuswertPythonObjType[] MengePyObjTyp { get; protected set; }

	public SictAuswertPythonObjType PyObjTypType { get; protected set; }

	public SictAuswertPythonObjType PyObjTypWeakref { get; protected set; }

	public SictAuswertPythonObjType PyObjTypStr { get; protected set; }

	public SictAuswertPythonObjType PyObjTypLong { get; protected set; }

	public SictAuswertPythonObjType PyObjTypInstance { get; protected set; }

	public SictAuswertPythonObjType PyObjTypInt { get; protected set; }

	public SictAuswertPythonObjType PyObjTypBool { get; protected set; }

	public SictAuswertPythonObjType PyObjTypFloat { get; protected set; }

	public SictAuswertPythonObjType PyObjTypList { get; protected set; }

	public SictAuswertPythonObjType PyObjTypDict { get; protected set; }

	public SictAuswertPythonObjType PyObjTypTuple { get; protected set; }

	public SictAuswertPythonObjType PyObjTypBunch { get; protected set; }

	public SictAuswertPythonObjType PyObjTypUnicode { get; protected set; }

	public SictAuswertPythonObjType PyObjTypPyChildrenList { get; protected set; }

	public SictAuswertPythonObjType PyObjTypBackgroundList { get; protected set; }

	public SictAuswertPythonObjType PyObjTypUIChildrenListAutoSize { get; protected set; }

	public SictAuswertPythonObjType PyObjTypUIRoot { get; protected set; }

	public SictAuswertPythonObjType PyObjTypPyColor { get; protected set; }

	public SictAuswertPythonObjType PyObjTypTrinityTr2Sprite2dTexture { get; protected set; }

	public SictAuswertPythonObjType PyObjTypTr2GlyphString { get; protected set; }

	public SictAuswertPythonObjPyOderUiChildrenList[] MengePyObjPyChildrenList { get; protected set; }

	public SictAuswertPyObjGbsAst[] AusPyChildrenListMengeGbsAst { get; protected set; }

	public SictAuswertPyObjGbsAst[] GbsMengeWurzelObj { get; protected set; }

	public SictAuswertPyObjGbsAst[] MengeGbsAst { get; protected set; }

	public void AusMengePyObjEntferneMitHerkunftAdrese(long HerkunftAdrese)
	{
		AusMengePyObjEntferne((SictAuswertPythonObj Kandidaat) => Kandidaat.HerkunftAdrese == HerkunftAdrese);
	}

	public void AusMengePyObjEntferne(Func<SictAuswertPythonObj, bool> Prädikat)
	{
		if (Prädikat != null)
		{
			KeyValuePair<long, SictAuswertPythonObj>[] array = MengeFürHerkunftAdrPyObj.Where((KeyValuePair<long, SictAuswertPythonObj> Kandidaat) => Prädikat(Kandidaat.Value)).ToArray();
			KeyValuePair<long, SictAuswertPythonObj>[] array2 = array;
			foreach (KeyValuePair<long, SictAuswertPythonObj> keyValuePair in array2)
			{
				MengeFürHerkunftAdrPyObj.Remove(keyValuePair.Key);
			}
		}
	}

	public void MengePyObjTypSezeAusMengeFürHerkunftAdrPyObj()
	{
		SictAuswertPythonObjType[] mengePyObjTyp = null;
		try
		{
			Dictionary<long, SictAuswertPythonObj> mengeFürHerkunftAdrPyObj = MengeFürHerkunftAdrPyObj;
			if (mengeFürHerkunftAdrPyObj != null)
			{
				mengePyObjTyp = (from Kandidaat in mengeFürHerkunftAdrPyObj.Select((KeyValuePair<long, SictAuswertPythonObj> FürHerkunftAdrPyObj) => FürHerkunftAdrPyObj.Value).OfType<SictAuswertPythonObjType>()
					orderby Kandidaat.tp_name
					select Kandidaat).ToArray();
			}
		}
		finally
		{
			MengePyObjTyp = mengePyObjTyp;
		}
	}

	public void FüleRefTypScpezVonMengePyObjType()
	{
		MengePyObjTypSezeAusMengeFürHerkunftAdrPyObj();
		SictAuswertPythonObjType[] array = MengePyObjTyp;
		if (array == null)
		{
			array = new SictAuswertPythonObjType[0];
		}
		PyObjTypType = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("type", Kandidaat.tp_name));
		PyObjTypWeakref = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("weakref", Kandidaat.tp_name));
		PyObjTypInt = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("int", Kandidaat.tp_name));
		PyObjTypBool = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("bool", Kandidaat.tp_name));
		PyObjTypFloat = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("float", Kandidaat.tp_name));
		PyObjTypStr = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("str", Kandidaat.tp_name));
		PyObjTypLong = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("long", Kandidaat.tp_name));
		PyObjTypInstance = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("instance", Kandidaat.tp_name));
		PyObjTypList = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("list", Kandidaat.tp_name));
		PyObjTypDict = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("dict", Kandidaat.tp_name));
		PyObjTypTuple = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("tuple", Kandidaat.tp_name));
		PyObjTypBunch = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("Bunch", Kandidaat.tp_name));
		PyObjTypUnicode = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("unicode", Kandidaat.tp_name));
		PyObjTypPyChildrenList = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("PyChildrenList", Kandidaat.tp_name));
		PyObjTypBackgroundList = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("BackgroundList", Kandidaat.tp_name));
		PyObjTypUIChildrenListAutoSize = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("UIChildrenListAutoSize", Kandidaat.tp_name));
		PyObjTypUIRoot = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("UIRoot", Kandidaat.tp_name));
		PyObjTypPyColor = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("PyColor", Kandidaat.tp_name));
		PyObjTypTrinityTr2Sprite2dTexture = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("trinity.Tr2Sprite2dTexture", Kandidaat.tp_name));
		PyObjTypTr2GlyphString = array.FirstOrDefault((SictAuswertPythonObjType Kandidaat) => string.Equals("Tr2GlyphString", Kandidaat.tp_name));
	}

	public void KopiireVon(SictProzesAuswertZuusctand ZuKopiire)
	{
		if (ZuKopiire != null)
		{
			MengeFürHerkunftAdrPyObj = new Dictionary<long, SictAuswertPythonObj>(ZuKopiire.MengeFürHerkunftAdrPyObj);
			MengePyObjTyp = MengeFürHerkunftAdrPyObj.Select((KeyValuePair<long, SictAuswertPythonObj> HerkunftAdrUndPyObj) => HerkunftAdrUndPyObj.Value).OfType<SictAuswertPythonObjType>().ToArray();
			FüleRefTypScpezVonMengePyObjType();
			MengePyObjPyChildrenList = ZuKopiire.MengePyObjPyChildrenList;
			GbsMengeWurzelObj = ZuKopiire.GbsMengeWurzelObj;
		}
	}

	public SictAuswertPythonDictEntryAinfac InPyBunchSuuceEntryFürKeyString(SictAuswertPythonObjBunch PyBunch, string Key, IMemoryReader ProzesLeeser, bool ScraibePyObjNaacScpaicer, bool ErmitleTypNurAusScpaicer = false, int? ListeEntryAnzaalScrankeMax = null)
	{
		if (PyBunch == null)
		{
			return null;
		}
		if (Key == null)
		{
			return null;
		}
		SictAuswertPythonDictEntryAinfac[] listeEntry = PyBunch.ListeEntry;
		if (listeEntry == null)
		{
			PyBunch.LaadeReferenziirte(ProzesLeeser, ListeEntryAnzaalScrankeMax);
			listeEntry = PyBunch.ListeEntry;
		}
		return InListePyDictEntrySuuceEntryFürKeyString(listeEntry, Key, ProzesLeeser, ScraibePyObjNaacScpaicer, ErmitleTypNurAusScpaicer);
	}

	public static KeyValuePair<string, MemberSetter>[] ZuTypeMengeZuDictKeyFieldSetterBerecne(Type Type)
	{
		if (null == Type)
		{
			return null;
		}
		List<KeyValuePair<string, MemberSetter>> list = new List<KeyValuePair<string, MemberSetter>>();
		FieldInfo[] fields = Type.GetFields();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(SictInPyDictEntryKeyAttribut), inherit: true);
			if (customAttributes == null)
			{
				continue;
			}
			object[] array2 = customAttributes;
			foreach (object obj in array2)
			{
				if (obj is SictInPyDictEntryKeyAttribut { DictEntryKeyString: var dictEntryKeyString })
				{
					MemberSetter value = FieldExtensions.DelegateForSetFieldValue(Type, fieldInfo.Name);
					list.Add(new KeyValuePair<string, MemberSetter>(dictEntryKeyString, value));
				}
			}
		}
		return list.ToArray();
	}

	public SictSuuceMengeDictEntryScpezGbsAstErgeebnis InDictMengeEntryScpezGbsAstBerecne(SictAuswertPythonObjDict PyDict, IMemoryReader ProzesLeeser, bool ScraibePyObjNaacScpaicer, bool ErmitleTypNurAusScpaicer = false, int? ListeEntryAnzaalScrankeMax = null)
	{
		SictAuswertPythonDictEntryAinfac[] listeDictEntry = PyDict.ListeDictEntry;
		if (listeDictEntry == null)
		{
			PyDict.LaadeReferenziirte(ProzesLeeser, ListeEntryAnzaalScrankeMax);
			listeDictEntry = PyDict.ListeDictEntry;
		}
		if (listeDictEntry == null)
		{
			return null;
		}
		return InMengeDictEntryMengeEntryScpezGbsAstBerecne(listeDictEntry, ProzesLeeser, ScraibePyObjNaacScpaicer, ErmitleTypNurAusScpaicer);
	}

	public object InMengeDictEntryMengeEntryScpezFürZiilObjektTypeBerecne(object ZiilObjekt, SictAuswertPythonDictEntryAinfac[] ListePyDictEntry, IMemoryReader ProzesLeeser, bool ScraibePyObjNaacScpaicer, bool ErmitleTypNurAusScpaicer = false)
	{
		if (ListePyDictEntry == null)
		{
			return null;
		}
		if (ZiilObjekt == null)
		{
			return null;
		}
		Type type = ZiilObjekt.GetType();
		KeyValuePair<string, MemberSetter>[] array = DictZuTypeMengeZuDictKeyFieldSetter.TryGetValueOrDefault(type);
		if (array == null)
		{
			array = ZuTypeMengeZuDictKeyFieldSetterBerecne(type);
			DictZuTypeMengeZuDictKeyFieldSetter[type] = array;
		}
		if (DictZuTypeMengeZuDictKeyFieldSetter == null)
		{
			return null;
		}
		foreach (SictAuswertPythonDictEntryAinfac sictAuswertPythonDictEntryAinfac in ListePyDictEntry)
		{
			if (sictAuswertPythonDictEntryAinfac == null)
			{
				continue;
			}
			long referenzKey = sictAuswertPythonDictEntryAinfac.ReferenzKey;
			if (referenzKey == 0)
			{
				continue;
			}
			SictAuswertPythonObj sictAuswertPythonObj = sictAuswertPythonDictEntryAinfac.Key;
			if (sictAuswertPythonObj == null)
			{
				sictAuswertPythonObj = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(referenzKey, ProzesLeeser, ErmitleTypNurAusScpaicer);
				if (ScraibePyObjNaacScpaicer)
				{
					PyObjSezeNaacScpaicer(sictAuswertPythonObj);
				}
				sictAuswertPythonDictEntryAinfac.Key = sictAuswertPythonObj;
			}
			if (!(sictAuswertPythonObj is SictAuswertPythonObjStr { String: { } @string }))
			{
				continue;
			}
			KeyValuePair<string, MemberSetter>[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				KeyValuePair<string, MemberSetter> keyValuePair = array2[j];
				if (string.Equals(@string, keyValuePair.Key))
				{
					keyValuePair.Value.Invoke(ZiilObjekt, (object)sictAuswertPythonDictEntryAinfac);
				}
			}
		}
		return ZiilObjekt;
	}

	public SictSuuceMengeDictEntryScpezGbsAstErgeebnis InMengeDictEntryMengeEntryScpezGbsAstBerecne(SictAuswertPythonDictEntryAinfac[] ListePyDictEntry, IMemoryReader ProzesLeeser, bool ScraibePyObjNaacScpaicer, bool ErmitleTypNurAusScpaicer = false)
	{
		if (ListePyDictEntry == null)
		{
			return null;
		}
		SictSuuceMengeDictEntryScpezGbsAstErgeebnis ziilObjekt = new SictSuuceMengeDictEntryScpezGbsAstErgeebnis();
		return InMengeDictEntryMengeEntryScpezFürZiilObjektTypeBerecne(ziilObjekt, ListePyDictEntry, ProzesLeeser, ScraibePyObjNaacScpaicer, ErmitleTypNurAusScpaicer) as SictSuuceMengeDictEntryScpezGbsAstErgeebnis;
	}

	public SictAuswertPythonDictEntryAinfac InPyDictSuuceEntryFürKeyString(SictAuswertPythonObjDict PyDict, string Key, IMemoryReader ProzesLeeser, bool ScraibePyObjNaacScpaicer, bool ErmitleTypNurAusScpaicer = false, int? ListeEntryAnzaalScrankeMax = null)
	{
		if (PyDict == null)
		{
			return null;
		}
		if (Key == null)
		{
			return null;
		}
		SictAuswertPythonDictEntryAinfac[] listeDictEntry = PyDict.ListeDictEntry;
		if (listeDictEntry == null)
		{
			PyDict.LaadeReferenziirte(ProzesLeeser, ListeEntryAnzaalScrankeMax);
			listeDictEntry = PyDict.ListeDictEntry;
		}
		return InListePyDictEntrySuuceEntryFürKeyString(listeDictEntry, Key, ProzesLeeser, ScraibePyObjNaacScpaicer, ErmitleTypNurAusScpaicer);
	}

	public static bool KeyStringEqualSictbarkaitProfiler(string str0, string str1)
	{
		return string.Equals(str0, str1);
	}

	public SictAuswertPythonDictEntryAinfac InListePyDictEntrySuuceEntryFürKeyString(SictAuswertPythonDictEntryAinfac[] ListePyDictEntry, string Key, IMemoryReader ProzesLeeser, bool ScraibePyObjNaacScpaicer, bool ErmitleTypNurAusScpaicer = false)
	{
		if (ListePyDictEntry == null)
		{
			return null;
		}
		if (Key == null)
		{
			return null;
		}
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < ListePyDictEntry.Length; i++)
		{
			SictAuswertPythonDictEntryAinfac sictAuswertPythonDictEntryAinfac = ListePyDictEntry[i];
			if (sictAuswertPythonDictEntryAinfac == null)
			{
				continue;
			}
			long referenzKey = sictAuswertPythonDictEntryAinfac.ReferenzKey;
			if (referenzKey == 0)
			{
				continue;
			}
			SictAuswertPythonObj key = sictAuswertPythonDictEntryAinfac.Key;
			if (key == null)
			{
				num2 = i;
				if (num < 0)
				{
					num = i;
				}
			}
			else if (key is SictAuswertPythonObjStr sictAuswertPythonObjStr && KeyStringEqualSictbarkaitProfiler(Key, sictAuswertPythonObjStr.String))
			{
				return sictAuswertPythonDictEntryAinfac;
			}
		}
		if (0 <= num)
		{
			for (int j = num; j <= num2; j++)
			{
				SictAuswertPythonDictEntryAinfac sictAuswertPythonDictEntryAinfac2 = ListePyDictEntry[j];
				long referenzKey2 = sictAuswertPythonDictEntryAinfac2.ReferenzKey;
				if (referenzKey2 == 0)
				{
					continue;
				}
				SictAuswertPythonObj sictAuswertPythonObj = sictAuswertPythonDictEntryAinfac2.Key;
				if (sictAuswertPythonObj == null)
				{
					sictAuswertPythonObj = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(referenzKey2, ProzesLeeser, ErmitleTypNurAusScpaicer);
					if (ScraibePyObjNaacScpaicer)
					{
						PyObjSezeNaacScpaicer(sictAuswertPythonObj);
					}
					sictAuswertPythonDictEntryAinfac2.Key = sictAuswertPythonObj;
				}
				if (sictAuswertPythonObj is SictAuswertPythonObjStr sictAuswertPythonObjStr2 && string.Equals(Key, sictAuswertPythonObjStr2.String))
				{
					return sictAuswertPythonDictEntryAinfac2;
				}
			}
		}
		return null;
	}

	public void PyObjDictEntryFüleAusScpaicer(SictAuswertPythonDictEntry Entry)
	{
		if (Entry != null)
		{
			MengeFürHerkunftAdrPyObj.TryGetValue(Entry.ReferenzKey, out var value);
			MengeFürHerkunftAdrPyObj.TryGetValue(Entry.ReferenzValue, out var value2);
			Entry.Key = value;
			Entry.Value = value2;
		}
	}

	public void PyObjDictEntryFüleAusScpaicer(SictAuswertPythonDictEntryAinfac Entry)
	{
		if (Entry != null)
		{
			MengeFürHerkunftAdrPyObj.TryGetValue(Entry.ReferenzKey, out var value);
			MengeFürHerkunftAdrPyObj.TryGetValue(Entry.ReferenzValue, out var value2);
			Entry.Key = value;
			Entry.Value = value2;
		}
	}

	public void PyObjDictEntryFüleAusScpaicerOderProzes(SictAuswertPythonDictEntry Entry, IMemoryReader ProzesLeeser, bool ObjSezeNaacScpaicer = false, bool ErmitleTypNurAusScpaicer = true)
	{
		if (Entry == null)
		{
			return;
		}
		PyObjDictEntryFüleAusScpaicer(Entry);
		if (ProzesLeeser != null)
		{
			if (Entry.Key == null)
			{
				Entry.Key = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(Entry.ReferenzKey, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			}
			if (Entry.Value == null)
			{
				Entry.Value = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(Entry.ReferenzValue, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			}
		}
	}

	public void PyObjDictEntryFüleAusScpaicerOderProzes(SictAuswertPythonDictEntryAinfac Entry, IMemoryReader ProzesLeeser, bool ObjSezeNaacScpaicer = false, bool ErmitleTypNurAusScpaicer = false)
	{
		if (Entry == null)
		{
			return;
		}
		PyObjDictEntryFüleAusScpaicer(Entry);
		if (ProzesLeeser != null)
		{
			if (Entry.Key == null)
			{
				Entry.Key = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(Entry.ReferenzKey, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			}
			if (Entry.Value == null)
			{
				Entry.Value = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(Entry.ReferenzValue, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			}
		}
	}

	public SictAuswertPythonObj PyObjFürHerkunftAdreseAusScpaicer(long PyObjHerkunftAdr)
	{
		MengeFürHerkunftAdrPyObj.TryGetValue(PyObjHerkunftAdr, out var value);
		return value;
	}

	public SictAuswertPythonObj PyObjFürHerkunftAdreseAusScpaicerOderErsctele(long PyObjHerkunftAdr, IMemoryReader ProzesLeeser, bool ObjSezeNaacScpaicer = false, bool ErmitleTypNurAusScpaicer = false, int? RekursScranke = null)
	{
		return PyObjFürHerkunftAdreseAusScpaicerOderErsctele(null, PyObjHerkunftAdr, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer, RekursScranke);
	}

	public Typ PyObjFürHerkunftAdreseAusScpaicerOderErsctele<Typ>(long PyObjHerkunftAdr, IMemoryReader ProzesLeeser, bool ObjSezeNaacScpaicer = false, bool ErmitleTypNurAusScpaicer = false, int? RekursScranke = null) where Typ : SictAuswertPythonObj
	{
		Type typeFromHandle = typeof(Typ);
		SictAuswertPythonObj sictAuswertPythonObj = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(typeFromHandle, PyObjHerkunftAdr, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer, RekursScranke);
		return sictAuswertPythonObj as Typ;
	}

	public SictAuswertPythonObj PyObjFürHerkunftAdreseAusScpaicerOderErsctele(Type ObjAssemblyTyp, long PyObjHerkunftAdr, IMemoryReader ProzesLeeser, bool ObjSezeNaacScpaicer = false, bool ErmitleTypNurAusScpaicer = false, int? RekursScranke = null)
	{
		SictAuswertPythonObj sictAuswertPythonObj = PyObjFürHerkunftAdreseAusScpaicer(PyObjHerkunftAdr);
		if (sictAuswertPythonObj != null)
		{
			return sictAuswertPythonObj;
		}
		sictAuswertPythonObj = PyObjFürHerkunftAdreseErsctele(ObjAssemblyTyp, PyObjHerkunftAdr, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer, RekursScranke);
		if (ObjSezeNaacScpaicer && sictAuswertPythonObj != null)
		{
			PyObjSezeNaacScpaicer(sictAuswertPythonObj);
		}
		return sictAuswertPythonObj;
	}

	public Type GibAssemblyTypFürPythonTypeMitHerkunftAdrese(long HerkunftAdrese)
	{
		SictAuswertPythonObjType pyObjType = PyObjFürHerkunftAdreseAusScpaicer(HerkunftAdrese) as SictAuswertPythonObjType;
		return GibAssemblyTypFürPythonType(pyObjType);
	}

	public Type GibAssemblyTypFürPythonType(SictAuswertPythonObjType PyObjType)
	{
		Type typeFromHandle = typeof(SictAuswertPythonObj);
		if (PyObjType == null)
		{
			return typeFromHandle;
		}
		if (PyObjType == PyObjTypType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjType);
		}
		if (PyObjType == PyObjTypWeakref)
		{
			typeFromHandle = typeof(SictAuswertPythonObjWeakRef);
		}
		if (PyObjType == PyObjTypDict)
		{
			typeFromHandle = typeof(SictAuswertPythonObjDict);
		}
		if (PyObjTypList == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjList);
		}
		if (PyObjTypTuple == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjTuple);
		}
		if (PyObjTypBunch == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjBunch);
		}
		if (PyObjTypUnicode == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjUnicode);
		}
		if (PyObjTypInt == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjInt);
		}
		if (PyObjTypBool == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjBool);
		}
		if (PyObjTypFloat == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjFloat);
		}
		if (PyObjTypStr == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjStr);
		}
		if (PyObjTypLong == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjLong);
		}
		if (PyObjTypInstance == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjInstance);
		}
		if (PyObjTypPyChildrenList == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjPyOderUiChildrenList);
		}
		if (PyObjTypUIChildrenListAutoSize == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjPyOderUiChildrenList);
		}
		if (PyObjTypBackgroundList == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjPyOderUiChildrenList);
		}
		if (PyObjTypUIRoot == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPyObjGbsAst);
		}
		if (PyObjTypPyColor == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPyObjPyColor);
		}
		if (PyObjTypTrinityTr2Sprite2dTexture == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjTrinityTr2Sprite2dTexture);
		}
		if (PyObjTypTr2GlyphString == PyObjType)
		{
			typeFromHandle = typeof(SictAuswertPythonObjTr2GlyphString);
		}
		return typeFromHandle;
	}

	public SictAuswertPythonObj PyObjFürTypErsctele(SictAuswertPythonObjType PyObjType, long HerkunftAdrese, byte[] ListeOktet, IMemoryReader ProzesLeeser)
	{
		Type assemblyType = GibAssemblyTypFürPythonType(PyObjType);
		return PyObjFürTypErsctele(assemblyType, HerkunftAdrese, ListeOktet, ProzesLeeser);
	}

	public Typ PyObjFürTypGenErsctele<Typ>(long HerkunftAdrese, byte[] ListeOktet, IMemoryReader ProzesLeeser) where Typ : SictAuswertPythonObj
	{
		Type typeFromHandle = typeof(Typ);
		SictAuswertPythonObj sictAuswertPythonObj = PyObjFürTypErsctele(typeFromHandle, HerkunftAdrese, ListeOktet, ProzesLeeser);
		return sictAuswertPythonObj as Typ;
	}

	public SictAuswertPythonObj PyObjFürTypErsctele(Type AssemblyType, long HerkunftAdrese, byte[] ListeOktet, IMemoryReader ProzesLeeser)
	{
		SictMesungZaitraumAusStopwatch sictMesungZaitraumAusStopwatch = new SictMesungZaitraumAusStopwatch(beginJezt: true);
		SictMesungZaitraumAusStopwatch sictMesungZaitraumAusStopwatch2 = new SictMesungZaitraumAusStopwatch(beginJezt: true);
		try
		{
			if (null == AssemblyType)
			{
				return null;
			}
			if (!typeof(SictAuswertPythonObj).IsAssignableFrom(AssemblyType))
			{
				return null;
			}
			ConstructorInfo[] constructors = AssemblyType.GetConstructors();
			ConstructorInfo[] array = constructors.OrderByDescending((ConstructorInfo Kandidaat) => Kandidaat.GetParameters().Length).ToArray();
			ConstructorInfo[] array2 = array;
			foreach (ConstructorInfo constructorInfo in array2)
			{
				ParameterInfo[] parameters = constructorInfo.GetParameters();
				if (3 == parameters.Length)
				{
					ParameterInfo parameterInfo = parameters[0];
					ParameterInfo parameterInfo2 = parameters[1];
					ParameterInfo parameterInfo3 = parameters[2];
					if (!(parameterInfo.ParameterType != typeof(long)) && !(parameterInfo2.ParameterType != typeof(byte[])) && !(parameterInfo3.ParameterType != typeof(IMemoryReader)))
					{
						object[] parameters2 = new object[3] { HerkunftAdrese, ListeOktet, ProzesLeeser };
						sictMesungZaitraumAusStopwatch2.EndeSezeJezt();
						object obj = constructorInfo.Invoke(parameters2);
						return obj as SictAuswertPythonObj;
					}
				}
			}
			return null;
		}
		finally
		{
			sictMesungZaitraumAusStopwatch.EndeSezeJezt();
		}
	}

	public SictAuswertPythonObj PyObjFürHerkunftAdreseErsctele(Type ObjAssemblyTyp, long PyObjHerkunftAdr, IMemoryReader ProzesLeeser, bool ObjSezeNaacScpaicer = false, bool ErmitleTypNurAusScpaicer = false, int? RekursScranke = null)
	{
		if (ProzesLeeser == null)
		{
			return null;
		}
		byte[] listeOktet = Extension.ListeOktetLeeseVonAdrese(ProzesLeeser, PyObjHerkunftAdr, 256L, false);
		SictAuswertPythonObjType sictAuswertPythonObjType = null;
		if (null == ObjAssemblyTyp)
		{
			SictAuswertPythonObj sictAuswertPythonObj = new SictAuswertPythonObj(PyObjHerkunftAdr, listeOktet);
			SictAuswertPythonObj sictAuswertPythonObj2 = PyObjFürHerkunftAdreseAusScpaicer(sictAuswertPythonObj.RefType);
			if (sictAuswertPythonObj2 == null && !ErmitleTypNurAusScpaicer && !(RekursScranke < 1))
			{
				sictAuswertPythonObj2 = PyObjFürHerkunftAdreseAusScpaicerOderErsctele<SictAuswertPythonObjType>(sictAuswertPythonObj.RefType, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer, (RekursScranke ?? 2) - 1);
			}
			sictAuswertPythonObjType = sictAuswertPythonObj2 as SictAuswertPythonObjType;
			ObjAssemblyTyp = GibAssemblyTypFürPythonType(sictAuswertPythonObjType);
		}
		SictAuswertPythonObj sictAuswertPythonObj3 = PyObjFürTypErsctele(ObjAssemblyTyp, PyObjHerkunftAdr, listeOktet, ProzesLeeser);
		sictAuswertPythonObj3.ObjType = sictAuswertPythonObjType;
		return sictAuswertPythonObj3;
	}

	public bool AssemblyTypHinraicendGenerelZuRefPyObjTyp(SictAuswertPythonObj PyObj)
	{
		if (PyObj == null)
		{
			return false;
		}
		long refType = PyObj.RefType;
		Type type = GibAssemblyTypFürPythonTypeMitHerkunftAdrese(refType);
		return type.IsAssignableFrom(PyObj.GetType());
	}

	public void LaadeType(SictAuswertPythonObj PyObj, IMemoryReader ProzesLeeser, bool ObjSezeNaacScpaicer = false, bool ErmitleTypNurAusScpaicer = true)
	{
		if (PyObj != null)
		{
			SictAuswertPythonObjType objType = PyObj.ObjType;
			long refType = PyObj.RefType;
			if (objType == null && refType != 0)
			{
				objType = (PyObj.ObjType = PyObjFürHerkunftAdreseAusScpaicerOderErsctele<SictAuswertPythonObjType>(refType, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer));
			}
		}
	}

	public void LaadeReferenziirte(ISictAuswertPythonObjMitRefDict PyObj, IMemoryReader ProzesLeeser, bool ObjSezeNaacScpaicer = false, bool ErmitleTypNurAusScpaicer = true)
	{
		if (PyObj == null)
		{
			return;
		}
		long refDict = PyObj.RefDict;
		if (PyObj.Dict == null && refDict != 0)
		{
			SictAuswertPythonObjDict sictAuswertPythonObjDict = PyObjFürHerkunftAdreseAusScpaicerOderErsctele<SictAuswertPythonObjDict>(refDict, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			if (AssemblyTypHinraicendGenerelZuRefPyObjTyp(sictAuswertPythonObjDict))
			{
				PyObj.Dict = sictAuswertPythonObjDict;
			}
		}
	}

	public void LaadeReferenziirte(SictAuswertPyObjGbsAst PyObj, IMemoryReader ProzesLeeser, bool ObjSezeNaacScpaicer = false, bool ErmitleTypNurAusScpaicer = true, int? DictListeEntryAnzaalScrankeMax = 1024)
	{
		if (PyObj == null)
		{
			return;
		}
		LaadeReferenziirte((ISictAuswertPythonObjMitRefDict)PyObj, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
		SictAuswertPythonObjDict dict = PyObj.Dict;
		if (dict == null)
		{
			return;
		}
		SictSuuceMengeDictEntryScpezGbsAstErgeebnis sictSuuceMengeDictEntryScpezGbsAstErgeebnis = InDictMengeEntryScpezGbsAstBerecne(dict, ProzesLeeser, ScraibePyObjNaacScpaicer: true, ErmitleTypNurAusScpaicer, DictListeEntryAnzaalScrankeMax);
		if (sictSuuceMengeDictEntryScpezGbsAstErgeebnis == null)
		{
			return;
		}
		SictAuswertPythonDictEntryAinfac dictEntryParentRef = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryParentRef;
		if (dictEntryParentRef != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryParentRef, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			if (dictEntryParentRef.Value is SictAuswertPythonObjWeakRef sictAuswertPythonObjWeakRef)
			{
				PyObj.AusDictParentRef = sictAuswertPythonObjWeakRef.RefBaiOktet8;
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryRenderObject = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryRenderObject;
		if (dictEntryRenderObject != null)
		{
			long referenzValue = dictEntryRenderObject.ReferenzValue;
			PyObj.AusDictRenderObjectRef = referenzValue;
			if (referenzValue != 0)
			{
				PyObj.AusDictRenderObject = PyObjFürHerkunftAdreseAusScpaicerOderErsctele<SictAuswertPythonObjMitRefBaiPlus8>(referenzValue, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryName = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryName;
		if (dictEntryName != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryName, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictName = dictEntryName.Value;
			if (dictEntryName.Value is SictAuswertPythonObjStr sictAuswertPythonObjStr)
			{
				sictAuswertPythonObjStr.LaadeReferenziirte(ProzesLeeser);
				PyObj.AusDictNameString = sictAuswertPythonObjStr.String;
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryText = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryText;
		if (dictEntryText != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryText, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictText = dictEntryText.Value;
			if (dictEntryText.Value is SictAuswertPythonObjUnicode sictAuswertPythonObjUnicode)
			{
				sictAuswertPythonObjUnicode.LaadeReferenziirte(ProzesLeeser);
				PyObj.AusDictTextString = sictAuswertPythonObjUnicode.String;
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryLinkText = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryLinkText;
		if (dictEntryLinkText != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryLinkText, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictLinkText = dictEntryLinkText.Value;
			if (dictEntryLinkText.Value is SictAuswertPythonObjUnicode sictAuswertPythonObjUnicode2)
			{
				sictAuswertPythonObjUnicode2.LaadeReferenziirte(ProzesLeeser);
				PyObj.AusDictLinkTextString = sictAuswertPythonObjUnicode2.String;
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntrySetText = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntrySetText;
		if (dictEntrySetText != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntrySetText, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictSetText = dictEntrySetText.Value;
			if (dictEntrySetText.Value is SictAuswertPythonObjStr sictAuswertPythonObjStr2)
			{
				sictAuswertPythonObjStr2.LaadeReferenziirte(ProzesLeeser);
				PyObj.AusDictSetTextString = sictAuswertPythonObjStr2.String;
			}
			if (dictEntrySetText.Value is SictAuswertPythonObjUnicode sictAuswertPythonObjUnicode3)
			{
				sictAuswertPythonObjUnicode3.LaadeReferenziirte(ProzesLeeser);
				PyObj.AusDictSetTextString = sictAuswertPythonObjUnicode3.String;
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryCaption = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryCaption;
		if (dictEntryCaption != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryCaption, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictCaption = dictEntryCaption.Value;
			if (dictEntryCaption.Value is SictAuswertPythonObjStr sictAuswertPythonObjStr3)
			{
				sictAuswertPythonObjStr3.LaadeReferenziirte(ProzesLeeser);
				PyObj.AusDictCaptionString = sictAuswertPythonObjStr3.String;
			}
			if (dictEntryCaption.Value is SictAuswertPythonObjUnicode sictAuswertPythonObjUnicode4)
			{
				sictAuswertPythonObjUnicode4.LaadeReferenziirte(ProzesLeeser);
				PyObj.AusDictCaptionString = sictAuswertPythonObjUnicode4.String;
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryWindowId = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryWindowId;
		if (dictEntryWindowId != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryWindowId, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictWindowID = dictEntryWindowId.Value;
			if (dictEntryWindowId.Value is SictAuswertPythonObjStr sictAuswertPythonObjStr4)
			{
				PyObj.AusDictWindowIDString = sictAuswertPythonObjStr4.String;
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryMinimized = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryMinimized;
		if (dictEntryMinimized != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryMinimized, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictMinimized = dictEntryMinimized.Value;
			if (dictEntryMinimized.Value is SictAuswertPythonObjBool sictAuswertPythonObjBool)
			{
				PyObj.AusDictMinimizedBool = sictAuswertPythonObjBool.Bool;
			}
			if (dictEntryMinimized.Value is SictAuswertPythonObjInt sictAuswertPythonObjInt)
			{
				PyObj.AusDictMinimizedInt = sictAuswertPythonObjInt.Int;
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryIsModal = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryIsModal;
		if (dictEntryIsModal != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryIsModal, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictIsModal = dictEntryIsModal.Value;
			if (dictEntryIsModal.Value is SictAuswertPythonObjBool sictAuswertPythonObjBool2)
			{
				PyObj.AusDictIsModalBool = sictAuswertPythonObjBool2.Bool;
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryLastState = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryLastState;
		if (dictEntryLastState != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryLastState, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictLastState = dictEntryLastState.Value;
			if (dictEntryLastState.Value is SictAuswertPythonObjFloat sictAuswertPythonObjFloat)
			{
				PyObj.AusDictLastStateFloat = sictAuswertPythonObjFloat.Float;
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryRotation = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryRotation;
		if (dictEntryRotation != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryRotation, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictRotation = dictEntryRotation.Value;
			if (dictEntryRotation.Value is SictAuswertPythonObjFloat sictAuswertPythonObjFloat2)
			{
				PyObj.AusDictRotationFloat = sictAuswertPythonObjFloat2.Float;
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryTexture = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryTexture;
		if (dictEntryTexture != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryTexture, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictTexture = dictEntryTexture.Value;
			if (dictEntryTexture.Value is SictAuswertPythonObjTrinityTr2Sprite2dTexture sictAuswertPythonObjTrinityTr2Sprite2dTexture)
			{
				long num = sictAuswertPythonObjTrinityTr2Sprite2dTexture.RefBaiOktet8 - 8;
				byte[] array = Extension.ListeOktetLeeseVonAdrese(ProzesLeeser, num, 256L, false);
				if (array != null)
				{
					uint num2 = BitConverter.ToUInt32(array, 80);
					PyObj.AusDictTextureMemBlockPlus80Ref = num2;
				}
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryColor = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryColor;
		if (dictEntryColor != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryColor, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDictColor = dictEntryColor.Value;
			if (dictEntryColor.Value is SictAuswertPyObjPyColor pyObj)
			{
				LaadeReferenziirte(pyObj, ProzesLeeser, ObjSezeNaacScpaicer: true, ErmitleTypNurAusScpaicer: true, 16);
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntry_Sr = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntry_Sr;
		if (dictEntry_Sr != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(dictEntry_Sr, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			PyObj.AusDict_Sr = dictEntry_Sr.Value;
			if (dictEntry_Sr.Value is SictAuswertPythonObjBunch sictAuswertPythonObjBunch)
			{
				sictAuswertPythonObjBunch.LaadeReferenziirte(ProzesLeeser, 256);
				SictAuswertPythonDictEntryAinfac sictAuswertPythonDictEntryAinfac = InPyBunchSuuceEntryFürKeyString(sictAuswertPythonObjBunch, "htmlstr", ProzesLeeser, ScraibePyObjNaacScpaicer: true, ErmitleTypNurAusScpaicer, DictListeEntryAnzaalScrankeMax);
				if (sictAuswertPythonDictEntryAinfac != null)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(sictAuswertPythonDictEntryAinfac, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
					PyObj.AusDict_SrEntryHtmlstr = sictAuswertPythonDictEntryAinfac.Value;
					if (sictAuswertPythonDictEntryAinfac.Value is SictAuswertPythonObjUnicode sictAuswertPythonObjUnicode5)
					{
						sictAuswertPythonObjUnicode5.LaadeReferenziirte(ProzesLeeser);
						PyObj.AusDict_SrEntryHtmlstrString = sictAuswertPythonObjUnicode5.String;
					}
				}
			}
		}
		SictAuswertPythonDictEntryAinfac dictEntryChildren = sictSuuceMengeDictEntryScpezGbsAstErgeebnis.DictEntryChildren;
		if (dictEntryChildren == null)
		{
			return;
		}
		PyObjDictEntryFüleAusScpaicerOderProzes(dictEntryChildren, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
		if (!(dictEntryChildren.Value is SictAuswertPythonObjPyOderUiChildrenList sictAuswertPythonObjPyOderUiChildrenList))
		{
			return;
		}
		LaadeReferenziirte(sictAuswertPythonObjPyOderUiChildrenList, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
		SictAuswertPythonObjDict dict2 = sictAuswertPythonObjPyOderUiChildrenList.Dict;
		if (dict2 == null)
		{
			return;
		}
		dict2.LaadeReferenziirte(ProzesLeeser, DictListeEntryAnzaalScrankeMax);
		SictAuswertPythonDictEntryAinfac sictAuswertPythonDictEntryAinfac2 = InPyDictSuuceEntryFürKeyString(dict2, "_childrenObjects", ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
		if (sictAuswertPythonDictEntryAinfac2 != null)
		{
			PyObjDictEntryFüleAusScpaicerOderProzes(sictAuswertPythonDictEntryAinfac2, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			if (sictAuswertPythonDictEntryAinfac2.Value is SictAuswertPythonObjList sictAuswertPythonObjList)
			{
				sictAuswertPythonObjList.LaadeReferenziirte(ProzesLeeser);
				PyObj.AusChildrenListRef = sictAuswertPythonObjList.ListeItemRef?.ToArray();
			}
		}
	}

	public void LaadeReferenziirte(SictAuswertPyObjPyColor PyObj, IMemoryReader ProzesLeeser, bool ObjSezeNaacScpaicer = false, bool ErmitleTypNurAusScpaicer = true, int? DictListeEntryAnzaalScrankeMax = 1024)
	{
		if (PyObj == null)
		{
			return;
		}
		LaadeReferenziirte((ISictAuswertPythonObjMitRefDict)PyObj, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
		SictAuswertPythonObjDict dict = PyObj.Dict;
		if (dict == null)
		{
			return;
		}
		string[] array = new string[4] { "_a", "_r", "_g", "_b" };
		KeyValuePair<SictAuswertPythonObj, int?>[] array2 = new KeyValuePair<SictAuswertPythonObj, int?>[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string key = array[i];
			SictAuswertPythonObj sictAuswertPythonObj = null;
			int? value = null;
			SictAuswertPythonDictEntryAinfac sictAuswertPythonDictEntryAinfac = InPyDictSuuceEntryFürKeyString(dict, key, ProzesLeeser, ScraibePyObjNaacScpaicer: true, ErmitleTypNurAusScpaicer, DictListeEntryAnzaalScrankeMax);
			if (sictAuswertPythonDictEntryAinfac != null)
			{
				PyObjDictEntryFüleAusScpaicerOderProzes(sictAuswertPythonDictEntryAinfac, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
				sictAuswertPythonObj = sictAuswertPythonDictEntryAinfac.Value;
			}
			if (sictAuswertPythonObj is SictAuswertPythonObjInt { Int: var @int })
			{
				value = @int * 1000;
			}
			if (sictAuswertPythonObj is SictAuswertPythonObjFloat { Float: var @float } && @float.HasValue)
			{
				value = (int)(@float.Value * 1000.0);
			}
			array2[i] = new KeyValuePair<SictAuswertPythonObj, int?>(sictAuswertPythonObj, value);
		}
		PyObj.AusDictA = array2[0].Key;
		PyObj.AusDictWertAMilli = array2[0].Value;
		PyObj.AusDictR = array2[1].Key;
		PyObj.AusDictWertRMilli = array2[1].Value;
		PyObj.AusDictG = array2[2].Key;
		PyObj.AusDictWertGMilli = array2[2].Value;
		PyObj.AusDictB = array2[3].Key;
		PyObj.AusDictWertBMilli = array2[3].Value;
	}

	public void MengePyObjSezeNaacScpaicer(IEnumerable<SictAuswertPythonObj> MengePyObj)
	{
		if (MengePyObj == null)
		{
			return;
		}
		foreach (SictAuswertPythonObj item in MengePyObj)
		{
			if (item != null)
			{
				MengeFürHerkunftAdrPyObj[item.HerkunftAdrese] = item;
			}
		}
	}

	public void PyObjSezeNaacScpaicer(SictAuswertPythonObj PyObj)
	{
		MengePyObjSezeNaacScpaicer(new SictAuswertPythonObj[1] { PyObj });
	}
}
