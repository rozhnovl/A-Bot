using System.Collections.Immutable;

namespace PythonStructures
{
	public class UITreeNode
	{
		public ulong pythonObjectAddress { set; get; }

		public string PythonObjectTypeName { set; get; }

		public ImmutableDictionary<string, object> DictEntriesOfInterest { set; get; }

		public string? NameProperty => DictEntriesOfInterest.GetValueOrDefault("_name")?.ToString();
		public string[] otherDictEntriesKeys { set; get; }

		public UITreeNode[]? children { set; get; }

		public record DictEntry
		{
			public string key { set; get; }
			public object value { set; get; }
		}
		public record Bunch(
			System.Text.Json.Nodes.JsonObject entriesOfInterest);


		public class DictEntryValueGenericRepresentation
		{
			public ulong address { set; get; }

			public string pythonObjectTypeName { set; get; }
		}

		public IEnumerable<UITreeNode> EnumerateSelfAndDescendants() =>
			new[] { this }
				.Concat((children ?? Array.Empty<UITreeNode>()).SelectMany(child => child?.EnumerateSelfAndDescendants() ?? ImmutableList<UITreeNode>.Empty));

		public UITreeNode WithOtherDictEntriesRemoved()
		{
			return new UITreeNode
			{
				pythonObjectAddress = pythonObjectAddress,
				PythonObjectTypeName = PythonObjectTypeName,
				DictEntriesOfInterest = DictEntriesOfInterest,
				otherDictEntriesKeys = null,
				children = children?.Select(child => child?.WithOtherDictEntriesRemoved()).ToArray(),
			};
		}
	}
	/*

	public class UITreeNode : PyObjectWithRefToDictAt8
	{
		PyDictEntry DictEntryChildren;

		PyChildrenList ChildrenList;

		public UITreeNode[] children
		{
			private set;
			get;
		}

		public UITreeNode(
			Int64 BaseAddress,
			IMemoryReader MemoryReader)
			:
			base(BaseAddress, MemoryReader)
		{
		}

		public UITreeNode[] LoadChildren(
			IPythonMemoryReader MemoryReader)
		{
			var Dict = this.Dict;

			if (null != Dict)
			{
				DictEntryChildren = Dict.EntryForKeyStr("children");
			}

			if (null != DictEntryChildren)
			{
				if (DictEntryChildren.me_value.HasValue)
				{
					ChildrenList = new PyChildrenList(DictEntryChildren.me_value.Value, MemoryReader);

					ChildrenList.LoadDict(MemoryReader);

					ChildrenList.LoadChildren(MemoryReader);
				}
			}

			if (null != ChildrenList)
			{
				children = ChildrenList.children;
			}

			return children;
		}

		public IEnumerable<UITreeNode> EnumerateChildrenTransitive(
			IPythonMemoryReader MemoryReader,
			int? DepthMax = null)
		{
			if (DepthMax <= 0)
			{
				yield break;
			}

			this.LoadDict(MemoryReader);

			this.LoadChildren(MemoryReader);

			var children = this.children;

			if (null == children)
			{
				yield break;
			}

			foreach (var child in children)
			{
				yield return child;

				foreach (var childChild in child.EnumerateChildrenTransitive(MemoryReader, DepthMax - 1))
				{
					yield return childChild;
				}
			}
		}
	}*/
}