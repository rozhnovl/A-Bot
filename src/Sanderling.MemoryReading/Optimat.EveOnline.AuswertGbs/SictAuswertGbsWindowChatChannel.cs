using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using Bib3.Geometrik;
using BotEngine;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowChatChannel : SictAuswertGbsWindow
{
	public SictAuswertGbsListViewport<IListEntry> ViewportMessageAuswert { get; private set; }

	public SictAuswertGbsListViewport<IChatParticipantEntry> ViewportParticipantAuswert { get; private set; }

	public WindowChatChannel ErgeebnisScpezChatChannel { get; private set; }

	public new static WindowChatChannel BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowChatChannel sictAuswertGbsWindowChatChannel = new SictAuswertGbsWindowChatChannel(windowAst);
		sictAuswertGbsWindowChatChannel.Berecne();
		return sictAuswertGbsWindowChatChannel.ErgeebnisScpezChatChannel;
	}

	public SictAuswertGbsWindowChatChannel(UINodeInfoInTree windowStackAst)
		: base(windowStackAst)
	{
	}

	public static ChatParticipantEntry ListEntryParticipantConstruct(UINodeInfoInTree ast, IColumnHeader[] listHeader, RectInt? regionConstraint)
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		IListEntry val = SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard(ast, listHeader, regionConstraint);
		if (val == null)
		{
			return null;
		}
		Sprite statusIcon = ast.MengeChildAstTransitiiveHüle()?.OfType<UINodeInfoInTree>()?.FirstOrDefault((UINodeInfoInTree k) => k.PyObjTypNameIsSprite() && (k.PyObjTypName?.ToLower().Contains("status") ?? false))?.AlsSprite();
		Sprite[] flagIcon = (ast?.MengeChildAstTransitiiveHüle()?.OfType<UINodeInfoInTree>()?.Where((UINodeInfoInTree k) => k?.PyObjTypNameMatchesRegexPatternIgnoreCase("FlagIconWithState") ?? false)?.ToArray())?.Select(delegate(UINodeInfoInTree flagNode)
		{
			Sprite val2 = flagNode.AlsSprite();
			Sprite val3 = flagNode?.MengeChildAstTransitiiveHüle()?.OfType<UINodeInfoInTree>()?.Where((UINodeInfoInTree k) => k.PyObjTypNameIsSprite())?.Select((UINodeInfoInTree k) => k?.AlsSprite())?.WhereNotDefault()?.FirstOrDefault();
			if (val2 != null)
			{
				val2.Texture0Id = val2.Texture0Id ?? ((val3 != null) ? val3.Texture0Id : null);
				val2.TexturePath = val2.TexturePath ?? ((val3 != null) ? val3.TexturePath : null);
				val2.Color = val2.Color ?? ((val3 != null) ? val3.Color : null);
			}
			return val2;
		})?.WhereNotDefault()?.ToArrayIfNotEmpty();
		return new ChatParticipantEntry(val)
		{
			NameLabel = ast.LargestLabelInSubtree().AsUIElementTextIfTextNotEmpty(),
			StatusIcon = (ISprite)(object)statusIcon,
			FlagIcon = (IEnumerable<ISprite>)(object)flagIcon
		};
	}

	public override void Berecne()
	{
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Expected O, but got Unknown
		base.Berecne();
		UINodeInfoInTree astMainContainer = base.AstMainContainer;
		if (astMainContainer != null && true == astMainContainer.VisibleIncludingInheritance)
		{
			Vektor2DInt? mainContainerCenter = astMainContainer.AsUIElementIfVisible().RegionCenter();
			List<(UINodeInfoInTree, Vektor2DInt?)> list = astMainContainer.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree node) => node.PyObjTypNameIsScroll())?.Select((UINodeInfoInTree node) => (node: node, regionCenter: node.AsUIElementIfVisible().RegionCenter()))?.ToList();
			UINodeInfoInTree ViewportSetMessageAst = list?.FirstOrDefault<(UINodeInfoInTree, Vektor2DInt?)>(((UINodeInfoInTree node, Vektor2DInt? regionCenter) nodeWithCenter) => nodeWithCenter.regionCenter?.A < mainContainerCenter?.A).Item1;
			UINodeInfoInTree ViewportSetParticipantAst = list?.FirstOrDefault<(UINodeInfoInTree, Vektor2DInt?)>(((UINodeInfoInTree node, Vektor2DInt? regionCenter) nodeWithCenter) => mainContainerCenter?.A < nodeWithCenter.regionCenter?.A).Item1;
			Func<IObjectIdInMemory, bool> FunkIsOther = delegate(IObjectIdInMemory obj)
			{
				UINodeInfoInTree uINodeInfoInTree = ViewportSetMessageAst;
				return (uINodeInfoInTree == null || !uINodeInfoInTree.EnthaltAstMitHerkunftAdrese(((IObjectIdInt64)obj).Id)) && !(ViewportSetParticipantAst?.EnthaltAstMitHerkunftAdrese(((IObjectIdInt64)obj).Id) ?? false);
			};
			IUIElementText[] array = astMainContainer.ExtraktMengeLabelString()?.Where((IUIElementText label) => FunkIsOther((IObjectIdInMemory)(object)label))?.OrdnungLabel<IUIElementText>()?.ToArray();
			if (ViewportSetMessageAst != null)
			{
				ViewportMessageAuswert = new SictAuswertGbsListViewport<IListEntry>(ViewportSetMessageAst, SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);
				ViewportMessageAuswert.Read();
			}
			if (ViewportSetParticipantAst != null)
			{
				ViewportParticipantAuswert = new SictAuswertGbsListViewport<IChatParticipantEntry>(ViewportSetParticipantAst, ListEntryParticipantConstruct);
				ViewportParticipantAuswert.Read();
			}
			UINodeInfoInTree uINodeInfoInTree2 = astMainContainer?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree node) => node?.PyObjTypNameMatchesRegexPattern("EditPlainText") ?? false)?.OrderByDescending((UINodeInfoInTree node) => node.Grööse?.BetraagQuadriirt ?? (-1.0))?.FirstOrDefault();
			ErgeebnisScpezChatChannel = new WindowChatChannel((IWindow)(object)base.Ergeebnis)
			{
				MessageView = (IListViewAndControl)(object)ViewportMessageAuswert?.Result,
				ParticipantView = (IListViewAndControl<IChatParticipantEntry>)(object)ViewportParticipantAuswert?.Result,
				MessageInput = uINodeInfoInTree2?.AsUIElementInputText()
			};
		}
	}
}
