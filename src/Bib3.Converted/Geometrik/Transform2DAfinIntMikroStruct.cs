// Decompiled with JetBrains decompiler
// Type: Bib3.Geometrik.Transform2DAfinIntMikroStruct
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;

namespace Bib3.Geometrik
{
  public struct Transform2DAfinIntMikroStruct : ITransform2D
  {
    public readonly long m00Mikro;
    public readonly long m01Mikro;
    public readonly long VersazAMikro;
    public readonly long m10Mikro;
    public readonly long m11Mikro;
    public readonly long VersazBMikro;
    public const long Mega = 1000000;
    public const long Tera = 1000000000000;
    public static Transform2DAfinIntMikroStruct Noitraal = new Transform2DAfinIntMikroStruct(1000000L, 0L, 0L, 1000000L, 0L, 0L);

    public static Transform2DAfinIntMikroStruct TransformVersazMikro(Vektor2DInt versazMikro) => Transform2DAfinIntMikroStruct.TransformVersazMikro(versazMikro.A, versazMikro.B);

    public static Transform2DAfinIntMikroStruct TransformVersazMikro(
      long versazAMikro,
      long versazBMikro)
    {
      return new Transform2DAfinIntMikroStruct(1000000L, 0L, 0L, 1000000L, versazAMikro, versazBMikro);
    }

    public static Transform2DAfinIntMikroStruct TransformSkalMikro(long skalAMikro, long skalBMikro) => new Transform2DAfinIntMikroStruct(skalAMikro, 0L, 0L, skalBMikro, 0L, 0L);

    public static Transform2DAfinIntMikroStruct TransformRotMikro(long rotationMikro)
    {
      double num1 = (double) rotationMikro * Math.PI / 500000.0;
      long m01Mikro = (long) (Math.Sin(num1) * 1000000.0);
      long num2 = (long) (Math.Cos(num1) * 1000000.0);
      return new Transform2DAfinIntMikroStruct(num2, m01Mikro, -m01Mikro, num2, 0L, 0L);
    }

    public Transform2DAfinIntMikroStruct(
      long m00Mikro,
      long m01Mikro,
      long m10Mikro,
      long m11Mikro,
      long versazAMikro,
      long versazBMikro)
    {
      this.m00Mikro = m00Mikro;
      this.m01Mikro = m01Mikro;
      this.m10Mikro = m10Mikro;
      this.m11Mikro = m11Mikro;
      this.VersazAMikro = versazAMikro;
      this.VersazBMikro = versazBMikro;
    }

    public Vektor2DInt Transform(Vektor2DInt punkt) => new Vektor2DInt((punkt.A * this.m00Mikro + punkt.B * this.m10Mikro + this.VersazAMikro) / 1000000L, (punkt.A * this.m01Mikro + punkt.B * this.m11Mikro + this.VersazBMikro) / 1000000L);

    public Vektor2DInt AntailVersazMikro() => new Vektor2DInt(this.VersazAMikro, this.VersazBMikro);

    public long DeterminantMikro => (this.m00Mikro * this.m11Mikro - this.m01Mikro * this.m10Mikro) / 1000000L;

    public bool HatInvers => this.DeterminantMikro != 0L;

    public Transform2DAfinIntMikroStruct Invers()
    {
      long num = 1000000000000L / this.DeterminantMikro;
      return new Transform2DAfinIntMikroStruct(this.m11Mikro * num / 1000000L, -this.m01Mikro * num / 1000000L, -this.m10Mikro * num / 1000000L, this.m00Mikro * num / 1000000L, (this.m10Mikro * this.VersazBMikro - this.VersazAMikro * this.m11Mikro) / 1000000L * num / 1000000L, (this.VersazAMikro * this.m01Mikro - this.m00Mikro * this.VersazBMikro) / 1000000L * num / 1000000L);
    }

    public static Transform2DAfinIntMikroStruct Produkt(
      Transform2DAfinIntMikroStruct transform0,
      Transform2DAfinIntMikroStruct transform1)
    {
      return new Transform2DAfinIntMikroStruct((transform0.m00Mikro * transform1.m00Mikro + transform0.m01Mikro * transform1.m10Mikro) / 1000000L, (transform0.m00Mikro * transform1.m01Mikro + transform0.m01Mikro * transform1.m11Mikro) / 1000000L, (transform0.m10Mikro * transform1.m00Mikro + transform0.m11Mikro * transform1.m10Mikro) / 1000000L, (transform0.m10Mikro * transform1.m01Mikro + transform0.m11Mikro * transform1.m11Mikro) / 1000000L, (transform0.VersazAMikro * transform1.m00Mikro + transform0.VersazBMikro * transform1.m10Mikro) / 1000000L + transform1.VersazAMikro, (transform0.VersazAMikro * transform1.m01Mikro + transform0.VersazBMikro * transform1.m11Mikro) / 1000000L + transform1.VersazBMikro);
    }
  }
}
