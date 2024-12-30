using System;
using System.Linq;
using Bib3;
using Bib3.Geometrik;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsListViewport<EntryT> where EntryT : class, IListEntry
{
	public readonly IAusGbsAstExtraktor EntryExtraktor;

	public readonly UINodeInfoInTree ListViewNode;

	public readonly Func<UINodeInfoInTree, IColumnHeader[], RectInt?, EntryT> CallbackListEntryConstruct;

	public readonly ListEntryTrenungZeleTypEnum? InEntryTrenungZeleTyp;

	public UINodeInfoInTree ScrollHeadersAst { get; private set; }

	public UINodeInfoInTree ScrollClipperAst { get; private set; }

	public UINodeInfoInTree ScrollClipperContentNode { get; private set; }

	public SictAuswertGbsListEntry[] ScrollClipperContentMengeKandidaatEntryAuswert { get; private set; }

	public UINodeInfoInTree[] ListeColumnHeaderAst { get; private set; }

	public SictAuswertGbsListColumnHeader[] ListeColumnHeaderAuswert { get; private set; }

	public IColumnHeader[] ListeHeader { get; private set; }

	public ListViewAndControl<EntryT> Result { get; private set; }

	public static ListViewAndControl<EntryT> ReadListView(UINodeInfoInTree listViewNode, Func<UINodeInfoInTree, IColumnHeader[], RectInt?, EntryT> callbackListEntryConstruct, ListEntryTrenungZeleTypEnum? inEntryTrenungZeleTyp = null)
	{
		SictAuswertGbsListViewport<EntryT> sictAuswertGbsListViewport = new SictAuswertGbsListViewport<EntryT>(listViewNode, callbackListEntryConstruct, inEntryTrenungZeleTyp);
		sictAuswertGbsListViewport.Read();
		return sictAuswertGbsListViewport.Result;
	}

	public static IListEntry ListEntryKonstruktSctandard(UINodeInfoInTree uiNode, IColumnHeader[] header, RectInt? regionConstraint, ListEntryTrenungZeleTypEnum? trenungZeleTyp)
	{
		if ((!(uiNode?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		SictAuswertGbsListEntry sictAuswertGbsListEntry = new SictAuswertGbsListEntry(uiNode, header, regionConstraint, trenungZeleTyp);
		sictAuswertGbsListEntry.Berecne();
		return sictAuswertGbsListEntry.ErgeebnisListEntry;
	}

	public static IListEntry ListEntryKonstruktSctandard(UINodeInfoInTree gbsAst, IColumnHeader[] header, RectInt? regionConstraint)
	{
		return ListEntryKonstruktSctandard(gbsAst, header, regionConstraint, null);
	}

	public SictAuswertGbsListViewport(UINodeInfoInTree listViewNode, Func<UINodeInfoInTree, IColumnHeader[], RectInt?, EntryT> callbackListEntryConstruct, ListEntryTrenungZeleTypEnum? inEntryTrenungZeleTyp = null)
	{
		ListViewNode = listViewNode;
		CallbackListEntryConstruct = callbackListEntryConstruct;
		InEntryTrenungZeleTyp = inEntryTrenungZeleTyp;
	}

	public void Read()
	{
		if (!(ListViewNode?.VisibleIncludingInheritance ?? false))
		{
			return;
		}
		ScrollReader scrollReader = new ScrollReader(ListViewNode);
		scrollReader.Read();
		Scroll result = scrollReader.Result;
		if (result != null)
		{
			ListeHeader = result.ColumnHeader;
			ColumnHeader[] ListeHeaderInfo = ListeHeader?.Select((Func<IColumnHeader, int, ColumnHeader>)((IColumnHeader header, int index) => new ColumnHeader((IContainer)(object)header)
			{
				ColumnIndex = index
			}))?.ToArray();
			ScrollClipperContentNode = scrollReader.ClipperContentNode;
			UINodeInfoInTree scrollClipperContentNode = ScrollClipperContentNode;
			RectInt? obj;
			if (scrollClipperContentNode == null)
			{
				obj = null;
			}
			else
			{
				IUIElement obj2 = scrollClipperContentNode.AsUIElementIfVisible();
				obj = ((obj2 != null) ? new RectInt?(obj2.Region.Value) : null);
			}
			RectInt? clipperRegion = obj;
			EntryT[] entry2 = ScrollClipperContentNode?.ListChild?.Select((UINodeInfoInTree kandidaatEntryAst) => CallbackListEntryConstruct(kandidaatEntryAst, (IColumnHeader[])(object)ListeHeaderInfo, clipperRegion))?.WhereNotDefault()?.OrderBy((EntryT entry) => ((IUIElement)entry).Region.Value.Center().B)?.ToArray();
			Result = new ListViewAndControl<EntryT>(ListViewNode.AsUIElementIfVisible())
			{
				ColumnHeader = ListeHeader,
				Entry = entry2,
				Scroll = (IScroll)(object)result
			};
		}
	}
}
