// Decompiled with JetBrains decompiler
// Type: Bib3.Geometrik.VektorExtension
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;

namespace Bib3.Geometrik
{
  public static class VektorExtension
  {
    public static long LengthSquared(this Vektor2DInt vektor) => vektor.A * vektor.A + vektor.B * vektor.B;

    public static long Length(this Vektor2DInt vektor) => (long) Math.Sqrt((double) vektor.LengthSquared());

    public static double LengthDouble(this Vektor2DInt vektor) => Math.Sqrt((double) vektor.LengthSquared());

    public static double LengthSquared(this Vektor2DDouble vektor) => vektor.A * vektor.A + vektor.B * vektor.B;

    public static double Length(this Vektor2DDouble vektor) => Math.Sqrt(vektor.LengthSquared());

    public static double Skalarprodukt(this Vektor2DInt vektor0, Vektor2DInt vektor1) => (double) (vektor0.A * vektor1.A + vektor0.B * vektor1.B);

    public static double Skalarprodukt(this Vektor2DDouble vektor0, Vektor2DDouble vektor1) => vektor0.A * vektor1.A + vektor0.B * vektor1.B;

    public static double Kroizprodukt(this Vektor2DDouble vektor0, Vektor2DDouble vektor1) => vektor0.A * vektor1.B - vektor0.B * vektor1.A;

    public static RectInt EnclosingRectangle(this IEnumerable<Vektor2DInt> setPoint)
    {
      long num1 = long.MaxValue;
      long num2 = long.MaxValue;
      long num3 = long.MinValue;
      long num4 = long.MinValue;
      foreach (Vektor2DInt vektor2Dint in setPoint.EmptyIfNull<Vektor2DInt>())
      {
        num1 = Math.Min(num1, vektor2Dint.A);
        num2 = Math.Min(num2, vektor2Dint.B);
        num3 = Math.Max(num3, vektor2Dint.A);
        num4 = Math.Max(num4, vektor2Dint.B);
      }
      return new RectInt(num1, num2, num3, num4);
    }

    public static Vektor2DInt WithA(this Vektor2DInt vektor, long a) => new Vektor2DInt(a, vektor.B);

    public static Vektor2DInt WithB(this Vektor2DInt vektor, long b) => new Vektor2DInt(vektor.A, b);
  }
}
