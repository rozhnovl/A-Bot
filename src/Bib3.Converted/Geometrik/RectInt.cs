// Decompiled with JetBrains decompiler
// Type: Bib3.Geometrik.RectInt
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3.Geometrik
{
  public struct RectInt
  {
    public long Min0;
    public long Min1;
    public long Max0;
    public long Max1;

    public static RectInt Empty => new RectInt(0L, 0L, 0L, 0L);

    public static RectInt EmptyMin => new RectInt(long.MinValue, long.MinValue, long.MinValue, long.MinValue);

    public RectInt(long min0, long min1, long max0, long max1)
    {
      this.Min0 = min0;
      this.Min1 = min1;
      this.Max0 = max0;
      this.Max1 = max1;
    }

    public RectInt(RectInt zuKopiirende)
      : this(zuKopiirende.Min0, zuKopiirende.Min1, zuKopiirende.Max0, zuKopiirende.Max1)
    {
    }

    public static RectInt FromMinPointAndMaxPoint(Vektor2DInt minPoint, Vektor2DInt maxPoint) => new RectInt(minPoint.A, minPoint.B, maxPoint.A, maxPoint.B);

    public static RectInt FromCenterAndSize(Vektor2DInt center, Vektor2DInt size) => RectInt.FromMinPointAndMaxPoint(center - size / 2L, center + (size + new Vektor2DInt(1L, 1L)) / 2L);

    public static RectInt operator -(RectInt minuend, Vektor2DInt subtrahend) => minuend.Offset(-subtrahend);

    public static RectInt operator +(RectInt sumand0, Vektor2DInt sumand1) => sumand0.Offset(sumand1);

    public static RectInt operator *(RectInt faktor0, long faktor1) => new RectInt(faktor0.Min0 * faktor1, faktor0.Min1 * faktor1, faktor0.Max0 * faktor1, faktor0.Max1 * faktor1);

    public static bool operator ==(RectInt o0, RectInt o1) => o0.Min0 == o1.Min0 && o0.Min1 == o1.Min1 && o0.Max0 == o1.Max0 && o0.Max1 == o1.Max1;

    public override bool Equals(object obj)
    {
      RectInt? nullable = obj.CastToNullable<RectInt>();
      return nullable.HasValue && nullable.Value == this;
    }

    public override int GetHashCode() => (this.Min0 + this.Min1 + this.Max0 + this.Max1).GetHashCode();

    public static bool operator !=(RectInt vektor0, RectInt vektor1) => !(vektor0 == vektor1);

    public static string ComponentToString(long? komponente) => Vektor2DInt.ComponentToString(komponente);

    public override string ToString() => "Min0 = " + RectInt.ComponentToString(new long?(this.Min0)) + ", Min1 = " + RectInt.ComponentToString(new long?(this.Min1)) + ", Max0 = " + RectInt.ComponentToString(new long?(this.Max0)) + ", Max1 = " + RectInt.ComponentToString(new long?(this.Max1));
  }
}
