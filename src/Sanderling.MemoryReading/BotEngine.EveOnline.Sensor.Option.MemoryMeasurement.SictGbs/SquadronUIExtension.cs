using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using Optimat.EveOnline.AuswertGbs;
using Sanderling.Interface.MemoryStruct;
using Extension = Sanderling.Interface.MemoryStruct.Extension;

namespace BotEngine.EveOnline.Sensor.Option.MemoryMeasurement.SictGbs;

public static class SquadronUIExtension
{
	public const string FightersHealthGaugePyTypeName = "FightersHealthGauge";

	public const string AbilityIconPyTypeName = "AbilityIcon";

	public static ISquadronsUI AsSquadronsUI(this UINodeInfoInTree squadronsNode)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		if ((!(squadronsNode?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		Container val = squadronsNode?.AlsContainer();
		UINodeInfoInTree[] array = squadronsNode?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree node) => node.PyObjTypNameMatchesRegexPatternIgnoreCase("SquadronUI"));
		Func<string, IUIElement> func = (string pyTypeNameRegexPattern) => squadronsNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree node) => node.PyObjTypNameMatchesRegexPatternIgnoreCase(pyTypeNameRegexPattern))?.AsUIElementIfVisible();
		return (ISquadronsUI)new SquadronsUI((IUIElement)(object)val)
		{
			SetSquadron = array?.Select((UINodeInfoInTree node) => node?.AsSquadronUI())?.WhereNotDefault()?.OrderBy((ISquadronUI squadronUI) => (squadronUI == null) ? null : Extension.RegionCenter((IUIElement)(object)squadronUI)?.A)?.ToArrayIfNotEmpty(),
			LaunchAllButton = func("ButtonLaunchAll"),
			OpenBayButton = func("ButtonOpenBay"),
			RecallAllButton = func("ButtonRecallAll")
		};
	}

	public static ISquadronUI AsSquadronUI(this UINodeInfoInTree squadronUINode)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		if ((!(squadronUINode?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		SquadronUI val = new SquadronUI(squadronUINode?.AsUIElementIfVisible());
		object setAbilityIcon;
		if (squadronUINode == null)
		{
			setAbilityIcon = null;
		}
		else
		{
			UINodeInfoInTree[] array = squadronUINode.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree node) => node.PyObjTypNameMatchesRegexPatternIgnoreCase("AbilityIcon"));
			if (array == null)
			{
				setAbilityIcon = null;
			}
			else
			{
				IEnumerable<ISquadronAbilityIcon> enumerable = array.Select(AsSquadronAbilityIcon);
				if (enumerable == null)
				{
					setAbilityIcon = null;
				}
				else
				{
					IEnumerable<ISquadronAbilityIcon> enumerable2 = enumerable.WhereNotDefault();
					setAbilityIcon = ((enumerable2 == null) ? null : Extension.OrderByCenterVerticalDown<ISquadronAbilityIcon>(enumerable2)?.ToArrayIfNotEmpty());
				}
			}
		}
		val.SetAbilityIcon = (IEnumerable<ISquadronAbilityIcon>)setAbilityIcon;
		val.Squadron = squadronUINode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree node) => (node?.VisibleIncludingInheritance ?? false) && node.PyObjTypNameMatchesRegexPatternIgnoreCase("SquadronCont"))?.AsSquadronContainer();
		return (ISquadronUI)(object)val;
	}

	public static ISquadronAbilityIcon AsSquadronAbilityIcon(this UINodeInfoInTree squadronAbilityIconNode)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		if ((!(squadronAbilityIconNode?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		UINodeInfoInTree uINodeInfoInTree = squadronAbilityIconNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree node) => node.PyObjTypNameIsContainer() && (node.Name?.RegexMatchSuccessIgnoreCase("quantityParent") ?? false))?.LargestLabelInSubtree();
		return (ISquadronAbilityIcon)new SquadronAbilityIcon(Extension.WithRegionSizeBoundedMaxPivotAtCenter(squadronAbilityIconNode.AsUIElementIfVisible(), new Vektor2DInt(26L, 26L)))
		{
			Quantity = uINodeInfoInTree?.LabelText()?.Trim()?.TryParseInt(),
			RampActive = squadronAbilityIconNode?.RampActive
		};
	}

	public static ISquadronContainer AsSquadronContainer(this UINodeInfoInTree squadronContainerNode)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		if ((!(squadronContainerNode?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		UINodeInfoInTree uINodeInfoInTree = squadronContainerNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree node) => node.PyObjTypNameMatchesRegexPatternIgnoreCase("SquadronNumber"))?.LargestLabelInSubtree();
		bool? isSelected = squadronContainerNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree n) => n?.Name?.RegexMatchSuccessIgnoreCase("SelectHilight") ?? false)?.VisibleIncludingInheritance;
		return (ISquadronContainer)new SquadronContainer((IUIElement)(object)squadronContainerNode.AlsContainer())
		{
			SquadronNumber = uINodeInfoInTree?.LabelText()?.TryParseInt(),
			Health = squadronContainerNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree node) => node.PyObjTypNameMatchesRegexPatternIgnoreCase("FightersHealthGauge")).AsSquadronHealth(),
			IsSelected = isSelected,
			Hint = squadronContainerNode?.Hint
		};
	}

	public static ISquadronHealth AsSquadronHealth(this UINodeInfoInTree squadronHealthNode)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		if ((!(squadronHealthNode?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		return (ISquadronHealth)new SquadronHealth
		{
			SquadronSizeCurrent = squadronHealthNode?.SquadronSize,
			SquadronSizeMax = squadronHealthNode?.SquadronMaxSize
		};
	}
}
