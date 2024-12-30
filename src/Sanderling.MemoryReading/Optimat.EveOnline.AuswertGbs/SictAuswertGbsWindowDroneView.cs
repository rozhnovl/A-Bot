using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3.Geometrik;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowDroneView : SictAuswertGbsWindow
{
	private UINodeInfoInTree ListViewportAst;

	private SictAuswertGbsListViewport<IListEntry> ListViewportAuswert;

	private static readonly string DroneEntryGaugeScpezAstNameRegexPattern = "gauge_(\\w+)";

	public WindowDroneView ErgeebnisScpez { get; private set; }

	public new static WindowDroneView BerecneFürWindowAst(UINodeInfoInTree windowNode)
	{
		if (windowNode == null)
		{
			return null;
		}
		SictAuswertGbsWindowDroneView sictAuswertGbsWindowDroneView = new SictAuswertGbsWindowDroneView(windowNode);
		sictAuswertGbsWindowDroneView.Berecne();
		return sictAuswertGbsWindowDroneView.ErgeebnisScpez;
	}

	public SictAuswertGbsWindowDroneView(UINodeInfoInTree windowNode)
		: base(windowNode)
	{
	}

	private static int? AusDroneEntryGaugeTreferpunkteRelMili(UINodeInfoInTree droneEntryGaugeAst)
	{
		UINodeInfoInTree[] array = droneEntryGaugeAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => true == kandidaat.VisibleIncludingInheritance && "Fill".EqualsIgnoreCase(kandidaat.PyObjTypName), null, 1, 1, omitNodesBelowNodesMatchingPredicate: true);
		if (array == null)
		{
			return null;
		}
		UINodeInfoInTree uINodeInfoInTree = array?.Where((UINodeInfoInTree kandidaat) => "droneGaugeBar".EqualsIgnoreCase(kandidaat.Name))?.FirstOrDefault();
		UINodeInfoInTree uINodeInfoInTree2 = array?.Where((UINodeInfoInTree kandidaat) => "droneGaugeBarDmg".EqualsIgnoreCase(kandidaat.Name))?.FirstOrDefault();
		if (uINodeInfoInTree == null || uINodeInfoInTree2 == null)
		{
			return null;
		}
		Vektor2DSingle? grööse = uINodeInfoInTree2.Grööse;
		Vektor2DSingle? grööse2 = uINodeInfoInTree.Grööse;
		if (!grööse.HasValue || !grööse2.HasValue)
		{
			return null;
		}
		int num = (int)grööse2.Value.A;
		int num2 = (int)grööse.Value.A;
		int num3 = num + num2;
		if (num3 < 1)
		{
			return null;
		}
		return num * 1000 / num3;
	}

	public static DroneViewEntry DroneEntryKonstrukt(UINodeInfoInTree entryAst, IColumnHeader[] listeScrollHeader, RectInt? regionConstraint)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Expected O, but got Unknown
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Expected O, but got Unknown
		if ((!(entryAst?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		SictAuswertGbsListEntry sictAuswertGbsListEntry = new SictAuswertGbsListEntry(entryAst, listeScrollHeader, regionConstraint, ListEntryTrenungZeleTypEnum.Ast);
		sictAuswertGbsListEntry.Berecne();
		IListEntry ergeebnisListEntry = sictAuswertGbsListEntry.ErgeebnisListEntry;
		if (ergeebnisListEntry == null)
		{
			return null;
		}
		UINodeInfoInTree gbsAst = entryAst?.LargestLabelInSubtree();
		IUIElementText val = gbsAst.AsUIElementTextIfTextNotEmpty();
		if (((ergeebnisListEntry != null) ? ergeebnisListEntry.IsGroup : null) ?? false)
		{
			IUIElementText val2 = val;
			return (DroneViewEntry)new DroneViewEntryGroup(ergeebnisListEntry)
			{
				Caption = val
			};
		}
		UINodeInfoInTree[] suuceMengeWurzel = entryAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer(), null, 3, 1);
		UINodeInfoInTree rootNode = suuceMengeWurzel.SuuceFlacMengeAstFrüheste((UINodeInfoInTree kandidaat) => string.Equals("gauges", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 0);
		UINodeInfoInTree[] array = rootNode.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer(), null, 1, 1, omitNodesBelowNodesMatchingPredicate: true);
		Dictionary<string, int?> dictionary = new Dictionary<string, int?>();
		if (array != null)
		{
			UINodeInfoInTree[] array2 = array;
			foreach (UINodeInfoInTree uINodeInfoInTree in array2)
			{
				if (uINodeInfoInTree == null)
				{
					continue;
				}
				string name = uINodeInfoInTree.Name;
				if (name != null)
				{
					string text = (name?.RegexMatchIfSuccess(DroneEntryGaugeScpezAstNameRegexPattern, RegexOptions.IgnoreCase))?.Groups[1].Value;
					if (text != null)
					{
						dictionary[text] = AusDroneEntryGaugeTreferpunkteRelMili(uINodeInfoInTree);
					}
				}
			}
		}
		KeyValuePair<string, int?> keyValuePair = dictionary.FirstOrDefault((KeyValuePair<string, int?> kandidaat) => kandidaat.Key.ToLower().Contains("struct"));
		KeyValuePair<string, int?> keyValuePair2 = dictionary.FirstOrDefault((KeyValuePair<string, int?> kandidaat) => kandidaat.Key.ToLower().Contains("armor"));
		KeyValuePair<string, int?> keyValuePair3 = dictionary.FirstOrDefault((KeyValuePair<string, int?> kandidaat) => kandidaat.Key.ToLower().Contains("shield"));
		ShipHitpointsAndEnergy hitpoints = new ShipHitpointsAndEnergy
		{
			Struct = keyValuePair.Value,
			Armor = keyValuePair2.Value,
			Shield = keyValuePair3.Value
		};
		return (DroneViewEntry)new DroneViewEntryItem(ergeebnisListEntry)
		{
			Hitpoints = (IShipHitpointsAndEnergy)(object)hitpoints
		};
	}

	public override void Berecne()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		base.Berecne();
		if (base.Ergeebnis != null)
		{
			ListViewportAst = base.AstMainContainerMain?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat?.PyObjTypNameIsScroll() ?? false)?.LargestNodeInSubtree();
			ListViewportAuswert = new SictAuswertGbsListViewport<IListEntry>(ListViewportAst, DroneEntryKonstrukt);
			ListViewportAuswert.Read();
			ErgeebnisScpez = new WindowDroneView((IWindow)(object)base.Ergeebnis)
			{
				ListView = (IListViewAndControl)(object)ListViewportAuswert?.Result
			};
		}
	}
}
