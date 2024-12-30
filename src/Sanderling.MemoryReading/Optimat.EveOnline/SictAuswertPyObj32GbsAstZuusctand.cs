using System.Collections.Generic;
using System.Linq;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32GbsAstZuusctand : SictAuswertPyObj32MitBaiPlus8RefDictZuusctand
{
	public const int AusDictRenderObjectRefVersazNaacRenderObjectBlok = -8;

	public readonly List<SictAuswertPyObj32GbsAstZuusctand> ListeChild = new List<SictAuswertPyObj32GbsAstZuusctand>();

	public readonly GbsAstInfo AstInfo = new GbsAstInfo();

	[SictInPyDictEntryKeyAttribut("_parentRef")]
	public SictAuswertPyObj32Zuusctand DictEntryParentRef;

	[SictInPyDictEntryKeyAttribut("children")]
	public SictAuswertPyObj32Zuusctand DictEntryChildren;

	[SictInPyDictEntryKeyAttribut("renderObject")]
	public SictAuswertPyObj32Zuusctand DictEntryRenderObject;

	[SictInPyDictEntryKeyAttribut("_name")]
	public SictAuswertPyObj32Zuusctand DictEntryName;

	[SictInPyDictEntryKeyAttribut("lastState")]
	public SictAuswertPyObj32Zuusctand DictEntryLastState;

	[SictInPyDictEntryKeyAttribut("_lastValue")]
	public SictAuswertPyObj32Zuusctand DictEntryLastValue;

	[SictInPyDictEntryKeyAttribut("lastSetCapacitor")]
	public SictAuswertPyObj32Zuusctand DictEntryLastSetCapacitor;

	[SictInPyDictEntryKeyAttribut("_rotation")]
	public SictAuswertPyObj32Zuusctand DictEntryRotation;

	[SictInPyDictEntryKeyAttribut("_texture")]
	public SictAuswertPyObj32Zuusctand DictEntryTexture;

	[SictInPyDictEntryKeyAttribut("_color")]
	public SictAuswertPyObj32Zuusctand DictEntryColor;

	[SictInPyDictEntryKeyAttribut("_hint")]
	public SictAuswertPyObj32Zuusctand DictEntryHint;

	[SictInPyDictEntryKeyAttribut("_sr")]
	public SictAuswertPyObj32Zuusctand DictEntry_Sr;

	[SictInPyDictEntryKeyAttribut("_text")]
	public SictAuswertPyObj32Zuusctand DictEntryText;

	[SictInPyDictEntryKeyAttribut("linkText")]
	public SictAuswertPyObj32Zuusctand DictEntryLinkText;

	[SictInPyDictEntryKeyAttribut("_setText")]
	public SictAuswertPyObj32Zuusctand DictEntrySetText;

	[SictInPyDictEntryKeyAttribut("isModal")]
	public SictAuswertPyObj32Zuusctand DictEntryIsModal;

	[SictInPyDictEntryKeyAttribut("_isSelected")]
	public SictAuswertPyObj32Zuusctand DictEntryOverviewEntryIsSelected;

	[SictInPyDictEntryKeyAttribut("isSelected")]
	public SictAuswertPyObj32Zuusctand DictEntryTreeViewEntryIsSelected;

	[SictInPyDictEntryKeyAttribut("texturePath")]
	public SictAuswertPyObj32Zuusctand DictEntryTexturePath;

	[SictInPyDictEntryKeyAttribut("_caption")]
	public SictAuswertPyObj32Zuusctand DictEntryCaption;

	[SictInPyDictEntryKeyAttribut("_minimized")]
	public SictAuswertPyObj32Zuusctand DictEntryMinimized;

	[SictInPyDictEntryKeyAttribut("isDialog")]
	public SictAuswertPyObj32Zuusctand DictEntryIsDialog;

	[SictInPyDictEntryKeyAttribut("windowID")]
	public SictAuswertPyObj32Zuusctand DictEntryWindowId;

	[SictInPyDictEntryKeyAttribut("_pinned")]
	public SictAuswertPyObj32Zuusctand DictEntryPinned;

	[SictInPyDictEntryKeyAttribut("lastSpeed")]
	public SictAuswertPyObj32Zuusctand DictEntrySpeed;

	[SictInPyDictEntryKeyAttribut("lastsetcapacitor")]
	public SictAuswertPyObj32Zuusctand DictEntryCapacitorLevel;

	[SictInPyDictEntryKeyAttribut("lastShield")]
	public SictAuswertPyObj32Zuusctand DictEntryShieldLevel;

	[SictInPyDictEntryKeyAttribut("lastArmor")]
	public SictAuswertPyObj32Zuusctand DictEntryArmorLevel;

	[SictInPyDictEntryKeyAttribut("lastStructure")]
	public SictAuswertPyObj32Zuusctand DictEntryStructureLevel;

	[SictInPyDictEntryKeyAttribut("_backgroundlist")]
	public SictAuswertPyObj32Zuusctand DictEntryBackgroundList;

	public SictAuswertPyObj32GbsAstZuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
		DictListeEntryAnzaalScrankeMax = 4096;
		AstInfo.PyObjAddress = HerkunftAdrese;
	}

	public void ListeChildPropagiireNaacInfoObjekt()
	{
		AstInfo.ListChild = ListeChild?.Select((SictAuswertPyObj32GbsAstZuusctand ChildObj) => ChildObj?.AstInfo)?.Reverse()?.ToArray();
	}
}
