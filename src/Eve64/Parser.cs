using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Bib3.Geometrik;
using Newtonsoft.Json.Linq;
using PythonStructures;
using Sanderling.Interface.MemoryStruct;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace Eve64
{
	[DebuggerDisplay("DisplayNode: {UiNode.PythonObjectTypeName}({UiNode.NameProperty})")]
	public class UITreeNodeWithDisplayRegion
	{
		public UITreeNode UINode=>UiNode;
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

		public long Area()
		{
			return Width * Height;
		}
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
				WindowOverview = ParseOverviewWindowsFromUITreeRoot(uiTree).ToArray(),
				WindowSelectedItemView =new[]{ ParseSelectedItemWindowFromUITreeRoot(uiTree)},
				WindowDroneView = ParseDronesWindowFromUITreeRoot(uiTree),
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
		private static IShipUi? ParseShipUIFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
		{
			var shipUINode = uiTreeRoot
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UINode.PythonObjectTypeName == "ShipUI");

			if (shipUINode == null)
				return null;

			var capacitorUINode = shipUINode
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UINode.PythonObjectTypeName == "CapacitorContainer");

			if (capacitorUINode == null)
				return null;

			var capacitor = ParseShipUICapacitorFromUINode(capacitorUINode);

			var indicationNode = shipUINode
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node =>
				{
					var name = node.UINode.GetNameFromDictEntries()?.ToLower();
					return name != null && name.Contains("indicationcontainer");
				});

			var indication = indicationNode != null ? ParseShipUIIndication(indicationNode) : null;

			var moduleButtons = shipUINode
				.ListDescendantsWithDisplayRegion()
				.Where(node => node.UINode.PythonObjectTypeName == "ShipSlot")
				.SelectMany(slotNode =>
					slotNode.ListDescendantsWithDisplayRegion()
						.Where(n => n.UINode.PythonObjectTypeName == "ModuleButton")
						.Select(moduleButtonNode =>
							ParseShipUIModuleButton(slotNode, moduleButtonNode)
						)
				)
				.ToList();
			Func<string, int?> GetLastValuePercentFromGaugeName = gaugeName =>
			{
				var gaugeNode = shipUINode
					.ListDescendantsWithDisplayRegion()
					.FirstOrDefault(node =>
					{
						var name = node.UINode.GetNameFromDictEntries();
						return name != null && name == gaugeName;
					});

				if (gaugeNode == null)
					return null;

				var lastValueToken = gaugeNode.UINode.DictEntriesOfInterest.TryGetValue("_lastValue", out var value) ? value : null;

				if (lastValueToken == null)
					return null;

				if (float.TryParse(lastValueToken.ToString(), out var decodedValue))
				{
					return (int)Math.Round(decodedValue * 100);
				}

				return null;
			};


				var offensiveBuffButtons = shipUINode
				.ListDescendantsWithDisplayRegion()
				.Select(node => new OffensiveBuffButton { UINode = node.AsUiElement(), Name = node.UINode.GetNameFromDictEntries() })
				.Where(x => x.Name != null)
				.ToList();

			var squadronsUI = shipUINode
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UINode.PythonObjectTypeName == "SquadronsUI");

			var heatGauges = shipUINode
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UINode.PythonObjectTypeName == "HeatGauges");
			Func<string, UITreeNodeWithDisplayRegion?> FindDescendantNode = pythonObjectTypeName =>
				shipUINode
					.ListDescendantsWithDisplayRegion()
					.SingleOrDefault(node => node.UINode.PythonObjectTypeName == pythonObjectTypeName);
			return GetLastValuePercentFromGaugeName("armorGauge") == null
				? null
				: new ShipUi
				{
					Region = shipUINode.AsUiElement().Region,
					Capacitor = capacitor,
					HitpointsPercent = new Hitpoints()
					{
						Armor = GetLastValuePercentFromGaugeName("armorGauge").Value,
						Shield = GetLastValuePercentFromGaugeName("shieldGauge").Value,
						Structure = GetLastValuePercentFromGaugeName("structureGauge").Value,
					},
					Indication = indication,
					ModuleButtons = moduleButtons,
					ModuleButtonsRows = GroupShipUIModulesIntoRows(capacitor, moduleButtons),
					OffensiveBuffButtons = offensiveBuffButtons,
					SquadronsUI = null,//TODO squadronsUI,
					StopButton = FindDescendantNode("StopButton")?.AsUiElement(),
					MaxSpeedButton = FindDescendantNode("MaxSpeedButton")?.AsUiElement(),
					HeatGauges = heatGauges != null ? ParseShipUIHeatGaugesFromUINode(heatGauges) : null
				};
		}
		public static ShipUIIndication ParseShipUIIndication(UITreeNodeWithDisplayRegion indicationUINode)
		{
			var displayTexts = indicationUINode.GetAllContainedDisplayTexts();

			var maneuverPatterns = new List<(string Pattern, ShipManeuverType Type)>
			{
				("Warp", ShipManeuverType.Warp),
				("Jump", ShipManeuverType.Jump),
				("Orbit", ShipManeuverType.Orbit),
				("Approach", ShipManeuverType.Approach),
				// Korean samples
				("워프 드라이브 가동", ShipManeuverType.Warp),
				("점프 중", ShipManeuverType.Jump)
			};

			var maneuverType = maneuverPatterns
				.FirstOrDefault(pattern => displayTexts.Any(text => text.Contains(pattern.Pattern)))
				.Type;

			return new ShipUIIndication
			{
				UINode = indicationUINode.AsUiElement(),
				ManeuverType = maneuverType
			};
		}

		private static ShipUICapacitor ParseShipUICapacitorFromUINode(UITreeNodeWithDisplayRegion capacitorUINode)
		{
			var pmarks = capacitorUINode
				.ListDescendantsWithDisplayRegion()
				.Where(node => node.UINode.GetNameFromDictEntries() == "pmark")
				.Select(pmarkUINode => new ShipUICapacitorPmark
				{
					UINode = pmarkUINode.AsUiElement(),
					ColorPercent = pmarkUINode.UiNode.GetColorPercentFromDictEntries()
				})
				.ToList();

			var levelFromPmarksPercent = CalculatePmarksPercent(pmarks);

			return new ShipUICapacitor
			{
				UINode = capacitorUINode.AsUiElement(),
				Pmarks = pmarks,
				LevelFromPmarksPercent = levelFromPmarksPercent
			};
		}

		private static int? CalculatePmarksPercent(List<ShipUICapacitorPmark> pmarks)
		{
			var pmarksFills = pmarks
				.Select(pmark => pmark.ColorPercent?.APercent < 20)
				.ToList();

			if (!pmarksFills.Any())
				return null;

			var filled = pmarksFills.Count(x => x == true);
			return (filled * 100) / pmarksFills.Count;
		}

		private static ShipUIHeatGauges ParseShipUIHeatGaugesFromUINode(UITreeNodeWithDisplayRegion gaugesUINode)
		{
			var gauges = gaugesUINode
				.ListDescendantsWithDisplayRegion()
				.Where(node => node.UINode.GetNameFromDictEntries() == "heatGauge")
				.Select(gaugeNode =>
				{
					var rotationPercent = (int)(gaugeNode.UINode.GetRotationFloatFromDictEntries()*100);
					return new ShipUIHeatGauge
					{
						UINode = gaugeNode.AsUiElement(),
						RotationPercent = rotationPercent,
						HeatPercent = rotationPercent != null ? CalculateHeatPercent(rotationPercent) : null
					};
				})
				.ToList();

			return new ShipUIHeatGauges
			{
				UINode = gaugesUINode.AsUiElement(),
				Gauges = gauges
			};
		}

		private static int? CalculateHeatPercent(float rotationPercent)
		{
			var heatGaugesRotationZeroValues = new[] { -213, -108, -3 };
			foreach (var zero in heatGaugesRotationZeroValues)
			{
				if (rotationPercent <= zero && zero - 100 <= rotationPercent)
				{
					return -(int)(rotationPercent - zero);
				}
			}

			return null;
		}

		private static ShipUIModuleButton ParseShipUIModuleButton(UITreeNodeWithDisplayRegion slotNode, UITreeNodeWithDisplayRegion moduleButtonNode)
		{
			var rampRotationMilli = CalculateRampRotationMilli(slotNode);

			return new ShipUIModuleButton
			{
				UINode = moduleButtonNode.AsUiElement(),
				ModuleInfo = ParseModuleDetails(moduleButtonNode),
				SlotUINode = slotNode.AsUiElement(),
				IsActive = moduleButtonNode.UINode.DictEntriesOfInterest.GetValueOrDefault("ramp_active") as bool? ?? false,//TODO check
				IsHiliteVisible = slotNode.Children.Any(c=>c.NodeWithRegion.UiNode.PythonObjectTypeName == "hilite"),
				IsBusy = slotNode.Children.Any(c => c.NodeWithRegion.UiNode.PythonObjectTypeName == "busy"),
				RampRotationMilli = rampRotationMilli
			};
		}

		private static ModuleInfo? ParseModuleDetails(UITreeNodeWithDisplayRegion moduleButtonNode)
		{
			Console.WriteLine($"Trying to get module info for Id: {moduleButtonNode.UiNode.NameProperty}");
			return null;
		}

		private static int? CalculateRampRotationMilli(UITreeNodeWithDisplayRegion slotNode)
		{
			// Calculation logic for ramp rotation (simplified)
			return null;
		}

		private static ModuleButtonsRows GroupShipUIModulesIntoRows(ShipUICapacitor capacitor, List<ShipUIModuleButton> modules)
		{
			var verticalCenter = capacitor.UINode.GetVerticalCenter();

			var grouped= modules.GroupBy(m =>
			{
				var center = m.UINode.GetVerticalCenter();
				if (center < verticalCenter - 20) return "top";
				if (center > verticalCenter + 20) return "bottom";
				return "middle";
			})
				.ToDictionary(g => g.Key, g => g.ToList());
			return new ModuleButtonsRows()
			{
				Bottom = grouped.GetValueOrDefault("bottom", new List<ShipUIModuleButton>()),
				Middle = grouped.GetValueOrDefault("middle", new List<ShipUIModuleButton>()),
				Top = grouped.GetValueOrDefault("top", new List<ShipUIModuleButton>()),
			};
		}

		public static long GetVerticalCenter(this IUIElement uiTree)
		{
			return (uiTree.Region.Value.Min1 + uiTree.Region.Value.Max1) / 2;
		}
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
		public static IShipUiTarget[] ParseTargetsFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
		{
			return uiTreeRoot
				.ListDescendantsWithDisplayRegion()
				.Where(node => node.UINode.PythonObjectTypeName == "TargetInBar")
				.Select(ParseTarget)
				.ToArray();
		}

		public static IShipUiTarget ParseTarget(UITreeNodeWithDisplayRegion targetNode)
		{
			var textsTopToBottom = targetNode
				.GetAllContainedDisplayTextsWithRegion()
				.OrderBy(tuple => tuple.NodeWithRegion.TotalDisplayRegion.Y)
				.Select(tuple => tuple.DisplayText)
				.ToList();

			var barAndImageCont = targetNode
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UINode.GetNameFromDictEntries() == "barAndImageCont");

			var isActiveTarget = targetNode.UINode
				.ListDescendantsInUITreeNode()
				.Any(node => node.PythonObjectTypeName == "ActiveTargetOnBracket");

			var assignedContainerNode = targetNode
				.ListDescendantsWithDisplayRegion()
				.Where(node =>
				{
					var name = node.UINode.GetNameFromDictEntries()?.ToLower();
					return name != null && name.Contains("assigned");
				})
				.OrderBy(node => node.TotalDisplayRegion.Width)
				.FirstOrDefault();

			var assignedIcons = assignedContainerNode?
				.ListDescendantsWithDisplayRegion()
				.Where(node => new[] { "Sprite", "Icon" }.Contains(node.UINode.PythonObjectTypeName))
				.ToList() ?? new List<UITreeNodeWithDisplayRegion>();

			return new ShipUiTarget(targetNode.AsUiElement())
			{
				BarAndImageCont = barAndImageCont,
				TextsTopToBottom = textsTopToBottom,
				IsActiveTarget = isActiveTarget,
				AssignedContainerNode = assignedContainerNode,
				AssignedIcons = assignedIcons
			};
		}

		public static List<WindowOverView> ParseOverviewWindowsFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
		{
			return uiTreeRoot
				.ListDescendantsWithDisplayRegion()
				.Where(node =>
				{
					var typeName = node.UINode.PythonObjectTypeName;
					return new[] { "OverView", "OverviewWindow", "OverviewWindowOld" }.Contains(typeName);
				})
				.Select(ParseOverviewWindow)
				.ToList();
		}

		public static WindowOverView ParseOverviewWindow(UITreeNodeWithDisplayRegion overviewWindowNode)
		{
			var scrollNode = overviewWindowNode
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UINode.PythonObjectTypeName?.ToLower().Contains("scroll") == true);

			var scrollControlsNode = scrollNode?
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UINode.PythonObjectTypeName.Contains("ScrollControls"));

			var headersContainerNode = scrollNode?
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UINode.PythonObjectTypeName?.ToLower().Contains("headers") == true);

			var entriesHeaders = headersContainerNode?
				.GetAllContainedDisplayTextsWithRegion() ?? new List<(string Text, UITreeNodeWithDisplayRegion Region)>();

			var entries = overviewWindowNode
				.ListDescendantsWithDisplayRegion()
				.Where(node => node.UINode.PythonObjectTypeName == "OverviewScrollEntry")
				.Select(node => ParseOverviewWindowEntry(entriesHeaders, node))
				.ToList();

			return new WindowOverView
			{
				UINode = overviewWindowNode,
				EntriesHeaders = entriesHeaders,
				Entries = entries,
				ScrollControls = scrollControlsNode != null ? ParseScrollControls(scrollControlsNode) : null
			};
		}

		public static IOverviewEntry ParseOverviewWindowEntry(List<(string Text, UITreeNodeWithDisplayRegion Region)> entriesHeaders, UITreeNodeWithDisplayRegion overviewEntryNode)
		{
			var textsLeftToRight = overviewEntryNode
				.GetAllContainedDisplayTextsWithRegion()
				.OrderBy(tuple => tuple.NodeWithRegion.TotalDisplayRegion.X)
				.Select(tuple => tuple.DisplayText)
				.ToList();

			var listViewEntry = ParseListViewEntry(entriesHeaders, overviewEntryNode);

			var objectDistance = listViewEntry.CellsTexts.TryGetValue("Distance", out var distance) ? distance : null;

			var objectDistanceInMeters = objectDistance != null
				? ParseOverviewEntryDistanceInMetersFromText(objectDistance)
				: Result.Error<int>("Did not find the 'Distance' cell text.");

			var spaceObjectIconNode = overviewEntryNode
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UINode.PythonObjectTypeName == "SpaceObjectIcon");

			var iconSpriteColorPercent = spaceObjectIconNode?
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UINode.GetNameFromDictEntries() == "iconSprite")?
				.UINode.GetColorPercentFromDictEntries();

			var namesUnderSpaceObjectIcon = spaceObjectIconNode?
				.UINode.ListDescendantsInUITreeNode()
				.Select(node => node.GetNameFromDictEntries())
				.Where(name => name != null)
				.ToHashSet() ?? new HashSet<string>();

			var bgColorFillsPercent = overviewEntryNode
				.ListDescendantsWithDisplayRegion()
				.Where(node => node.UINode.PythonObjectTypeName == "Fill" && node.UINode.GetNameFromDictEntries() == "bgColor")
				.Select(node => node.UINode.GetColorPercentFromDictEntries())
				.Where(color => color != null)
				.ToList();

			var rightAlignedIconsHints = overviewEntryNode
				.ListDescendantsWithDisplayRegion()
				.Where(node => node.UINode.GetNameFromDictEntries() == "rightAlignedIconContainer")
				.SelectMany(node => node.ListDescendantsWithDisplayRegion())
				.Select(node => node.UINode.GetHintTextFromDictEntries())
				.Where(hint => hint != null)
				.ToList();

			return new OverviewEntry()
			{
				UINode = overviewEntryNode,
				TextsLeftToRight = textsLeftToRight,
				CellsTexts = listViewEntry.CellsTexts,
				ObjectDistance = objectDistance,
				ObjectDistanceInMeters = objectDistanceInMeters,
				ObjectName = listViewEntry.CellsTexts.TryGetValue("Name", out var name) ? name : null,
				ObjectType = listViewEntry.CellsTexts.TryGetValue("Type", out var type) ? type : null,
				ObjectAlliance = listViewEntry.CellsTexts.TryGetValue("Alliance", out var alliance) ? alliance : null,
				IconSpriteColorPercent = iconSpriteColorPercent,
				NamesUnderSpaceObjectIcon = namesUnderSpaceObjectIcon,
				BgColorFillsPercent = bgColorFillsPercent,
				RightAlignedIconsHints = rightAlignedIconsHints
			};
		}

		public static int? ParseOverviewEntryDistanceInMetersFromText(string distanceText)
		{
			var parts = distanceText.Trim().Split(' ').Reverse().ToArray();
			if (parts.Length < 2) return null;

			var unitText = parts[0];
			var numberText = string.Join(" ", parts.Skip(1).Reverse());

			var unitInMeters = unitText switch
			{
				"m" => 1,
				"km" => 1000,
				_ => (int?)null
			};

			if (unitInMeters == null) return null;

			if (float.TryParse(numberText, out var parsedNumber))
			{
				return (int)(parsedNumber * unitInMeters);
			}

			return null;
		}
		public static IWindowSelectedItemView? ParseSelectedItemWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
		{
			var windowNode = uiTreeRoot
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node => node.UINode.PythonObjectTypeName == "ActiveItem");

			return windowNode != null ? ParseSelectedItemWindow(windowNode) : null;
		}

		public static IWindowSelectedItemView ParseSelectedItemWindow(UITreeNodeWithDisplayRegion windowNode)
		{
			Func<string, UITreeNodeWithDisplayRegion?> actionButtonFromTexturePathEnding = texturePathEnding =>
				windowNode
					.ListDescendantsWithDisplayRegion()
					.FirstOrDefault(node =>
						node.UINode.GetTexturePathFromDictEntries()?.ToLower().EndsWith(texturePathEnding.ToLower()) == true);

			var orbitButton = actionButtonFromTexturePathEnding("44_32_21.png");

			return new WindowSelectedItemView()
			{
				UINode = windowNode,
				OrbitButton = orbitButton
			};
		}

		public static DronesWindow? ParseDronesWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
		{
			var windowNode = uiTreeRoot
				.ListDescendantsWithDisplayRegion()
				.FirstOrDefault(node =>
				{
					var typeName = node.UINode.PythonObjectTypeName;
					return new[] { "DroneView", "DronesWindow" }.Contains(typeName);
				});

			if (windowNode == null)
				return null;

			var droneGroupHeaders = windowNode
				.ListDescendantsWithDisplayRegion()
				.Where(node => node.UINode.PythonObjectTypeName.Contains("DroneGroupHeader"))
				.Select(ParseDronesWindowDroneGroupHeader)
				.Where(header => header != null)
				.ToList();

			var droneEntries = windowNode
				.ListDescendantsWithDisplayRegion()
				.Where(node =>
				{
					var typeName = node.UINode.PythonObjectTypeName;
					return typeName.StartsWith("Drone") && typeName.EndsWith("Entry");
				})
				.Select(ParseDronesWindowDroneEntry)
				.ToList();

			var droneGroups = CombineDroneGroups(droneEntries, droneGroupHeaders);

			Func<string, DronesWindowEntryGroupStructure?> droneGroupFromHeaderTextPart = headerTextPart =>
				droneGroups
					.Where(group =>
						group.Header.MainText?.ToLower().Contains(headerTextPart.ToLower()) == true)
					.OrderBy(group => group.Header.MainText?.Length ?? int.MaxValue)
					.FirstOrDefault();

			return new DronesWindow
			{
				UINode = windowNode,
				DroneGroups = droneGroups,
				DroneGroupInBay = droneGroupFromHeaderTextPart("in bay"),
				DroneGroupInSpace = droneGroupFromHeaderTextPart("in space")
			};
		}

		private static List<DronesWindowEntryGroupStructure> CombineDroneGroups(
			List<DronesWindowEntryDroneStructure> droneEntries,
			List<DronesWindowDroneGroupHeader> droneGroupHeaders)
		{
			var droneGroups = droneEntries.Select(entry => new DronesWindowEntryGroupStructure
			{
				Header = null,
				Children = new List<DronesWindowEntry> { new DronesWindowEntryDrone { Entry = entry } }
			}).ToList();

			droneGroups.AddRange(droneGroupHeaders.Select(header => new DronesWindowEntryGroupStructure
			{
				Header = header,
				Children = new List<DronesWindowEntry>()
			}));

			return DroneGroupTreesFromFlatList(droneGroups);
		}

		private static List<DronesWindowEntryGroupStructure> DroneGroupTreesFromFlatList(
			List<DronesWindowEntryGroupStructure> entries)
		{
			var orderedEntries = entries.OrderBy(entry =>
				entry.Header?.UINode.TotalDisplayRegion.Y ?? entry.Children.First().UINode.TotalDisplayRegion.Y).ToList();

			// Recursive tree-building logic can be added here if needed.
			return orderedEntries;
		}

		private static DronesWindowDroneGroupHeader? ParseDronesWindowDroneGroupHeader(UITreeNodeWithDisplayRegion groupHeaderNode)
		{
			var mainText = groupHeaderNode
				.GetAllContainedDisplayTextsWithRegion()
				.OrderByDescending(tuple => tuple.NodeWithRegion.TotalDisplayRegion.Area())
				.Select(tuple => tuple.DisplayText)
				.FirstOrDefault();

			if (mainText == null)
				return null;

			var quantityFromTitle = ParseQuantityFromDroneGroupTitleText(mainText);

			return new DronesWindowDroneGroupHeader
			{
				UINode = groupHeaderNode,
				MainText = mainText,
				QuantityFromTitle = quantityFromTitle
			};
		}

		private static DronesWindowDroneGroupHeaderQuantity? ParseQuantityFromDroneGroupTitleText(string titleText)
		{
			var parts = titleText.Split('(').Skip(1).ToArray();
			if (parts.Length == 0)
				return null;

			var textInParens = parts[0].Split(')').FirstOrDefault();
			if (textInParens == null)
				return null;

			var numbers = textInParens
				.Split('/')
				.Select(numberText => int.TryParse(numberText.Trim(), out var value) ? value : (int?)null)
				.ToList();

			if (numbers.Count == 1)
				return new DronesWindowDroneGroupHeaderQuantity { Current = numbers[0], Maximum = null };
			if (numbers.Count == 2)
				return new DronesWindowDroneGroupHeaderQuantity { Current = numbers[0], Maximum = numbers[1] };

			return null;
		}

		private static DronesWindowEntryDroneStructure ParseDronesWindowDroneEntry(UITreeNodeWithDisplayRegion droneEntryNode)
		{
			var mainText = droneEntryNode
				.GetAllContainedDisplayTextsWithRegion()
				.OrderByDescending(tuple => tuple.NodeWithRegion.TotalDisplayRegion.Area())
				.Select(tuple => tuple.DisplayText)
				.FirstOrDefault();

			return new DronesWindowEntryDroneStructure
			{
				UINode = droneEntryNode,
				MainText = mainText
			};
		}

		public class DronesWindow : IWindowDroneView
		{
			public UITreeNodeWithDisplayRegion UINode { get; set; }
			public List<DronesWindowEntryGroupStructure> DroneGroups { get; set; }
			public DronesWindowEntryGroupStructure? DroneGroupInBay { get; set; }
			public DronesWindowEntryGroupStructure? DroneGroupInSpace { get; set; }
		}

		public class DronesWindowEntryGroupStructure
		{
			public DronesWindowDroneGroupHeader? Header { get; set; }
			public List<DronesWindowEntry> Children { get; set; }
		}

		public class DronesWindowEntry { }
		public class DronesWindowEntryDrone : DronesWindowEntry
		{
			public DronesWindowEntryDroneStructure Entry { get; set; }
		}

		public class DronesWindowDroneGroupHeader
		{
			public UITreeNodeWithDisplayRegion UINode { get; set; }
			public string? MainText { get; set; }
			public DronesWindowDroneGroupHeaderQuantity? QuantityFromTitle { get; set; }
		}

		public class DronesWindowDroneGroupHeaderQuantity
		{
			public int? Current { get; set; }
			public int? Maximum { get; set; }
		}

		public class DronesWindowEntryDroneStructure
		{
			public UITreeNodeWithDisplayRegion UINode { get; set; }
			public string? MainText { get; set; }
		}
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

		public static ColorComponents? GetColorPercentFromDictEntries(this UITreeNode uiNode)
		{
			if (uiNode.DictEntriesOfInterest.TryGetValue("_color", out var value))
			{
				var serializedValue = JsonSerializer.Serialize(value);
				return JsonDecodeColorPercent(JToken.Parse(serializedValue));
			}
			return null;
		}

		public static double? GetRotationFloatFromDictEntries(this UITreeNode uiNode)
		{
			return uiNode.DictEntriesOfInterest.TryGetValue("_rotation", out var value) && value is float f
				? f
				: value is double d
					? d
					: null;
		}

		public static float? GetOpacityFloatFromDictEntries(this UITreeNode uiNode)
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
		public IWindowDroneView? WindowDroneView { get; init; }
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
