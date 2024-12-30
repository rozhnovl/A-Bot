using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Bib3.Geometrik;
using BotEngine;
using Sanderling.Interface.MemoryStruct;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs;

public static class Glob
{
	public static Regex RegexGbsAstPyObjTypNameContainer = new Regex("Container", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	public static Regex RegexGbsAstPyObjTypNameIcon = new Regex("Icon", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	public static Regex RegexGbsAstPyObjTypNameButton = new Regex("Button|StationServiceBtn", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static Regex PyTypeNameSpriteRegex = "sprite".AlsRegexIgnoreCaseCompiled();

	public static KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>[] MengeZuFunktioonWindowAuswertMengeStringWindowTypFilter = new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>[20]
	{
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsMessageBox.BerecneFürWindowAst, new string[1] { "MessageBox" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsHybridWindow.BerecneFürWindowAst, new string[1] { "HybridWindow" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowOverview.BerecneFürWindowAst, new string[1] { "(?<!Sov.*)(?<!Imp.*)OverView(?!.*Set)" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowSelectedItem.BerecneFürWindowAst, new string[2] { "selecteditemview", "ActiveItem" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowInventoryPrimary.BerecneFürWindowAst, new string[4] { "Inventory", "ShipCargo", "StationItem", "StationShip" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowStation.BerecneFürWindowAst, new string[1] { "Lobby" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowAgentDialogue.BerecneFürWindowAst, new string[1] { "AgentDialogueWindow" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowStack.BerecneFürWindowAst, new string[1] { "WindowStack" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowDroneView.BerecneFürWindowAst, new string[1] { "DroneView" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowPeopleAndPlaces.BerecneFürWindowAst, new string[1] { "AddressBookWindow" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowSurveyScanView.BerecneFürWindowAst, new string[1] { "SurveyScanView" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowShipFitting.BerecneFürWindowAst, new string[1] { "FittingWindow" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowFittingMgmt.BerecneFürWindowAst, new string[1] { "FittingMgmt" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowTelecom.BerecneFürWindowAst, new string[1] { "Telecom" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowStack.BerecneFürWindowAst, new string[1] { "LSCStack" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowChatChannel.BerecneFürWindowAst, new string[2] { "Channel", "ChatWindow" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowRegionalMarket.BerecneFürWindowAst, new string[1] { "RegionalMarket" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowMarketAction.BerecneFürWindowAst, new string[1] { "MarketActionWindow" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowItemSell.BerecneFürWindowAst, new string[1] { "SellItems" }),
		new KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>(SictAuswertGbsWindowProbeScanner.BerecneFürWindowAst, new string[1] { "ProbeScannerWindow" })
	};

	public static bool PyObjTypNameMatchesRegex(this GbsAstInfo node, Regex regex)
	{
		string text = node?.PyObjTypName;
		if (text == null)
		{
			return false;
		}
		return regex?.Match(text)?.Success ?? false;
	}

	public static bool PyObjTypNameMatchesRegexPattern(this GbsAstInfo uiNode, string regexPattern, RegexOptions regexOptions = RegexOptions.None)
	{
		return uiNode.PyObjTypNameMatchesRegex(regexPattern.AlsRegex(regexOptions));
	}

	public static bool PyObjTypNameMatchesRegexPatternIgnoreCase(this GbsAstInfo uiNode, string regexPattern, RegexOptions regexOptions = RegexOptions.None)
	{
		return uiNode.PyObjTypNameMatchesRegexPattern(regexPattern, RegexOptions.IgnoreCase | regexOptions);
	}

	public static bool PyObjTypNameIsContainer(this GbsAstInfo uiNode)
	{
		return uiNode.PyObjTypNameMatchesRegex(RegexGbsAstPyObjTypNameContainer);
	}

	public static bool PyObjTypNameIsIcon(this GbsAstInfo uiNode)
	{
		return uiNode.PyObjTypNameMatchesRegex(RegexGbsAstPyObjTypNameIcon);
	}

	public static bool PyObjTypNameIsButton(this GbsAstInfo uiNode)
	{
		return uiNode.PyObjTypNameMatchesRegex(RegexGbsAstPyObjTypNameButton);
	}

	public static bool PyObjTypNameIsSprite(this GbsAstInfo uiNode)
	{
		return uiNode.PyObjTypNameMatchesRegex(PyTypeNameSpriteRegex);
	}

	public static bool PyObjTypNameIsScroll(this UINodeInfoInTree uiNode)
	{
		return string.Equals("BasicDynamicScroll".ToLowerInvariant(), uiNode?.PyObjTypName?.ToLowerInvariant()) || string.Equals("Scroll".ToLowerInvariant(), uiNode?.PyObjTypName?.ToLowerInvariant());
	}

	public static bool PyObjTypNameEqualsIgnoreCase(this UINodeInfoInTree uiNode, string typeName)
	{
		return string.Equals(typeName, uiNode?.PyObjTypName, StringComparison.InvariantCultureIgnoreCase);
	}

	public static bool NameEqualsIgnoreCase(this UINodeInfoInTree uiNode, string name)
	{
		return string.Equals(name, uiNode?.Name, StringComparison.InvariantCultureIgnoreCase);
	}

	public static bool NameMatchesRegex(this UINodeInfoInTree uiNode, Regex regex)
	{
		return regex.Match(uiNode?.Name ?? "").Success;
	}

	public static bool NameMatchesRegexPattern(this UINodeInfoInTree uiNode, string regexPattern, RegexOptions regexOptions = RegexOptions.None)
	{
		return uiNode.NameMatchesRegex(new Regex(regexPattern, regexOptions));
	}

	public static bool NameMatchesRegexPatternIgnoreCase(this UINodeInfoInTree uiNode, string regexPattern, RegexOptions regexOptions = RegexOptions.None)
	{
		return uiNode.NameMatchesRegexPattern(regexPattern, RegexOptions.IgnoreCase | regexOptions);
	}

	public static bool GbsAstTypeIstEveIcon(UINodeInfoInTree uiNode)
	{
		return string.Equals("icon", uiNode?.PyObjTypName, StringComparison.InvariantCultureIgnoreCase);
	}

	public static bool GbsAstTypeIstSprite(UINodeInfoInTree uiNode)
	{
		string text = uiNode?.PyObjTypName;
		if (text.IsNullOrEmpty())
		{
			return false;
		}
		return Regex.Match(text, "Sprite", RegexOptions.IgnoreCase).Success;
	}

	public static bool GbsAstTypeIstLabel(this UINodeInfoInTree node)
	{
		if ((node?.Text ?? node?.SetText ?? node?.LinkText) == null)
		{
			return false;
		}
		string text = node?.PyObjTypName;
		if (text == null)
		{
			return false;
		}
		Match match = Regex.Match(text, "label", RegexOptions.IgnoreCase);
		return match.Success;
	}

	public static bool GbsAstTypeIstEveLabel(this UINodeInfoInTree uiNode)
	{
		return uiNode?.PyObjTypName?.StartsWith("EveLabel", StringComparison.InvariantCultureIgnoreCase) ?? false;
	}

	public static bool GbsAstTypeIstEveCaption(UINodeInfoInTree gbsAst)
	{
		return gbsAst?.PyObjTypName?.StartsWith("EveCaption", StringComparison.InvariantCultureIgnoreCase) ?? false;
	}

	public static ObjectIdInt64 VonGbsAstObjektMitBezaicnerInt(UINodeInfoInTree gbsAst)
	{
		return new ObjectIdInt64(gbsAst?.PyObjAddress ?? (-1));
	}

	public static RectInt? FläceAusGbsAstInfoMitVonParentErbe(UINodeInfoInTree gbsAstInfo)
	{
		if (gbsAstInfo == null)
		{
			return null;
		}
		Vektor2DSingle? grööse = gbsAstInfo.Grööse;
		Vektor2DSingle? vektor2DSingle = gbsAstInfo.LaagePlusVonParentErbeLaage() + grööse * 0.5;
		if (!grööse.HasValue || !vektor2DSingle.HasValue)
		{
			return null;
		}
		return RectInt.FromCenterAndSize(vektor2DSingle.Value.AlsVektor2DInt(), grööse.Value.AlsVektor2DInt());
	}

	public static Window WindowBerecneScpezTypFürGbsAst(UINodeInfoInTree kandidaatWindowNode)
	{
		string KandidaatPyObjTypName = kandidaatWindowNode?.PyObjTypName;
		if (KandidaatPyObjTypName == null)
		{
			return null;
		}
		KeyValuePair<Func<UINodeInfoInTree, Window>, string[]>[] mengeZuFunktioonWindowAuswertMengeStringWindowTypFilter = MengeZuFunktioonWindowAuswertMengeStringWindowTypFilter;
		for (int i = 0; i < mengeZuFunktioonWindowAuswertMengeStringWindowTypFilter.Length; i++)
		{
			KeyValuePair<Func<UINodeInfoInTree, Window>, string[]> keyValuePair = mengeZuFunktioonWindowAuswertMengeStringWindowTypFilter[i];
			string[] value = keyValuePair.Value;
			if (value != null && value.Any((string stringWindowTypFilter) => Regex.Match(KandidaatPyObjTypName, stringWindowTypFilter, RegexOptions.IgnoreCase).Success))
			{
				return keyValuePair.Key?.Invoke(kandidaatWindowNode);
			}
		}
		return SictAuswertGbsWindow.BerecneFürWindowAst(kandidaatWindowNode);
	}
}
