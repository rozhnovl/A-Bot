using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bib3.Geometrik;
using Newtonsoft.Json.Linq;
using PythonStructures;
using Sanderling.Interface.MemoryStruct;

namespace Eve64
{
	[DebuggerDisplay("DisplayNode: {UiNode.PythonObjectTypeName}({UiNode.NameProperty})")]
	public class UITreeNodeWithDisplayRegion
	{
		public UITreeNode UiNode { get; set; }
		public List<ChildOfNodeWithDisplayRegion>? Children { get; set; } // Nullable to represent Maybe type
		public DisplayRegion SelfDisplayRegion { get; set; }
		public DisplayRegion TotalDisplayRegion { get; set; }
		public DisplayRegion TotalDisplayRegionVisible { get; set; }
	}

	public class ChildOfNodeWithDisplayRegion
	{
		public UITreeNodeWithDisplayRegion? NodeWithRegion { get; init; }
		public UITreeNode? NodeWithoutRegion { get; init; }


	}

	public class DisplayRegion
	{
		public long X { get; set; }
		public long Y { get; set; }
		public long Width { get; set; }
		public long Height { get; set; }
	}

	public static class Parser
	{
		public static UITreeNodeWithDisplayRegion ParseUITreeWithDisplayRegionFromUITree(UITreeNode uiTree)
		{
			var selfDisplayRegion = GetDisplayRegionFromDictEntries(uiTree) ?? new DisplayRegion { X = 0, Y = 0, Width = 0, Height = 0 };

			return AsUITreeNodeWithDisplayRegion(new DisplayRegionParameters
			{
				SelfDisplayRegion = selfDisplayRegion,
				TotalDisplayRegion = selfDisplayRegion,
				OccludedRegions = new List<DisplayRegion>()
			}, uiTree);
		}

		public static ParsedUserInterface ParseUserInterfaceFromUITree(UITreeNodeWithDisplayRegion uiTree)
		{
			return new ParsedUserInterface
			{
				//UiTree = uiTree,
				Menu = ParseContextMenusFromUITreeRoot(uiTree),
				ShipUi = ParseShipUIFromUITreeRoot(uiTree),
				Target = ParseTargetsFromUITreeRoot(uiTree),
				InfoPanelContainer = ParseInfoPanelContainerFromUIRoot(uiTree),
				//OverviewWindows = ParseOverviewWindowsFromUITreeRoot(uiTree),
				//SelectedItemWindow = ParseSelectedItemWindowFromUITreeRoot(uiTree),
				//DronesWindow = ParseDronesWindowFromUITreeRoot(uiTree),
				//FittingWindow = ParseFittingWindowFromUITreeRoot(uiTree),
				//ProbeScannerWindow = ParseProbeScannerWindowFromUITreeRoot(uiTree),
				//DirectionalScannerWindow = ParseDirectionalScannerWindowFromUITreeRoot(uiTree),
				WindowStation = ParseStationWindowFromUITreeRoot(uiTree),
				//InventoryWindows = ParseInventoryWindowsFromUITreeRoot(uiTree),
				ModuleButtonTooltip = ParseModuleButtonTooltipFromUITreeRoot(uiTree),
				//HeatStatusTooltip = ParseHeatStatusTooltipFromUITreeRoot(uiTree),
				//ChatWindowStacks = ParseChatWindowStacksFromUITreeRoot(uiTree),
				//AgentConversationWindows = ParseAgentConversationWindowsFromUITreeRoot(uiTree),
				//MarketOrdersWindow = ParseMarketOrdersWindowFromUITreeRoot(uiTree),
				//SurveyScanWindow = ParseSurveyScanWindowFromUITreeRoot(uiTree),
				//BookmarkLocationWindow = ParseBookmarkLocationWindowFromUITreeRoot(uiTree),
				//RepairShopWindow = ParseRepairShopWindowFromUITreeRoot(uiTree),
				//CharacterSheetWindow = ParseCharacterSheetWindowFromUITreeRoot(uiTree),
				//FleetWindow = ParseFleetWindowFromUITreeRoot(uiTree),
				//LocationsWindow = ParseLocationsWindowFromUITreeRoot(uiTree),
				//WatchListPanel = ParseWatchListPanelFromUITreeRoot(uiTree),
				//StandaloneBookmarkWindow = ParseStandaloneBookmarkWindowFromUITreeRoot(uiTree),
				Neocom = ParseNeocomFromUITreeRoot(uiTree),
				//MessageBoxes = ParseMessageBoxesFromUITreeRoot(uiTree),
				//LayerAbovemain = ParseLayerAbovemainFromUITreeRoot(uiTree),
				//KeyActivationWindow = ParseKeyActivationWindowFromUITreeRoot(uiTree),
				//CompressionWindow = ParseCompressionWindowFromUITreeRoot(uiTree),
			};
		}

		public static UITreeNodeWithDisplayRegion AsUITreeNodeWithDisplayRegion(DisplayRegionParameters parameters, UITreeNode uiNode)
		{
			List<ChildOfNodeWithDisplayRegion> children = null;
			if (uiNode.children != null)
			{
				var (mappedSiblings, occludedRegionsFromSiblings) = uiNode.children.Aggregate(
					(MappedSiblings: new List<ChildOfNodeWithDisplayRegion>(),
						OccludedRegionsFromSiblings: new List<DisplayRegion>()),
					(acc, currentChild) =>
					{
						var currentChildResult = AsUITreeNodeWithInheritedOffset(
							(parameters.TotalDisplayRegion.X, parameters.TotalDisplayRegion.Y),
							acc.OccludedRegionsFromSiblings.Concat(parameters.OccludedRegions).ToList(), currentChild);

						var newOccludedRegionsFromSiblings = currentChildResult
							.NodeWithRegion
							?.ListDescendantsWithDisplayRegion()
							?.Where(node => NodeOccludesFollowingNodes(node.UiNode))
							?.Select(node => node.TotalDisplayRegion)
							?.ToList() ?? new List<DisplayRegion>();

						acc.MappedSiblings.Add(currentChildResult);
						acc.OccludedRegionsFromSiblings.AddRange(newOccludedRegionsFromSiblings);

						return acc;
					});
				mappedSiblings.Reverse();
				children = mappedSiblings;
			}

			//List<ChildOfNodeWithDisplayRegion>? children = uiNode.children?.Select(child =>
			//{
			//	var currentChildResult = AsUITreeNodeWithInheritedOffset(new DisplayRegionParameters
			//	{
			//		SelfDisplayRegion = parameters.TotalDisplayRegion,
			//		TotalDisplayRegion = parameters.TotalDisplayRegion,
			//		OccludedRegions = parameters.OccludedRegions
			//	}, child);

			//	var newOccludedRegions = currentChildResult?.Children?
			//		.Where(c => NodeOccludesFollowingNodes(c.NodeWithRegion.UiNode))
			//		.Select(c => c.NodeWithRegion.TotalDisplayRegion)
			//		.ToList() ?? new List<DisplayRegion>();

			//	parameters.OccludedRegions.AddRange(newOccludedRegions);
			//	//TODO double check
			//	return new ChildOfNodeWithDisplayRegion() { NodeWithRegion = currentChildResult };
			//}).ToList();

			var totalDisplayRegionVisible = SubtractRegionsFromRegion(new RegionSubtractionParameters
			{
				Minuend = parameters.TotalDisplayRegion,
				Subtrahend = parameters.OccludedRegions
			}).OrderByDescending(r => GetAreaFromDisplayRegion(r) ?? -1).FirstOrDefault() ?? new DisplayRegion { X = -1, Y = -1, Width = 0, Height = 0 };

			return new UITreeNodeWithDisplayRegion
			{
				UiNode = uiNode,
				Children = children?.ToList(),
				SelfDisplayRegion = parameters.SelfDisplayRegion,
				TotalDisplayRegion = parameters.TotalDisplayRegion,
				TotalDisplayRegionVisible = totalDisplayRegionVisible
			};
		}

		public static ChildOfNodeWithDisplayRegion AsUITreeNodeWithInheritedOffset(
			(long X, long Y) inheritedOffset,
			List<DisplayRegion> occludedRegions,
			UITreeNode rawNode)
		{
			var selfRegion = GetDisplayRegionFromDictEntries(rawNode);

			if (selfRegion == null)
			{
				return new ChildOfNodeWithDisplayRegion() { NodeWithoutRegion = rawNode };
			}

			var totalDisplayRegion = new DisplayRegion
			{
				X = inheritedOffset.X + selfRegion.X,
				Y = inheritedOffset.Y + selfRegion.Y,
				Width = selfRegion.Width,
				Height = selfRegion.Height
			};

			return new ChildOfNodeWithDisplayRegion
			{
				NodeWithRegion = AsUITreeNodeWithDisplayRegion(new DisplayRegionParameters
					{
						SelfDisplayRegion = selfRegion, TotalDisplayRegion = totalDisplayRegion,
						OccludedRegions = occludedRegions
					},
					rawNode)
			};
		}
		public static DisplayRegion? GetDisplayRegionFromDictEntries(UITreeNode uiNode)
		{
			long? FixedNumberFromJsonValue(dynamic value)
			{
				try
				{
					if (value is int i)
					{
						return i;
					}
					if (value is long l)
					{
						return l;
					}
					if (value is string s && int.TryParse(s, out var result))
					{
						return result;
					}
					if (value.int_low32 != null)
					{
						return FixedNumberFromJsonValue(value.int_low32);
					}
					if (value.Type == JTokenType.Integer)
					{
						return value.Value<int>();
					}
				}
				catch
				{
					// Ignored
				}
				return null;
			}

			long? FixedNumberFromPropertyName(string propertyName)
			{
				return uiNode.DictEntriesOfInterest.TryGetValue(propertyName, out var value)
					? value is int i ? i :
					value is long l ? l : FixedNumberFromJsonValue(value)
					: null;
			}

			var displayX = FixedNumberFromPropertyName("_displayX");
			var displayY = FixedNumberFromPropertyName("_displayY");
			var displayWidth = FixedNumberFromPropertyName("_displayWidth");
			var displayHeight = FixedNumberFromPropertyName("_displayHeight");

			if (displayX.HasValue && displayY.HasValue && displayWidth.HasValue && displayHeight.HasValue)
			{
				return new DisplayRegion
				{
					X = displayX.Value,
					Y = displayY.Value,
					Width = displayWidth.Value,
					Height = displayHeight.Value
				};
			}

			return null;
		}
		private static UITreeNodeWithDisplayRegion AsUITreeNodeWithInheritedOffset(DisplayRegionParameters parameters, UITreeNode uiNode)
		{
			// Placeholder for actual logic
			return null;
		}

		private static List<DisplayRegion> SubtractRegionsFromRegion(RegionSubtractionParameters parameters)
		{
			// Placeholder for actual logic
			return new List<DisplayRegion>();
		}

		private static long? GetAreaFromDisplayRegion(DisplayRegion region)
		{
			if (region == null) return null;
			return region.Width * region.Height;
		}

		private static bool NodeOccludesFollowingNodes(UITreeNode node)
		{
			// Placeholder for logic to check if a node occludes following nodes
			return false;
		}

		// Methods for parsing specific parts of the user interface
		private static IMenu[] ParseContextMenusFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static IShipUi ParseShipUIFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static IShipUiTarget[] ParseTargetsFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		public static InfoPanelContainer? ParseInfoPanelContainerFromUIRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
		{
			var containerNode = uiTreeRoot
				.ListDescendantsWithDisplayRegion()
				.Where(node => node.UiNode.PythonObjectTypeName == "InfoPanelContainer")
				.OrderByDescending(node => node.UiNode.EnumerateSelfAndDescendants().Count())
				.FirstOrDefault();

			if (containerNode == null) return null;

			return new InfoPanelContainer
			{
				UINode = containerNode,
				Icons = ParseInfoPanelIconsFromInfoPanelContainer(containerNode),
				InfoPanelLocationInfo = ParseInfoPanelLocationInfoFromInfoPanelContainer(containerNode),
				InfoPanelRoute = ParseInfoPanelRouteFromInfoPanelContainer(containerNode),
				//InfoPanelAgentMissions = ParseInfoPanelAgentMissionsFromInfoPanelContainer(containerNode)
			};
		}

		public static InfoPanelIcons? ParseInfoPanelIconsFromInfoPanelContainer(UITreeNodeWithDisplayRegion infoPanelContainerNode)
		{
			var iconContainerNode = infoPanelContainerNode
				.ListDescendantsWithDisplayRegion()
				.Where(node => node.UiNode.GetNameFromDictEntries() == "iconCont")
				.OrderBy(node => node.TotalDisplayRegion.Y)
				.FirstOrDefault();

			if (iconContainerNode == null) return null;

			UITreeNodeWithDisplayRegion? IconNodeFromTexturePathEnd(string texturePathEnd) =>
				iconContainerNode.ListDescendantsWithDisplayRegion()
					.Where(node => node.UiNode.GetTexturePathFromDictEntries()?.EndsWith(texturePathEnd) == true)
					.FirstOrDefault();

			return new InfoPanelIcons
			{
				UINode = iconContainerNode,
				Search = IconNodeFromTexturePathEnd("search.png"),
				LocationInfo = IconNodeFromTexturePathEnd("LocationInfo.png"),
				Route = IconNodeFromTexturePathEnd("Route.png"),
				AgentMissions = IconNodeFromTexturePathEnd("Missions.png"),
				DailyChallenge = IconNodeFromTexturePathEnd("dailyChallenge.png")
			};
		}

		public static InfoPanelLocationInfo? ParseInfoPanelLocationInfoFromInfoPanelContainer(UITreeNodeWithDisplayRegion infoPanelContainerNode)
		{
			var infoPanelNode = infoPanelContainerNode
				.ListDescendantsWithDisplayRegion()
				.Where(node => node.UiNode.PythonObjectTypeName == "InfoPanelLocationInfo")
				.FirstOrDefault();

			if (infoPanelNode == null) return null;

			var securityStatusPercent = infoPanelNode
				.GetAllContainedDisplayTexts()
				.Select(ParseSecurityStatusPercentFromUINodeText)
				.FirstOrDefault(text => text != null);

			var currentSolarSystemName = infoPanelNode
				.GetAllContainedDisplayTexts()
				.Select(ParseCurrentSolarSystemFromUINodeText)
				.FirstOrDefault() ??
				infoPanelNode.ListDescendantsWithDisplayRegion()
					.Where(node => node.UiNode.GetNameFromDictEntries()?.ToLower().Contains("labelsystemname") == true)
					.SelectMany(node => node.GetAllContainedDisplayTexts())
					.FirstOrDefault();

			var maybeListSurroundingsButton = infoPanelNode
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UiNode.PythonObjectTypeName == "ListSurroundingsBtn");

			var expandedContent = infoPanelNode
				.ListDescendantsWithDisplayRegion()
				.Where(node =>
					node.UiNode.PythonObjectTypeName.Contains("Container") &&
					node.UiNode.GetNameFromDictEntries()?.Contains("mainCont") == true)
				.FirstOrDefault()?.GetAllContainedDisplayTexts()
				.Select(ParseCurrentStationNameFromInfoPanelLocationInfoLabelText)
				.FirstOrDefault();

			if (maybeListSurroundingsButton == null) return null;

			return new InfoPanelLocationInfo
			{
				UINode = infoPanelNode,
				ListSurroundingsButton = maybeListSurroundingsButton,
				CurrentSolarSystemName = currentSolarSystemName,
				SecurityStatusPercent = securityStatusPercent?.ToString(),
				ExpandedContent = expandedContent
			};
		}

		// Placeholders for undefined methods
		public static InfoPanelRoute? ParseInfoPanelRouteFromInfoPanelContainer(UITreeNodeWithDisplayRegion containerNode) => null;
		//public static InfoPanelAgentMissions? ParseInfoPanelAgentMissionsFromInfoPanelContainer(UITreeNodeWithDisplayRegion containerNode) => null;
		public static int? ParseSecurityStatusPercentFromUINodeText(string text)
		{
			string?[] patterns = {
				GetSubstringBetweenXmlTagsAfterMarker(text, "hint='Security status'"),
				GetSubstringBetweenXmlTagsAfterMarker(text, "hint=\"Security status\"><color=")
			};

			foreach (var pattern in patterns.Where(p => p != null))
			{
				if (float.TryParse(pattern.Trim(), out var value))
				{
					return (int)Math.Round(value * 100);
				}
			}

			return null;
		}

		public static string? ParseCurrentSolarSystemFromUINodeText(string text)
		{
			string?[] patterns = {
				GetSubstringBetweenXmlTagsAfterMarker(text, "alt='Current Solar System'"),
				GetSubstringBetweenXmlTagsAfterMarker(text, "alt=\"Current Solar System\"")
			};

			return patterns.FirstOrDefault(p => !string.IsNullOrEmpty(p));
		}

		public static string? ParseCurrentStationNameFromInfoPanelLocationInfoLabelText(string text)
		{
			var result = GetSubstringBetweenXmlTagsAfterMarker(text, "alt='Current Station'");
			return result?.Trim();
		}

		private static string? GetSubstringBetweenXmlTagsAfterMarker(string text, string marker)
		{
			var markerIndex = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
			if (markerIndex == -1) return null;

			var tagStartIndex = text.IndexOf('>', markerIndex);
			var tagEndIndex = text.IndexOf('<', tagStartIndex);

			if (tagStartIndex != -1 && tagEndIndex != -1 && tagEndIndex > tagStartIndex)
			{
				return text.Substring(tagStartIndex + 1, tagEndIndex - tagStartIndex - 1);
			}

			return null;
		}
		private static object ParseOverviewWindowsFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseSelectedItemWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseDronesWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseFittingWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseProbeScannerWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseDirectionalScannerWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;

		private static IWindowStation[] ParseStationWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree)
		{
			// Find the first descendant node of type "LobbyWnd"
			var windowNode = uiTree
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node =>
					node.UiNode.PythonObjectTypeName == "LobbyWnd");

			if (windowNode == null)
			{
				return null;
			}

			// Helper function to find a button by its display text
			UITreeNodeWithDisplayRegion? ButtonFromDisplayText(string textToSearch)
			{
				var textToSearchLowercase = textToSearch.ToLower();

				bool TextMatches(string text)
				{
					return text == textToSearchLowercase ||
					       text.Contains($">{textToSearchLowercase}<");
				}

				return windowNode.FindButtonInDescendantsByDisplayTextsPredicate(
					(IEnumerable<string> descendantTexts) => descendantTexts.Any(text => TextMatches(text.ToLower()))
				);
			}

			return new[]
			{
				new WindowStation()
				{
					Region = windowNode.AsUiElement().Region,
					//UiNode = windowNode,
					UndockButton = ButtonFromDisplayText("undock").AsUiElement(),
					//AbortUndockButton = ButtonFromDisplayText("undocking")
				}
			};
		}

		private static UITreeNodeWithDisplayRegion FindButtonInDescendantsByDisplayTextsPredicate(this UITreeNodeWithDisplayRegion parent, Func<IEnumerable<string>, bool> predicate)
		{
			return ListDescendantsWithDisplayRegion(parent)
				.Where(n => n.UiNode.PythonObjectTypeName.Contains("Button"))
				.Where(n => predicate(GetAllContainedDisplayTexts(n)))
				.OrderBy(n => GetAreaFromDisplayRegion(n.TotalDisplayRegion))
				.FirstOrDefault();
		}
		public static string? GetDisplayText(UITreeNode uiNode)
		{
			var propertiesToCheck = new[] { "_setText", "_text" };

			return propertiesToCheck
				.Select(propertyName => uiNode.DictEntriesOfInterest.TryGetValue(propertyName, out var value)
					? GetDisplayTextFromDictEntry(value)
					: null)
				.Where(text => text != null)
				.OrderByDescending(text => text?.Length ?? 0)
				.FirstOrDefault();
		}

		private static string? GetDisplayTextFromDictEntry(object dictEntryValue)
		{
			try
			{
				if (dictEntryValue is string s)
				{
					return s;
				}
				else if (dictEntryValue is UITreeNode asNode)
				{
					return asNode != null ? GetDisplayText(asNode) : null;
				}
			}
			catch
			{
				// Ignored: Failed to decode value.
			}

			return null;
		}

		public static List<string> GetAllContainedDisplayTexts(this UITreeNodeWithDisplayRegion uiNode)
		{
			var nodes = new List<UITreeNode> { uiNode.UiNode }
				.Concat(ListDescendantsWithDisplayRegion(uiNode).Select(u=>u.UiNode))
				.ToList();

			return nodes
				.Select(GetDisplayText)
				.Where(text => text != null)
				.ToList();
		}

		public static List<(string DisplayText, UITreeNodeWithDisplayRegion NodeWithRegion)> GetAllContainedDisplayTextsWithRegion(this UITreeNodeWithDisplayRegion uiNode)
		{
			var nodesWithRegion = new List<UITreeNodeWithDisplayRegion> { uiNode }
				.Concat(uiNode.ListDescendantsWithDisplayRegion())
				.ToList();

			return nodesWithRegion
				.Select(descendant => {
					var displayText = GetDisplayText(descendant.UiNode);
					return !string.IsNullOrEmpty(displayText) ? (displayText, descendant) : default((string, UITreeNodeWithDisplayRegion)?);
				})
				.Where(tuple => tuple != null)
				.Select(tuple => tuple!.Value)
				.ToList();
		}
		public static List<UITreeNodeWithDisplayRegion> ListDescendantsWithDisplayRegion(this UITreeNodeWithDisplayRegion parent)
		{
			// Get the children of the current node
			var children = ListChildrenWithDisplayRegion(parent);

			// Recursively get the descendants of each child
			return children.Concat(
				children.SelectMany(child => ListDescendantsWithDisplayRegion(child))
			).ToList();
		}
		public static List<UITreeNodeWithDisplayRegion> ListChildrenWithDisplayRegion(UITreeNodeWithDisplayRegion parent)
		{
			// Get the children property, defaulting to an empty list if it's null
			var children = parent.Children ?? new List<ChildOfNodeWithDisplayRegion>();

			// Filter and map the children using the justCaseWithDisplayRegion function
			return children
				.Select(JustCaseWithDisplayRegion)
				.Where(child => child != null)
				.Select(child => child!)
				.ToList();
		}
		public static UITreeNodeWithDisplayRegion? JustCaseWithDisplayRegion(ChildOfNodeWithDisplayRegion child)
		{
			if (child.NodeWithRegion !=null)
				return child.NodeWithRegion;
			return null;
		}
		private static object ParseInventoryWindowsFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static IContainer ParseModuleButtonTooltipFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseHeatStatusTooltipFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseChatWindowStacksFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseAgentConversationWindowsFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseMarketOrdersWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseSurveyScanWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseBookmarkLocationWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseRepairShopWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseCharacterSheetWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseFleetWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseLocationsWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseWatchListPanelFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseStandaloneBookmarkWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static INeocom ParseNeocomFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseMessageBoxesFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseLayerAbovemainFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseKeyActivationWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;
		private static object ParseCompressionWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTree) => null;

		public static IUIElement? AsUiElement(this UITreeNodeWithDisplayRegion? uiTree)
		{
			if (uiTree == null) return null;
			return new UIElement
			{
				Region = new RectInt
				{
					Min0 = uiTree.TotalDisplayRegion.X,
					Min1 = uiTree.TotalDisplayRegion.Y,
					Max0 = uiTree.TotalDisplayRegion.X + uiTree.TotalDisplayRegion.Width,
					Max1 = uiTree.TotalDisplayRegion.Y + uiTree.TotalDisplayRegion.Height
				}
			};
		}
		public static string? GetNameFromDictEntries(this UITreeNode uiNode) =>
			GetStringPropertyFromDictEntries("_name", uiNode);

		public static string? GetHintTextFromDictEntries(this UITreeNode uiNode) =>
			GetStringPropertyFromDictEntries("_hint", uiNode);

		public static string? GetTexturePathFromDictEntries(this UITreeNode uiNode)
		{
			return GetStringPropertyFromDictEntries("texturePath", uiNode) ??
				   GetStringPropertyFromDictEntries("_texturePath", uiNode);
		}

		public static string? GetStringPropertyFromDictEntries(string dictEntryKey, UITreeNode uiNode)
		{
			if (uiNode.DictEntriesOfInterest.TryGetValue(dictEntryKey, out var value))
			{
				try
				{
					return value as string;
				}
				catch
				{
					return null;
				}
			}
			return null;
		}

		public static ColorComponents? GetColorPercentFromDictEntries(UITreeNode uiNode)
		{
			if (uiNode.DictEntriesOfInterest.TryGetValue("_color", out var value))
			{
				return JsonDecodeColorPercent((JToken)value);
			}
			return null;
		}

		public static float? GetRotationFloatFromDictEntries(UITreeNode uiNode)
		{
			return uiNode.DictEntriesOfInterest.TryGetValue("_rotation", out var value) && value is float f
				? f
				: null;
		}

		public static float? GetOpacityFloatFromDictEntries(UITreeNode uiNode)
		{
			return uiNode.DictEntriesOfInterest.TryGetValue("_opacity", out var value) && value is float f
				? f
				: null;
		}

		private static ColorComponents? JsonDecodeColorPercent(JToken json)
		{
			try
			{
				return new ColorComponents
				{
					APercent = JsonDecodeIntFromIntOrString(json["aPercent"]),
					RPercent = JsonDecodeIntFromIntOrString(json["rPercent"]),
					GPercent = JsonDecodeIntFromIntOrString(json["gPercent"]),
					BPercent = JsonDecodeIntFromIntOrString(json["bPercent"])
				};
			}
			catch
			{
				return null;
			}
		}
		public class ColorComponents
		{
			public int APercent { get; set; }
			public int RPercent { get; set; }
			public int GPercent { get; set; }
			public int BPercent { get; set; }
		}
		private static int JsonDecodeIntFromIntOrString(JToken json)
		{
			if (json.Type == JTokenType.Integer)
			{
				return json.ToObject<int>();
			}
			if (json.Type == JTokenType.String && int.TryParse(json.ToString(), out var result))
			{
				return result;
			}
			throw new InvalidOperationException($"Failed to parse integer from {json}");
		}
	}

	internal class RegionSubtractionParameters
	{
		public DisplayRegion Minuend { get; set; }
		public List<DisplayRegion> Subtrahend { get; set; }
	}

	public class DisplayRegionParameters
	{
		public DisplayRegion SelfDisplayRegion { get; set; }
		public DisplayRegion TotalDisplayRegion { get; set; }
		public List<DisplayRegion> OccludedRegions { get; set; }
	}

	public class ParsedUserInterface : IMemoryMeasurement
	{
		public int? SessionDurationRemaining { get; init; }
		public string UserDefaultLocaleName { get; init; }
		public string VersionString { get; init; }
		public Vektor2DInt ScreenSize { get; init; }
		public IMenu[] Menu { get; init; }
		public IContainer[] Tooltip { get; init; }
		public IShipUi ShipUi { get; init; }
		public IShipUiTarget[] Target { get; init; }
		public IInSpaceBracket[] InflightBracket { get; init; }
		public IContainer ModuleButtonTooltip { get; init; }
		public IWindow SystemMenu { get; init; }
		public INeocom Neocom { get; init; }
		public IUIElement InfoPanelButtonCurrentSystem { get; init; }
		public IUIElement InfoPanelButtonRoute { get; init; }
		public IUIElement InfoPanelButtonMissions { get; init; }
		public IUIElement InfoPanelButtonIncursions { get; init; }
		public IInfoPanelContainer InfoPanelContainer { get; init; }
		public IInfoPanelSystem InfoPanelCurrentSystem { get; init; }
		public IInfoPanelRoute InfoPanelRoute { get; init; }
		public IInfoPanelMissions InfoPanelMissions { get; init; }
		public IContainer[] Utilmenu { get; init; }
		public IUIElementText[] AbovemainMessage { get; init; }
		public PanelGroup[] AbovemainPanelGroup { get; init; }
		public PanelGroup[] AbovemainPanelEveMenu { get; init; }
		public IWindow[] WindowOther { get; init; }
		public WindowStack[] WindowStack { get; init; }
		public IWindowOverview[] WindowOverview { get; init; }
		public WindowChatChannel[] WindowChatChannel { get; init; }
		public IWindowSelectedItemView[] WindowSelectedItemView { get; init; }
		public IWindowDroneView[] WindowDroneView { get; init; }
		public WindowPeopleAndPlaces[] WindowPeopleAndPlaces { get; init; }
		public IWindowStation[] WindowStation { get; init; }
		public WindowShipFitting[] WindowShipFitting { get; init; }
		public WindowFittingMgmt[] WindowFittingMgmt { get; init; }
		public IWindowSurveyScanView[] WindowSurveyScanView { get; init; }
		public IWindowInventory[] WindowInventory { get; init; }
		public IWindowAgentDialogue[] WindowAgentDialogue { get; init; }
		public WindowAgentBrowser[] WindowAgentBrowser { get; init; }
		public WindowTelecom[] WindowTelecom { get; init; }
		public WindowRegionalMarket[] WindowRegionalMarket { get; init; }
		public WindowMarketAction[] WindowMarketAction { get; init; }
		public WindowItemSell[] WindowItemSell { get; init; }
		public IEnumerable<IWindowProbeScanner> WindowProbeScanner { get; init; }
	}
	// Supporting classes
	public class InfoPanelContainer:IInfoPanelContainer
	{
		public UITreeNodeWithDisplayRegion UINode { get; set; }
		public InfoPanelIcons? Icons { get; set; }
		public InfoPanelLocationInfo InfoPanelLocationInfo { get; set; }
		//public IInfoPanelSystem InfoPanelCurrentSystem { get; init; }

		public IInfoPanelMissions InfoPanelMissions { get; init; }

		public IInfoPanelLocationInfo LocationInfo => InfoPanelLocationInfo;
		public IInfoPanelRoute InfoPanelRoute { get; set; }
		//public InfoPanelAgentMissions? InfoPanelAgentMissions { get; set; }
	}

	public class InfoPanelIcons
	{
		public UITreeNodeWithDisplayRegion UINode { get; set; }
		public UITreeNodeWithDisplayRegion? Search { get; set; }
		public UITreeNodeWithDisplayRegion? LocationInfo { get; set; }
		public UITreeNodeWithDisplayRegion? Route { get; set; }
		public UITreeNodeWithDisplayRegion? AgentMissions { get; set; }
		public UITreeNodeWithDisplayRegion? DailyChallenge { get; set; }
	}

	public class InfoPanelLocationInfo:IInfoPanelLocationInfo
	{
		public UITreeNodeWithDisplayRegion UINode { get; set; }
		public UITreeNodeWithDisplayRegion? ListSurroundingsButton { get; set; }
		public string? CurrentSolarSystemName { get; set; }
		public string? SecurityStatusPercent { get; set; }
		public string? ExpandedContent { get; set; }
	}
}
