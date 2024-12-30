using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObjGbsAst : SictAuswertPythonObjMitRefDictBaiPlus8
{
	public const int AusDictRenderObjectRefVersazNaacRenderObjectBlok = -8;

	public long? AusDictParentRef;

	public long[] AusChildrenListRef;

	public long? AusDictRenderObjectRef;

	public SictAuswertPythonObjMitRefBaiPlus8 AusDictRenderObject;

	public SictAuswertPythonObj AusDictName;

	public string AusDictNameString;

	public SictAuswertPythonObj AusDictText;

	public string AusDictTextString;

	public SictAuswertPythonObj AusDictLinkText;

	public string AusDictLinkTextString;

	public SictAuswertPythonObj AusDictSetText;

	public string AusDictSetTextString;

	public SictAuswertPythonObj AusDictCaption;

	public string AusDictCaptionString;

	public SictAuswertPythonObj AusDictWindowID;

	public string AusDictWindowIDString;

	public SictAuswertPythonObj AusDictMinimized;

	public bool? AusDictMinimizedBool;

	public int? AusDictMinimizedInt;

	public SictAuswertPythonObj AusDictIsModal;

	public bool? AusDictIsModalBool;

	public SictAuswertPythonObj AusDictLastState;

	public double? AusDictLastStateFloat;

	public SictAuswertPythonObj AusDictRotation;

	public double? AusDictRotationFloat;

	public SictAuswertPythonObj AusDict_Sr;

	public SictAuswertPythonObj AusDict_SrEntryHtmlstr;

	public SictAuswertPythonObj AusDict_SrEntryNode;

	public string AusDict_SrEntryHtmlstrString;

	public string AusDict_SrEntryNodeGlyphStringText;

	public SictAuswertPythonObj AusDictColor;

	public SictAuswertPythonObj AusDictTexture;

	public long? AusDictTextureMemBlockPlus80Ref;

	public SictAuswertPyObjGbsAst(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 16);
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
	}
}
