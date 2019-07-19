using Bib3;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BotEngine.Windows
{
	public struct RECT
	{
		private int _left;

		private int _top;

		private int _right;

		private int _bottom;

		public int X
		{
			get
			{
				return Left;
			}
			set
			{
				Left = value;
			}
		}

		public int Y
		{
			get
			{
				return Top;
			}
			set
			{
				Top = value;
			}
		}

		public int Left
		{
			get
			{
				return _left;
			}
			set
			{
				_left = value;
			}
		}

		public int Top
		{
			get
			{
				return _top;
			}
			set
			{
				_top = value;
			}
		}

		public int Right
		{
			get
			{
				return _right;
			}
			set
			{
				_right = value;
			}
		}

		public int Bottom
		{
			get
			{
				return _bottom;
			}
			set
			{
				_bottom = value;
			}
		}

		public int Height
		{
			get
			{
				return Bottom - Top;
			}
			set
			{
				Bottom = value - Top;
			}
		}

		public int Width
		{
			get
			{
				return Right - Left;
			}
			set
			{
				Right = value + Left;
			}
		}

		public POINT LeftTop => new POINT(Left, Top);

		public POINT RightBottom => new POINT(Right, Bottom);

		public RECT(int left, int top, int right, int bottom)
		{
			_left = left;
			_top = top;
			_right = right;
			_bottom = bottom;
		}

		public static bool operator ==(RECT rectangle1, RECT rectangle2)
		{
			return rectangle1.Equals(rectangle2);
		}

		public static bool operator !=(RECT rectangle1, RECT rectangle2)
		{
			return !rectangle1.Equals(rectangle2);
		}

		public override string ToString()
		{
			return "{Left: " + Left + "; Top: " + Top + "; Right: " + Right + "; Bottom: " + Bottom + "}";
		}

		public bool Equals(RECT rectangle)
		{
			return rectangle.Left == Left && rectangle.Top == Top && rectangle.Right == Right && rectangle.Bottom == Bottom;
		}

		public override bool Equals(object @object)
		{
			if (@object is RECT)
			{
				return Equals((RECT)@object);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Left.GetHashCode() ^ Right.GetHashCode() ^ Top.GetHashCode() ^ Bottom.GetHashCode();
		}

		public static RECT Scnitmenge(RECT rect0, RECT rect1)
		{
			return Scnitmenge(new RECT[2]
			{
				rect0,
				rect1
			});
		}

		public static RECT Scnitmenge(IEnumerable<RECT> mengeRect)
		{
			if (mengeRect.IsNullOrEmpty())
			{
				return default(RECT);
			}
			RECT result = mengeRect?.FirstOrDefault() ?? default(RECT);
			foreach (RECT item in mengeRect)
			{
				result.Left = Math.Max(result.Left, item.Left);
				result.Top = Math.Max(result.Top, item.Top);
				result.Right = Math.Min(result.Right, item.Right);
				result.Bottom = Math.Min(result.Bottom, item.Bottom);
			}
			return result;
		}

		public RECT Versezt(POINT point)
		{
			return Versezt(point.x, point.y);
		}

		public RECT Versezt(int x, int y)
		{
			return new RECT(Left + x, Top + y, Right + x, Bottom + y);
		}
	}
}
