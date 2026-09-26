using BotEngine.Common;
using BotEngine.Motor;
using System;
using System.Collections.Generic;
using System.Linq;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using Bib3;
using BotEngine.Windows;
using Sanderling; // SubstractionRemainder / GetOccludedUIElementRemainingRegion

namespace Sanderling.Motor
{
	static public class Extension
	{
		public const int MotionMouseWaypointSafetyMarginMin = 2;

		public const int MotionMouseWaypointSafetyMarginAdditional = 0;

		static public IEnumerable<IUIElement> EnumerateSetElementExcludedFromOcclusion(this IMemoryMeasurement memoryMeasurement) => new[]
			{
				memoryMeasurement?.ModuleButtonTooltip,
			}.WhereNotDefault();

		static public IEnumerable<Motion> AsSequenceMotion(
			this MotionParam motion,
			IMemoryMeasurement memoryMeasurement)
		{
			if (null == motion)
			{
				yield break;
			}

			if (motion?.WindowToForeground ?? false)
				yield return new Motion(null, windowToForeground: true);

			var SetElementExcludedFromOcclusion = memoryMeasurement?.EnumerateSetElementExcludedFromOcclusion()?.ToArray();

			var Random = new Random((int)Bib3.Glob.StopwatchZaitMiliSictInt());

			var MouseListWaypoint = motion?.MouseListWaypoint;

			var mouseButtonDownMotion = new Motion(null, motion?.MouseButton);
			var mouseButtonUpMotion = new Motion(null, null, motion?.MouseButton);

			for (int WaypointIndex = 0; WaypointIndex < (MouseListWaypoint?.Length ?? 0); WaypointIndex++)
			{
				var mouseWaypoint = MouseListWaypoint[WaypointIndex];

				var waypointUIElement = mouseWaypoint?.UIElement;

				waypointUIElement = (waypointUIElement as Accumulation.IRepresentingMemoryObject)?.RepresentedMemoryObject as UIElement ?? waypointUIElement;

				//todo
				var waypointUIElementCurrent = waypointUIElement;//waypointUIElement.GetInstanceWithIdFromCLRGraph(memoryMeasurement, Interface.FromInterfaceResponse.SerialisPolicyCache);

				if (null == waypointUIElementCurrent)
					throw new ApplicationException("mouse waypoint not anymore contained in UITree");

				var waypointRegion = waypointUIElementCurrent.RegionInteraction?.Region 
					?? waypointUIElementCurrent.Region;

				waypointRegion = mouseWaypoint.RegionReplacementAbsolute ?? waypointRegion;

				if (!waypointRegion.HasValue)
					throw new ArgumentException("Did not find a region for the waypoint.");

				// Capture the target's draw-order frontier before WithRegion (windows with a larger
				// index are drawn IN FRONT and therefore occlude it).
				var targetFrontier = waypointUIElementCurrent.ChildLastInTreeIndex
				                     ?? waypointUIElementCurrent.InTreeIndex;

				waypointUIElementCurrent = waypointUIElementCurrent.WithRegion(waypointRegion.Value);

				RectInt[] WaypointRegionPortionVisible = null;
				if (memoryMeasurement is IOcclusionModel occlusionModel && targetFrontier.HasValue)
				{
					//	Native occlusion for the Eve64 model: subtract opaque windows drawn in front of the
					//	target (higher tree index). No reflection walk — that one throws on this model.
					var occluderRegions = occlusionModel.WindowRegionsForOcclusion
						?.Where(w => w?.Region != null && (w.InTreeIndex ?? -1) > targetFrontier.Value)
						?.Select(w => w.Region.Value)
						?.ToArray() ?? System.Array.Empty<RectInt>();

					if (0 < occluderRegions.Length)
						WaypointRegionPortionVisible =
							waypointRegion.Value.SubstractionRemainder(occluderRegions)
							?.Select(portionVisible => portionVisible.WithSizeExpandedPivotAtCenter(-MotionMouseWaypointSafetyMarginMin * 2))
							?.Where(portionVisible => !portionVisible.IsEmpty())
							?.ToArray();
				}
				else
				{
					try
					{
						WaypointRegionPortionVisible =
							waypointUIElementCurrent.GetOccludedUIElementRemainingRegion(
								memoryMeasurement,
								c => SetElementExcludedFromOcclusion?.Contains(c) ?? false)
							?.Select(portionVisible => portionVisible.WithSizeExpandedPivotAtCenter(-MotionMouseWaypointSafetyMarginMin * 2))
							?.Where(portionVisible => !portionVisible.IsEmpty())
							?.ToArray();
					}
					catch { WaypointRegionPortionVisible = null; }
				}

				//	Native occlusion produced an EMPTY remainder => a window fully covers the target.
				//	Do NOT fall back to clicking its hidden region (that lands on the covering window and
				//	is exactly the "easy to break something" case). Skip the click this tick.
				if (memoryMeasurement is IOcclusionModel && targetFrontier.HasValue &&
				    WaypointRegionPortionVisible != null && WaypointRegionPortionVisible.Length == 0)
					yield break;

				var WaypointRegionPortionVisibleLargestPatch =
					WaypointRegionPortionVisible
					?.OrderByDescending(patch => Math.Min(patch.Side0Length(), patch.Side1Length()))
					?.FirstOrDefault();

				//	Fall back to the whole waypoint region when occlusion produced nothing usable,
				//	rather than aborting the click entirely.
				if (!(0 < WaypointRegionPortionVisibleLargestPatch?.Side0Length() &&
					0 < WaypointRegionPortionVisibleLargestPatch?.Side1Length()))
				{
					WaypointRegionPortionVisibleLargestPatch = waypointRegion.Value;
				}

				var Point =
					WaypointRegionPortionVisibleLargestPatch.Value
					.WithSizeExpandedPivotAtCenter(-MotionMouseWaypointSafetyMarginAdditional * 2)
					.RandomPointInRectangle(Random);

				yield return new Motion(Point);

				if (0 == WaypointIndex)
				{
					yield return mouseButtonDownMotion;

					for (int repetitionIndex = 0; repetitionIndex < motion?.MouseButtonRepetitionCount; repetitionIndex++)
					{
						yield return mouseButtonUpMotion;
						yield return mouseButtonDownMotion;
					}
				}
			}

			yield return mouseButtonUpMotion;

			var MotionKeyDown = motion?.KeyDown;
			var MotionKeyUp = motion?.KeyUp;

			if (null != MotionKeyDown)
			{
				yield return new Motion(null, keyDown: MotionKeyDown);
			}

			if (null != MotionKeyUp)
			{
				yield return new Motion(null, keyUp: MotionKeyUp);
			}

			var MotionTextEntry = motion?.TextEntry;

			if (0 < MotionTextEntry?.Length)
				yield return new Motion(null, textEntry: MotionTextEntry);
		}

		static public Vektor2DInt? ClientToScreen(this IntPtr hWnd, Vektor2DInt locationInClient)
		{
			var structWinApi = locationInClient.AsWindowsPoint();

			if (!BotEngine.WinApi.User32.ClientToScreen(hWnd, ref structWinApi))
				return null;

			return structWinApi.AsVektor2DInt();
		}
	}
}
