using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3.Geometrik;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsShipUiSlotsSlot
{
	private const string overloadOffHintRegexPattern = "Turn\\s*On\\s*Overload";

	private const string overloadOnHintRegexPattern = "Turn\\s*Off\\s*Overload";

	public const string ModuleButtonPyTypeName = "ModuleButton";

	public readonly UINodeInfoInTree slotNode;

	public UINodeInfoInTree ModuleButtonAst { get; private set; }

	public UINodeInfoInTree ModuleButtonIconAst { get; private set; }

	public UINodeInfoInTree ModuleButtonQuantityAst { get; private set; }

	public UINodeInfoInTree ModuleButtonQuantityLabelAst { get; private set; }

	public UINodeInfoInTree AstMainShape { get; private set; }

	public UINodeInfoInTree[] MengeKandidaatRampAst { get; private set; }

	public SictAuswertGbsShipModuleButtonRamps[] MengeKandidaatRampAuswert { get; private set; }

	public UINodeInfoInTree SpriteHiliteAst { get; private set; }

	public UINodeInfoInTree SpriteGlowAst { get; private set; }

	public UINodeInfoInTree SpriteBusyAst { get; private set; }

	public ShipUiModule ModuleRepr { get; private set; }

	public SictAuswertGbsShipUiSlotsSlot(UINodeInfoInTree slotNode)
	{
		this.slotNode = slotNode;
	}

	public void Berecne()
	{
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_0622: Unknown result type (might be due to invalid IL or missing references)
		//IL_062a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0632: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Expected O, but got Unknown
		if (!(slotNode?.VisibleIncludingInheritance ?? false))
		{
			return;
		}
		ModuleButtonAst = slotNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("ModuleButton", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 1, 1);
		ModuleButtonIconAst = ModuleButtonAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Icon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) || string.Equals("EveIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 1, 1);
		ModuleButtonQuantityAst = ModuleButtonAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("quantityParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 1);
		ModuleButtonQuantityLabelAst = ModuleButtonQuantityAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Label", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 1, 1);
		UINodeInfoInTree[] array = slotNode?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => true == kandidaat.VisibleIncludingInheritance && string.Equals("Sprite", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 1, 1);
		SpriteHiliteAst = array?.FirstOrDefault((UINodeInfoInTree kandidaat) => string.Equals("hilite", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));
		SpriteGlowAst = array?.FirstOrDefault((UINodeInfoInTree kandidaat) => string.Equals("glow", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));
		SpriteBusyAst = array?.FirstOrDefault((UINodeInfoInTree kandidaat) => string.Equals("busy", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));
		MengeKandidaatRampAst = slotNode?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => true == kandidaat.VisibleIncludingInheritance && Regex.Match(kandidaat.PyObjTypName ?? "", "ramps", RegexOptions.IgnoreCase).Success, 1, 1);
		MengeKandidaatRampAuswert = MengeKandidaatRampAst?.Select(delegate(UINodeInfoInTree kandidaatRampAst)
		{
			SictAuswertGbsShipModuleButtonRamps sictAuswertGbsShipModuleButtonRamps = new SictAuswertGbsShipModuleButtonRamps(kandidaatRampAst);
			sictAuswertGbsShipModuleButtonRamps.Berecne();
			return sictAuswertGbsShipModuleButtonRamps;
		}).ToArray();
		SictAuswertGbsShipModuleButtonRamps sictAuswertGbsShipModuleButtonRamps2 = MengeKandidaatRampAuswert?.FirstOrDefault((SictAuswertGbsShipModuleButtonRamps kandidaat) => kandidaat.LeftRampAst != null || kandidaat.RampAktiiv);
		AstMainShape = slotNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("mainshape", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 1);
		if (AstMainShape == null)
		{
			return;
		}
		bool? hiliteVisible = null;
		bool? glowVisible = null;
		bool? busyVisible = null;
		if (SpriteHiliteAst != null)
		{
			hiliteVisible = true == SpriteHiliteAst.VisibleIncludingInheritance;
		}
		if (SpriteGlowAst != null)
		{
			glowVisible = true == SpriteGlowAst.VisibleIncludingInheritance;
		}
		if (SpriteBusyAst != null)
		{
			busyVisible = true == SpriteBusyAst.VisibleIncludingInheritance;
		}
		bool? moduleButtonVisible = ModuleButtonAst?.VisibleIncludingInheritance;
		IUIElement val = ModuleButtonAst.AsUIElementIfVisible().WithRegionSizePivotAtCenter(new Vektor2DInt(16L, 16L));
		long? num = ModuleButtonIconAst?.TextureIdent0;
		bool rampActive = ModuleButtonAst?.RampActive ?? sictAuswertGbsShipModuleButtonRamps2?.RampAktiiv ?? false;
		int? rampRotationMilli = sictAuswertGbsShipModuleButtonRamps2?.RotatioonMili;
		bool? overloadOn = null;
		UINodeInfoInTree uINodeInfoInTree = slotNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree node) => node.PyObjTypNameIsSprite() && (node?.Name?.RegexMatchSuccessIgnoreCase("OverloadB(tn|utton)") ?? false));
		if (uINodeInfoInTree != null)
		{
			if (uINodeInfoInTree?.Hint?.RegexMatchSuccessIgnoreCase("Turn\\s*On\\s*Overload") ?? false)
			{
				overloadOn = false;
			}
			if (uINodeInfoInTree?.Hint?.RegexMatchSuccessIgnoreCase("Turn\\s*Off\\s*Overload") ?? false)
			{
				overloadOn = true;
			}
		}
		ShipUiModule moduleRepr = new ShipUiModule(slotNode.AsUIElementIfVisible())
		{
			ModuleButtonVisible = moduleButtonVisible,
			ModuleButtonIconTexture = (num.HasValue ? num.GetValueOrDefault().AsObjectIdInMemory() : null),
			ModuleButtonQuantity = ModuleButtonQuantityLabelAst?.SetText,
			RampActive = rampActive,
			RampRotationMilli = rampRotationMilli,
			HiliteVisible = hiliteVisible,
			GlowVisible = glowVisible,
			BusyVisible = busyVisible,
			OverloadOn = overloadOn
		};
		ModuleRepr = moduleRepr;
	}
}
