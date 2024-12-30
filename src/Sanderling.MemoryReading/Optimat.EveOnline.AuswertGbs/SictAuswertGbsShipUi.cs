using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Bib3.Geometrik;
using BotEngine.EveOnline.Sensor.Option.MemoryMeasurement.SictGbs;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsShipUi
{
	private static Regex ReadoutContainerAstNameRegex = "readout".AlsRegexIgnoreCaseCompiled();

	public readonly UINodeInfoInTree LayerShipUiNode;

	public static string AusShipUiGaugeHintTextTailProzentRegexPattern = "\\:\\s*(\\d+)\\s*\\%";

	public static string AusShipUiGaugeHintTextTailTotalRegexPattern = "(\\d+) left of maximum (\\d+)";

	public UINodeInfoInTree ShipUIContainerAst { get; private set; }

	public UINodeInfoInTree EwarUIContainerAst { get; private set; }

	public UINodeInfoInTree[] EwarUIContainerMengeEWarElementKandidaatAst { get; private set; }

	public SictAuswertGbsShipUiEWarElement[] EwarUIContainerMengeEWarElementKandidaatAuswert { get; private set; }

	public ShipUiEWarElement[] EwarUIContainerMengeEWarElementKandidaatAuswertErgeebnis { get; private set; }

	public UINodeInfoInTree ContainerPowerCoreAst { get; private set; }

	public UINodeInfoInTree[] ContainerPowerCoreMengeMarkAst { get; private set; }

	public int? ContainerPowerCoreMengeMarkAinAnzaal { get; private set; }

	public int? ContainerPowerCoreMengeMarkAusAnzaal { get; private set; }

	public UINodeInfoInTree AstIndicationContainer { get; private set; }

	public SictAuswertGbsShipUiSlots AuswertSlots { get; private set; }

	public UINodeInfoInTree ButtonStopAst { get; private set; }

	public UINodeInfoInTree TimersContainerAst { get; private set; }

	public UINodeInfoInTree[] MengeTimerKandidaatAst { get; private set; }

	public ShipUi Ergeebnis { get; private set; }

	public SictAuswertGbsShipUi(UINodeInfoInTree layerShipUiNode)
	{
		LayerShipUiNode = layerShipUiNode;
	}

	public static IShipUiTimer AlsTimer(UINodeInfoInTree node)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		Container val = node?.AlsContainer();
		if (val == null)
		{
			return null;
		}
		return (IShipUiTimer)new ShipUiTimer((IUIElement)(object)val)
		{
			Name = node?.Name
		};
	}

	public void Berecne()
	{
		//IL_08fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0901: Unknown result type (might be due to invalid IL or missing references)
		//IL_0976: Unknown result type (might be due to invalid IL or missing references)
		//IL_09eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a60: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6b: Expected O, but got Unknown
		//IL_0b4c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b51: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b78: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b81: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b8a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b9f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bbe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c44: Expected O, but got Unknown
		if (!(LayerShipUiNode?.VisibleIncludingInheritance ?? false))
		{
			return;
		}
		ShipUIContainerAst = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("ShipUIContainer", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) || string.Equals("ShipHudContainer", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		EwarUIContainerAst = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EwarUIContainer", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) || string.Equals("EwarContainer", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) || string.Equals("BuffBarContainer", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		TimersContainerAst = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("timers", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		UINodeInfoInTree uINodeInfoInTree = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameEqualsIgnoreCase("CapacitorContainer"));
		MengeTimerKandidaatAst = TimersContainerAst?.ListChild;
		EwarUIContainerMengeEWarElementKandidaatAst = EwarUIContainerAst?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => (kandidaat?.VisibleIncludingInheritance ?? false) && (kandidaat?.PyObjTypNameEqualsIgnoreCase("BuffSlotParent") ?? false), null, 2, 1);
		EwarUIContainerMengeEWarElementKandidaatAuswert = EwarUIContainerMengeEWarElementKandidaatAst?.Select(delegate(UINodeInfoInTree kandidaatAst)
		{
			SictAuswertGbsShipUiEWarElement sictAuswertGbsShipUiEWarElement = new SictAuswertGbsShipUiEWarElement(kandidaatAst);
			sictAuswertGbsShipUiEWarElement.Berecne();
			return sictAuswertGbsShipUiEWarElement;
		}).ToArray();
		EwarUIContainerMengeEWarElementKandidaatAuswertErgeebnis = EwarUIContainerMengeEWarElementKandidaatAuswert?.Select((SictAuswertGbsShipUiEWarElement auswert) => auswert.Ergeebnis).WhereNotDefault().ToArray();
		ContainerPowerCoreAst = ShipUIContainerAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("powercore", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		string text = ContainerPowerCoreAst?.Hint;
		ContainerPowerCoreMengeMarkAst = ContainerPowerCoreAst?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Sprite", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("pmark", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), null, 2, 1);
		if (ContainerPowerCoreMengeMarkAst != null)
		{
			ContainerPowerCoreMengeMarkAinAnzaal = ContainerPowerCoreMengeMarkAst?.Count((UINodeInfoInTree node) => 700 < node?.Color.Value.OMilli);
		}
		UINodeInfoInTree uINodeInfoInTree2 = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("gaugeReadout", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));
		UINodeInfoInTree uINodeInfoInTree3 = (LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("underMain", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase)))?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Transform", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && Regex.Match(kandidaat.Name ?? "", "speedNeedle", RegexOptions.IgnoreCase).Success);
		UINodeInfoInTree uINodeInfoInTree4 = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => string.Equals("HPGauges", k?.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)) ?? LayerShipUiNode;
		UINodeInfoInTree uINodeInfoInTree5 = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => string.Equals("SpeedGauge", k?.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)) ?? LayerShipUiNode;
		ButtonStopAst = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k?.NameEqualsIgnoreCase("stopButton") ?? false) ?? LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k?.PyObjTypNameEqualsIgnoreCase("StopButton") ?? false);
		UINodeInfoInTree gbsAst = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k != null && k.PyObjTypNameIsContainer() && (k?.NameMatchesRegex(ReadoutContainerAstNameRegex) ?? false));
		IUIElementText[] readout = gbsAst.ExtraktMengeLabelString()?.OrdnungLabel<IUIElementText>()?.ToArray();
		UINodeInfoInTree uINodeInfoInTree6 = uINodeInfoInTree5?.LargestLabelInSubtree();
		UINodeInfoInTree uINodeInfoInTree7 = uINodeInfoInTree4?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("structureGauge", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));
		UINodeInfoInTree uINodeInfoInTree8 = uINodeInfoInTree4?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("armorGauge", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));
		UINodeInfoInTree uINodeInfoInTree9 = uINodeInfoInTree4?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("shieldGauge", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));
		string text2 = uINodeInfoInTree7?.Hint;
		string text3 = uINodeInfoInTree8?.Hint;
		string text4 = uINodeInfoInTree9?.Hint;
		UINodeInfoInTree shipUiSlotsNode = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("slotsContainer", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 4) ?? LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameEqualsIgnoreCase("SlotsContainer"));
		AstIndicationContainer = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("indicationContainer", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 4, 1);
		Container indication = AstIndicationContainer?.AlsContainer();
		AuswertSlots = new SictAuswertGbsShipUiSlots(shipUiSlotsNode);
		AuswertSlots.Berecne();
		ShipUiModule[] listModuleButton = AuswertSlots.ListModuleButton;
		ShipHitpointsAndEnergy val = null;
		int? num = ContainerPowerCoreMengeMarkAst?.Count();
		int? num2 = null;
		if (0 < num)
		{
			num2 = ContainerPowerCoreMengeMarkAinAnzaal * 1000 / num;
		}
		num2 = num2 ?? ((int?)((uINodeInfoInTree == null) ? null : (uINodeInfoInTree.LastSetCapacitorFloat * 1000.0)));
		val = new ShipHitpointsAndEnergy
		{
			Struct = (int?)((((double?)LayerShipUiNode.StructureLevel) ?? uINodeInfoInTree7?.LastValueFloat) * 1000.0),
			Armor = (int?)((((double?)LayerShipUiNode.ArmorLevel) ?? uINodeInfoInTree8?.LastValueFloat) * 1000.0),
			Shield = (int?)((((double?)LayerShipUiNode.ShieldLevel) ?? uINodeInfoInTree9?.LastValueFloat) * 1000.0),
			Capacitor = num2
		};
		IUIElement buttonSpeed = ButtonStopAst?.AsUIElementIfVisible();
		IUIElement buttonSpeedMax = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k?.PyObjTypNameEqualsIgnoreCase("MaxSpeedButton") ?? false).AsUIElementIfVisible();
		IShipUiTimer[] timer = MengeTimerKandidaatAst?.Select(AlsTimer)?.OrdnungLabel<IShipUiTimer>()?.ToArray();
		UINodeInfoInTree uINodeInfoInTree10 = LayerShipUiNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree node) => node.PyObjTypNameMatchesRegexPatternIgnoreCase("SquadronsUI"));
		Ergeebnis = new ShipUi((IUIElement)null)
		{
			Center = Sanderling.Interface.MemoryStruct.Extension.WithRegionSizePivotAtCenter((ContainerPowerCoreAst ?? uINodeInfoInTree).AsUIElementIfVisible(), new Vektor2DInt(40L, 40L)),
			Indication = (IContainer)(object)indication,
			HitpointsAndEnergy = (IShipHitpointsAndEnergy)(object)val,
			SpeedLabel = uINodeInfoInTree6?.AsUIElementTextIfTextNotEmpty(),
			EWarElement = EwarUIContainerMengeEWarElementKandidaatAuswertErgeebnis,
			Timer = timer,
			ButtonSpeed0 = buttonSpeed,
			ButtonSpeedMax = buttonSpeedMax,
			Module = (IShipUiModule[])(object)listModuleButton,
			SpeedMilli = (long?)((double?)LayerShipUiNode?.Speed * 1000.0),
			Readout = readout,
			SquadronsUI = uINodeInfoInTree10?.AsSquadronsUI()
		};
	}
}
