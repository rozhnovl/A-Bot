// Decompiled with JetBrains decompiler
// Type: Bib3.Geometrik.Vektor2DInt
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Globalization;
using System.Linq;

namespace Bib3.Geometrik
{
  public struct Vektor2DInt
  {
    public long A;
    public long B;
    private static NumberFormatInfo ComponentNumberFormat = Vektor2DInt.ComponentNumberFormatConstruct();

    public Vektor2DInt(long a, long b)
    {
      this.A = a;
      this.B = b;
    }

    public Vektor2DInt(Vektor2DInt zuKopiirende)
      : this(zuKopiirende.A, zuKopiirende.B)
    {
    }

    public static Vektor2DInt operator -(Vektor2DInt minuend, Vektor2DInt subtrahend) => new Vektor2DInt(minuend.A - subtrahend.A, minuend.B - subtrahend.B);

    public static Vektor2DInt operator -(Vektor2DInt subtrahend) => new Vektor2DInt(0L, 0L) - subtrahend;

    public static Vektor2DInt operator +(Vektor2DInt vektor0, Vektor2DInt vektor1) => new Vektor2DInt(vektor0.A + vektor1.A, vektor0.B + vektor1.B);

    public static Vektor2DInt operator /(Vektor2DInt dividend, long divisor) => new Vektor2DInt(dividend.A / divisor, dividend.B / divisor);

    public static Vektor2DInt operator *(Vektor2DInt vektor0, long faktor) => new Vektor2DInt(vektor0.A * faktor, vektor0.B * faktor);

    public static Vektor2DInt operator *(long faktor, Vektor2DInt vektor0) => new Vektor2DInt(vektor0.A * faktor, vektor0.B * faktor);

    public static bool operator ==(Vektor2DInt vektor0, Vektor2DInt vektor1) => vektor0.A == vektor1.A && vektor0.B == vektor1.B;

    public static bool operator !=(Vektor2DInt vektor0, Vektor2DInt vektor1) => !(vektor0 == vektor1);

    private static NumberFormatInfo ComponentNumberFormatConstruct()
    {
      NumberFormatInfo numberFormatInfo = (NumberFormatInfo) NumberFormatInfo.InvariantInfo.Clone();
      numberFormatInfo.NumberGroupSeparator = ".";
      numberFormatInfo.NumberGroupSizes = Enumerable.Range(0, 10).Select<int, int>((Func<int, int>) (t => 3)).ToArray<int>();
      numberFormatInfo.NumberDecimalDigits = 0;
      return numberFormatInfo;
    }

    public static string ComponentToString(long? component) => !component.HasValue ? (string) null : Vektor2DInt.ComponentToString(component.Value);

    public static string ComponentToString(long component) => component == 0L ? "0" : component.ToString("### ### ### ### ### ### ### ### ###").Trim();

    public override int GetHashCode() => this.A.GetHashCode() ^ this.B.GetHashCode();

    public override string ToString() => "A = " + Vektor2DInt.ComponentToString(this.A) + ", B = " + Vektor2DInt.ComponentToString(this.B);
  }
}
