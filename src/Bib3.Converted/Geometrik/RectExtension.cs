// Decompiled with JetBrains decompiler
// Type: Bib3.Geometrik.RectExtension
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3.Geometrik
{
  public static class RectExtension
  {
    public static Vektor2DInt MinPoint(this RectInt rect) => new Vektor2DInt(rect.Min0, rect.Min1);

    public static Vektor2DInt MaxPoint(this RectInt rect) => new Vektor2DInt(rect.Max0, rect.Max1);

    public static long Side0Length(this RectInt rect) => rect.Max0 - rect.Min0;

    public static long Side1Length(this RectInt rect) => rect.Max1 - rect.Min1;

    public static long Area(this RectInt rect) => (rect.Max0 - rect.Min0) * (rect.Max1 - rect.Min1);

    public static Vektor2DInt Center(this RectInt rect) => new Vektor2DInt((rect.Min0 + rect.Max0) / 2L, (rect.Min1 + rect.Max1) / 2L);

    public static Vektor2DInt Size(this RectInt rect) => new Vektor2DInt(rect.Max0 - rect.Min0, rect.Max1 - rect.Min1);

    public static bool IsEmpty(this RectInt rect) => rect.Area() == 0L;

    public static bool ContainsPointForMinInclusiveAndMaxExclusive(
      this RectInt rect,
      Vektor2DInt point)
    {
      return rect.Min0 <= point.A && rect.Min1 <= point.B && point.A < rect.Max0 && point.B < rect.Max1;
    }

    public static bool ContainsPointForMinInclusiveAndMaxInclusive(
      this RectInt rect,
      Vektor2DInt point)
    {
      return rect.Min0 <= point.A && rect.Min1 <= point.B && point.A <= rect.Max0 && point.B <= rect.Max1;
    }
  }
}
