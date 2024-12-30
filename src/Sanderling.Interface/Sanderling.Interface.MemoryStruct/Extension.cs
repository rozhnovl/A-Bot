using Bib3;
using Bib3.Geometrik;
using Bib3.RefNezDiferenz;
using BotEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Bib3.RefBaumKopii;

namespace Sanderling.Interface.MemoryStruct
{
	public static class Extension
	{
		public static IObjectIdInMemory AsObjectIdInMemory(this long id)
		{
			return new ObjectIdInMemory(new ObjectIdInt64(id));
		}

		public static T Largest<T>(this IEnumerable<T> source) where T : class, IUIElement
		{
			IOrderedEnumerable<T> orderedEnumerable = from item in source
			orderby item?.Region?.Area() ?? (-1) descending
			select item;
			return (orderedEnumerable != null) ? orderedEnumerable.FirstOrDefault() : null;
		}

		public static IEnumerable<object> EnumerateReferencedTransitive(this object parent)
		{
			return parent.EnumMengeRefAusNezAusWurzel(FromInterfaceResponse.UITreeComponentTypeHandlePolicyCache);
		}

		public static IEnumerable<IUIElement> EnumerateReferencedUIElementTransitive(this object parent)
		{
			return parent.EnumerateReferencedTransitive()?.OfType<IUIElement>();
		}

		public static T CopyByPolicyMemoryMeasurement<T>(this T toBeCopied) where T : class
		{
			return RefBaumKopiiStatic.ObjektKopiiErsctele(toBeCopied, new Param(null, FromInterfaceResponse.SerialisPolicyCache));
		}

		public static IUIElement WithRegion(this IUIElement @base, RectInt region)
		{
			return (@base == null) ? null : new UIElement(@base)
			{
				Region = region
			};
		}
		
		public static IUIElement WithRegionSizePivotAtCenter(this IUIElement @base, Vektor2DInt regionSize)
		{
			return @base?.WithRegion(@base.Region.Value.WithSizePivotAtCenter(regionSize));
		}
		
		public static IUIElement WithRegionSizeBoundedMaxPivotAtCenter(this IUIElement @base, Vektor2DInt regionSizeMax)
		{
			throw new NotImplementedException();
			//return @base?.WithRegion(@base.Region.WithSizeBoundedMaxPivotAtCenter(regionSizeMax));
		}

		public static Vektor2DInt? RegionCenter(this IUIElement uiElement)
		{
			return (uiElement?.Region)?.Center();
		}
		/*
		public static Vektor2DInt? RegionSize(this IUIElement uiElement)
		{
			return (uiElement?.Region)?.Size();
		}

		public static Vektor2DInt? RegionCornerLeftTop(this IUIElement uiElement)
		{
			return uiElement?.Region.MinPoint();
		}

		public static Vektor2DInt? RegionCornerRightBottom(this IUIElement uiElement)
		{
			return uiElement?.Region.MaxPoint();
		}
		*/
		public static IEnumerable<ITreeViewEntry> EnumerateChildNodeTransitive(this ITreeViewEntry treeViewEntry)
		{
			return treeViewEntry?.EnumerateNodeFromTreeBFirst((ITreeViewEntry node) => node.Child);
		}

		public static IEnumerable<T> OrderByCenterDistanceToPoint<T>(this IEnumerable<T> sequence, Vektor2DInt point) where T : IUIElement
		{
			return sequence?.OrderBy(delegate(T element)
			{
				Vektor2DInt value = point;
				Vektor2DInt? subtrahend = (element != null) ? element.RegionCenter() : null;
				return (value - subtrahend)?.LengthSquared() ?? long.MaxValue;
			});
		}

		public static IEnumerable<T> OrderByCenterVerticalDown<T>(this IEnumerable<T> source) where T : IUIElement
		{
			return source?.OrderBy((T element) => ((element == null) ? null : element.RegionCenter()?.B) ?? int.MaxValue);
		}

		public static IEnumerable<T> OrderByNearestPointOnLine<T>(this IEnumerable<T> sequence, Vektor2DInt lineVector, Func<T, Vektor2DInt?> getPointRepresentingElement)
		{
			long num = lineVector.Length();
			if (getPointRepresentingElement == null || num < 1)
			{
				return sequence;
			}
			Vektor2DInt LineVectorNormalizedMilli = lineVector * 1000L / num;
			return sequence?.Select(delegate(T element)
			{
				long? locationOnLine = null;
				Vektor2DInt? vektor2DInt = getPointRepresentingElement(element);
				if (vektor2DInt.HasValue)
				{
					locationOnLine = vektor2DInt.Value.A * LineVectorNormalizedMilli.A + vektor2DInt.Value.B * LineVectorNormalizedMilli.B;
				}
				return new
				{
					Element = element,
					LocationOnLine = locationOnLine
				};
			})?.OrderBy(elementAndLocation => elementAndLocation.LocationOnLine)?.Select(elementAndLocation => elementAndLocation.Element);
		}
	}
}
