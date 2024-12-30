using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Bib3.Geometrik;
using BotEngine;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class ScrollReader
{
	public const string MainContainerUIElementName = "maincontainer";

	public const string ClipperUIElementName = "__clipper";

	public const string ClipperContentUIElementName = "__content";

	public readonly UINodeInfoInTree ScrollNode;

	public UINodeInfoInTree ClipperContentNode { get; private set; }

	public Scroll Result { get; private set; }

	public ScrollReader(UINodeInfoInTree scrollNode)
	{
		ScrollNode = scrollNode;
	}

	public virtual void Read()
	{
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Expected O, but got Unknown
		if (ScrollNode?.VisibleIncludingInheritance ?? false)
		{
			UINodeInfoInTree uINodeInfoInTree = ScrollNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree candidate) => string.Equals("maincontainer", candidate.Name, StringComparison.InvariantCultureIgnoreCase), 2, 0);
			IColumnHeader[] columnHeader2 = ((ScrollNode?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree candidate) => (candidate.PyObjTypNameIsContainer() && Regex.Match(candidate.Name ?? "", "scrollHeader", RegexOptions.IgnoreCase).Success) || Regex.Match(candidate.PyObjTypName ?? "", "SortHeader", RegexOptions.IgnoreCase).Success || candidate.PyObjTypNameMatchesRegexPatternIgnoreCase("ScrollColumnHeader"), 3, 1))?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree candidate) => Regex.Match(candidate.PyObjTypName ?? "", "ColumnHeader", RegexOptions.IgnoreCase).Success || candidate.PyObjTypNameIsContainer(), null, 2, 1)?.ToArray())?.Select(SictAuswertGbsListColumnHeader.Read)?.Where((IColumnHeader columnHeader) => columnHeader != null)?.Where((IColumnHeader columnHeader) => !((columnHeader != null) ? ((IUIElementText)columnHeader).Text : null).IsNullOrEmpty()).TailmengeUnterste<IColumnHeader>(ScrollNode)?.OrderBy((IColumnHeader columnHeader) => ((IUIElement)columnHeader).Region.Value.Center().A)?.GroupBy((IColumnHeader header) => ((IObjectIdInt64)header).Id)?.Select((IGrouping<long, IColumnHeader> headerGroup) => headerGroup.FirstOrDefault())?.ToArray();
			UINodeInfoInTree uINodeInfoInTree2 = uINodeInfoInTree?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree candidate) => string.Equals("ScrollControls", candidate.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			UINodeInfoInTree uiNode = uINodeInfoInTree2?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree candidate) => string.Equals("ScrollHandle", candidate.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			UINodeInfoInTree uINodeInfoInTree3 = uINodeInfoInTree?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree candidate) => string.Equals("__clipper", candidate.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			ClipperContentNode = uINodeInfoInTree3?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree candidate) => candidate.PyObjTypNameIsContainer() && string.Equals("__content", candidate.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			Result = new Scroll(ScrollNode.AsUIElementIfVisible())
			{
				ColumnHeader = columnHeader2,
				Clipper = uINodeInfoInTree3.AsUIElementIfVisible(),
				ScrollHandleBound = uINodeInfoInTree2.AsUIElementIfVisible(),
				ScrollHandle = uiNode.AsUIElementIfVisible()
			};
		}
	}
}
