// Decompiled with JetBrains decompiler
// Type: Bib3.Geometrik.Vektor2DDouble
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3.Geometrik
{
  public struct Vektor2DDouble
  {
    public double A;
    public double B;

    public Vektor2DDouble(double a, double b)
    {
      this.A = a;
      this.B = b;
    }

    public static Vektor2DDouble operator -(Vektor2DDouble minuend, Vektor2DDouble subtrahend) => new Vektor2DDouble(minuend.A - subtrahend.A, minuend.B - subtrahend.B);

    public static Vektor2DDouble operator -(Vektor2DDouble subtrahend) => new Vektor2DDouble(0.0, 0.0) - subtrahend;

    public static Vektor2DDouble operator +(Vektor2DDouble vektor0, Vektor2DDouble vektor1) => new Vektor2DDouble(vektor0.A + vektor1.A, vektor0.B + vektor1.B);

    public static Vektor2DDouble operator /(Vektor2DDouble dividend, double divisor) => new Vektor2DDouble(dividend.A / divisor, dividend.B / divisor);

    public static Vektor2DDouble operator *(Vektor2DDouble vektor0, double faktor) => new Vektor2DDouble(vektor0.A * faktor, vektor0.B * faktor);

    public static Vektor2DDouble operator *(double faktor, Vektor2DDouble vektor0) => new Vektor2DDouble(vektor0.A * faktor, vektor0.B * faktor);

    public static bool operator ==(Vektor2DDouble vektor0, Vektor2DDouble vektor1) => vektor0.A == vektor1.A && vektor0.B == vektor1.B;

    public static bool operator !=(Vektor2DDouble vektor0, Vektor2DDouble vektor1) => !(vektor0 == vektor1);

    public override int GetHashCode() => this.A.GetHashCode() ^ this.B.GetHashCode();

    public override string ToString() => "{ A:" + this.A.ToString() + ", B:" + this.B.ToString() + "}";
  }
}
