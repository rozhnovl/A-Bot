using System;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsShipModuleButtonRamps
{
	public readonly UINodeInfoInTree shipModuleButtonRampsNode;

	public UINodeInfoInTree LeftRampAst { get; private set; }

	public UINodeInfoInTree RightRampAst { get; private set; }

	public double? LeftRampRotation { get; private set; }

	public double? RightRampRotation { get; private set; }

	public bool RampAktiiv { get; private set; }

	public int? RotatioonMili { get; private set; }

	public SictAuswertGbsShipModuleButtonRamps(UINodeInfoInTree shipModuleButtonRampsNode)
	{
		this.shipModuleButtonRampsNode = shipModuleButtonRampsNode;
	}

	public static bool RampRotatioonInGültigeBeraic(double rampRotation, out bool inRegioonAnim)
	{
		inRegioonAnim = false;
		if (!(0.0 <= rampRotation) || !(rampRotation <= 3.151592653589793))
		{
			return false;
		}
		inRegioonAnim = 0.01 < rampRotation && rampRotation < 3.1315926535897933;
		return true;
	}

	public static int? RotatioonMiliAusLeftRampUndRightRamp(double leftRampRotation, double rightRampRotation, out bool rampAktiiv)
	{
		rampAktiiv = false;
		if (!RampRotatioonInGültigeBeraic(leftRampRotation, out var inRegioonAnim))
		{
			return null;
		}
		if (!RampRotatioonInGültigeBeraic(rightRampRotation, out var inRegioonAnim2))
		{
			return null;
		}
		if (inRegioonAnim && inRegioonAnim2)
		{
			rampAktiiv = true;
			return null;
		}
		int value = ((int)(1000.0 - (leftRampRotation + rightRampRotation) * 500.0 / Math.PI) % 1000 + 1000) % 1000;
		rampAktiiv = 0 < Math.Abs(value);
		return value;
	}

	public void Berecne()
	{
		UINodeInfoInTree uINodeInfoInTree = shipModuleButtonRampsNode;
		if (!(uINodeInfoInTree?.VisibleIncludingInheritance ?? false))
		{
			return;
		}
		UINodeInfoInTree[] array = uINodeInfoInTree?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Transform", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 4, 1, omitNodesBelowNodesMatchingPredicate: true);
		LeftRampAst = array?.SuuceFlacMengeAstFrüheste((UINodeInfoInTree kandidaat) => string.Equals("leftRamp", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 0);
		RightRampAst = array?.SuuceFlacMengeAstFrüheste((UINodeInfoInTree kandidaat) => string.Equals("rightRamp", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 0);
		if (LeftRampAst != null && RightRampAst != null)
		{
			LeftRampRotation = LeftRampAst.RotationFloat;
			RightRampRotation = RightRampAst.RotationFloat;
			if (0.0 < LeftRampRotation || 0.0 < RightRampRotation)
			{
				RotatioonMili = RotatioonMiliAusLeftRampUndRightRamp(LeftRampRotation ?? 0.0, RightRampRotation ?? 0.0, out var rampAktiiv);
				RampAktiiv = rampAktiiv;
			}
		}
	}
}
