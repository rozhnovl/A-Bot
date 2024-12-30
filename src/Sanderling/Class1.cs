using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bib3;
using Bib3.Geometrik;
using Sanderling.Interface.MemoryStruct;

namespace EveOnline.ParseUserInterface
{
	// ------------------------- Models -------------------------

	public struct Location2D
	{
		public int X { get; set; }
		public int Y { get; set; }
	}

	public class DisplayRegion
	{
		public int Left { get; set; }
		public int Top { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
	}

	public class UITreeNodeWithDisplayRegion
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public DisplayRegion DisplayRegion { get; set; }
		public List<UITreeNodeWithDisplayRegion> Children { get; set; }
	}

	public class ParsedUserInterface
	{
		public UITreeNodeWithDisplayRegion UiTree { get; set; }
	}

	// ------------------------- Core Functions -------------------------

	public static class UITreeNavigator
	{
		public static UITreeNodeWithDisplayRegion FindNodeById(string id, UITreeNodeWithDisplayRegion node)
		{
			if (node.Id == id)
				return node;

			if (node.Children != null)
			{
				foreach (var child in node.Children)
				{
					var result = FindNodeById(id, child);
					if (result != null)
						return result;
				}
			}

			return null;
		}

		public static List<UITreeNodeWithDisplayRegion> FilterNodesByName(string name, UITreeNodeWithDisplayRegion node)
		{
			var matchingNodes = new List<UITreeNodeWithDisplayRegion>();

			if (node.Name == name)
				matchingNodes.Add(node);

			if (node.Children != null)
			{
				foreach (var child in node.Children)
				{
					matchingNodes.AddRange(FilterNodesByName(name, child));
				}
			}

			return matchingNodes;
		}

		public static List<UITreeNodeWithDisplayRegion> GetLeafNodes(UITreeNodeWithDisplayRegion node)
		{
			var leafNodes = new List<UITreeNodeWithDisplayRegion>();

			if (node.Children == null || node.Children.Count == 0)
				leafNodes.Add(node);
			else
			{
				foreach (var child in node.Children)
				{
					leafNodes.AddRange(GetLeafNodes(child));
				}
			}

			return leafNodes;
		}
	}

	// ------------------------- JSON Serialization -------------------------

	public static class JsonUtils
	{
		public static string SerializeParsedUserInterface(ParsedUserInterface parsedUI)
		{
			return JsonSerializer.Serialize(parsedUI, new JsonSerializerOptions { WriteIndented = true });
		}

		public static ParsedUserInterface DeserializeParsedUserInterface(string json)
		{
			return JsonSerializer.Deserialize<ParsedUserInterface>(json);
		}

		public static string SerializeUITreeNode(UITreeNodeWithDisplayRegion node)
		{
			return JsonSerializer.Serialize(node, new JsonSerializerOptions { WriteIndented = true });
		}

		public static UITreeNodeWithDisplayRegion DeserializeUITreeNode(string json)
		{
			return JsonSerializer.Deserialize<UITreeNodeWithDisplayRegion>(json);
		}
	}
}