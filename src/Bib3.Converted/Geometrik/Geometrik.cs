// Decompiled with JetBrains decompiler
// Type: Bib3.Geometrik.Geometrik
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib3.Geometrik
{
  public static class Geometrik
  {
    public static Vektor2DDouble AsVektor2DDouble(this Vektor2DInt vektor) => new Vektor2DDouble((double) vektor.A, (double) vektor.B);

    public static Vektor2DInt AsVektor2DInt(this Vektor2DDouble vektor) => new Vektor2DInt((long) vektor.A, (long) vektor.B);

    public static Vektor2DDouble Normalized(this Vektor2DDouble vektor)
    {
      double num = vektor.Length();
      return new Vektor2DDouble(vektor.A / num, vektor.B / num);
    }

    public static RectInt Offset(this RectInt ortogoon, Vektor2DInt offset) => new RectInt(ortogoon.Min0 + offset.A, ortogoon.Min1 + offset.B, ortogoon.Max0 + offset.A, ortogoon.Max1 + offset.B);

    public static RectInt WithSizePivotAtCenter(this RectInt ortogoon, Vektor2DInt size) => RectInt.FromCenterAndSize(ortogoon.Center(), size);

    public static RectInt WithSizeBoundedMaxPivotAtCenter(
      this RectInt ortogoon,
      Vektor2DInt sizeMax)
    {
      return ortogoon.WithSizePivotAtCenter(new Vektor2DInt(Math.Min(sizeMax.A, ortogoon.Side0Length()), Math.Min(sizeMax.B, ortogoon.Side1Length())));
    }

    public static RectInt WithSizeExpandedPivotAtCenter(
      this RectInt ortogoon,
      Vektor2DInt expansion)
    {
      return ortogoon.WithSizePivotAtCenter(ortogoon.Size() + expansion);
    }

    public static RectInt WithSizeExpandedPivotAtCenter(this RectInt ortogoon, int expansion) => ortogoon.WithSizeExpandedPivotAtCenter(new Vektor2DInt((long) expansion, (long) expansion));

    public static RectInt Intersection(this RectInt o0, RectInt o1)
    {
      long num1 = Math.Min(o0.Max0, Math.Max(o0.Min0, o1.Min0));
      long num2 = Math.Min(o0.Max1, Math.Max(o0.Min1, o1.Min1));
      return new RectInt(num1, num2, Math.Max(num1, Math.Min(o0.Max0, o1.Max0)), Math.Max(num2, Math.Min(o0.Max1, o1.Max1)));
    }

    public static IEnumerable<Vektor2DInt> ListCornerLocation(this RectInt ortogoon)
    {
      for (int EkeIndex = 0; EkeIndex < 4; ++EkeIndex)
      {
        bool RictungAGrenzeIndex = 1 == (EkeIndex + 1) / 2 % 2;
        bool RictungBGrenzeIndex = 1 == EkeIndex / 2 % 2;
        yield return new Vektor2DInt(RictungAGrenzeIndex ? ortogoon.Max0 : ortogoon.Min0, RictungBGrenzeIndex ? ortogoon.Max1 : ortogoon.Min1);
      }
    }

    public static RectInt BoundingRectangle(this IEnumerable<Vektor2DInt> mengePunkt)
    {
      long num1 = long.MaxValue;
      long num2 = long.MaxValue;
      long num3 = long.MinValue;
      long num4 = long.MinValue;
      if (mengePunkt != null)
      {
        foreach (Vektor2DInt vektor2Dint in mengePunkt)
        {
          num1 = Math.Min(num1, vektor2Dint.A);
          num2 = Math.Min(num2, vektor2Dint.B);
          num3 = Math.Max(num3, vektor2Dint.A);
          num4 = Math.Max(num4, vektor2Dint.B);
        }
      }
      return new RectInt(num1, num2, num3, num4);
    }

    public static bool ScnaidendGeraadeSegmentMitGeraadeSegment(
      Vektor2DDouble geraadeSegment0Begin,
      Vektor2DDouble geraadeSegment0Ende,
      Vektor2DDouble geraadeSegment1Begin,
      Vektor2DDouble geraadeSegment1Ende)
    {
      bool überlapend;
      return Bib3.Geometrik.Geometrik.ScnitpunktGeraadeSegmentMitGeraadeSegment(geraadeSegment0Begin, geraadeSegment0Ende, geraadeSegment1Begin, geraadeSegment1Ende, out bool _, out bool _, out überlapend).HasValue || überlapend;
    }

    public static Vektor2DDouble? ScnitpunktGeraadeSegmentMitGeraadeSegment(
      Vektor2DDouble geraadeSegment0Begin,
      Vektor2DDouble geraadeSegment0Ende,
      Vektor2DDouble geraadeSegment1Begin,
      Vektor2DDouble geraadeSegment1Ende,
      out bool paralel,
      out bool kolinear,
      out bool überlapend)
    {
      kolinear = false;
      überlapend = false;
      Vektor2DDouble vektor2Ddouble1 = geraadeSegment0Ende - geraadeSegment0Begin;
      Vektor2DDouble vektor2Ddouble2 = geraadeSegment1Ende - geraadeSegment1Begin;
      double num1 = vektor2Ddouble1.Kroizprodukt(vektor2Ddouble2);
      double num2 = (geraadeSegment1Begin - geraadeSegment0Begin).Kroizprodukt(vektor2Ddouble2) / num1;
      double num3 = (geraadeSegment1Begin - geraadeSegment0Begin).Kroizprodukt(vektor2Ddouble1) / num1;
      if (0.0 == num1)
      {
        paralel = true;
        if (0.0 == (geraadeSegment1Begin - geraadeSegment0Begin).Kroizprodukt(vektor2Ddouble1))
        {
          kolinear = true;
          double num4 = (geraadeSegment1Begin - geraadeSegment0Begin).Skalarprodukt(vektor2Ddouble1);
          double num5 = (geraadeSegment0Begin - geraadeSegment1Begin).Skalarprodukt(vektor2Ddouble2);
          if (0.0 <= num4 && num4 <= vektor2Ddouble1.Skalarprodukt(vektor2Ddouble1) || 0.0 <= num5 && num5 <= vektor2Ddouble2.Skalarprodukt(vektor2Ddouble2))
          {
            überlapend = true;
            return new Vektor2DDouble?();
          }
        }
      }
      else
      {
        paralel = false;
        if (0.0 <= num2 && num2 <= 1.0 && 0.0 <= num3 && num3 <= 1.0)
          return new Vektor2DDouble?(geraadeSegment0Begin + num2 * vektor2Ddouble1);
      }
      return new Vektor2DDouble?();
    }

    public static Vektor2DDouble NääxterPunktAufGeraadeSegment(
      Vektor2DDouble geraadeSegmentBegin,
      Vektor2DDouble geraadeSegmentEnde,
      Vektor2DDouble suuceUrscprungPunktLaage)
    {
      return Bib3.Geometrik.Geometrik.NääxterPunktAufGeraadeSegment(geraadeSegmentBegin, geraadeSegmentEnde, suuceUrscprungPunktLaage, out double _);
    }

    public static Vektor2DDouble NääxterPunktAufGeraadeSegment(
      Vektor2DDouble geraadeSegmentBegin,
      Vektor2DDouble geraadeSegmentEnde,
      Vektor2DDouble suuceUrscprungPunktLaage,
      out double aufGeraadeNääxtePunktLaage)
    {
      double num = (geraadeSegmentEnde - geraadeSegmentBegin).LengthSquared();
      if (num <= 0.0)
      {
        aufGeraadeNääxtePunktLaage = !(suuceUrscprungPunktLaage == geraadeSegmentBegin) ? double.PositiveInfinity : 0.0;
        return geraadeSegmentBegin;
      }
      aufGeraadeNääxtePunktLaage = (suuceUrscprungPunktLaage - geraadeSegmentBegin).Skalarprodukt(geraadeSegmentEnde - geraadeSegmentBegin) / num;
      if (aufGeraadeNääxtePunktLaage < 0.0)
        return geraadeSegmentBegin;
      return 1.0 < aufGeraadeNääxtePunktLaage ? geraadeSegmentEnde : geraadeSegmentBegin + aufGeraadeNääxtePunktLaage * (geraadeSegmentEnde - geraadeSegmentBegin);
    }

    public static Vektor2DDouble NääxterPunktAufGeraade(
      Vektor2DDouble geradeRichtung,
      Vektor2DDouble punkt)
    {
      geradeRichtung = geradeRichtung.Normalized();
      double num = punkt.A * geradeRichtung.A + punkt.B * geradeRichtung.B;
      return new Vektor2DDouble(geradeRichtung.A * num, geradeRichtung.B * num);
    }

    public static Vektor2DDouble NääxterPunktAufGeraade(
      Vektor2DDouble geradeRichtung,
      Vektor2DDouble punkt,
      Vektor2DDouble geradeVersatz)
    {
      return Bib3.Geometrik.Geometrik.NääxterPunktAufGeraade(geradeRichtung, punkt - geradeVersatz) + geradeVersatz;
    }

    public static double DistanzVonPunktZuGeraade(
      Vektor2DDouble geradeRichtung,
      Vektor2DDouble punkt)
    {
      return (Bib3.Geometrik.Geometrik.NääxterPunktAufGeraade(geradeRichtung, punkt) - punkt).Length();
    }

    public static double DistanzVonPunktZuGeraadeSegment(
      Vektor2DDouble geraadeSegmentBegin,
      Vektor2DDouble geraadeSegmentEnde,
      Vektor2DDouble punkt)
    {
      Vektor2DDouble vektor2Ddouble = Bib3.Geometrik.Geometrik.NääxterPunktAufGeraadeSegment(geraadeSegmentBegin, geraadeSegmentEnde, punkt);
      return (punkt - vektor2Ddouble).Length();
    }

    public static double InFolgePunktNääxteBerecne(
      double suuceUrscprungPunktLaage,
      double zyyklusPunktLaage,
      double zyyklusLänge)
    {
      double num = (((zyyklusPunktLaage / zyyklusLänge + 1.0) % 1.0 + 1.5) % 1.0 - 0.5) * zyyklusLänge;
      return (double) (long) ((suuceUrscprungPunktLaage - num) / zyyklusLänge + 0.5) * zyyklusLänge + num;
    }

    public static int[] KonvexeHüleListePunktIndexBerecne(Vektor2DDouble[] listePunkt)
    {
      if (listePunkt == null)
        return (int[]) null;
      if (listePunkt.Length < 4)
        return Enumerable.Range(0, listePunkt.Length).ToArray<int>();
      KeyValuePair<int, Vektor2DDouble> keyValuePair = ((IEnumerable<KeyValuePair<int, Vektor2DDouble>>) ((IEnumerable<Vektor2DDouble>) listePunkt).Select<Vektor2DDouble, KeyValuePair<int, Vektor2DDouble>>((Func<Vektor2DDouble, int, KeyValuePair<int, Vektor2DDouble>>) ((punkt, index) => new KeyValuePair<int, Vektor2DDouble>(index, punkt))).ToArray<KeyValuePair<int, Vektor2DDouble>>()).OrderBy<KeyValuePair<int, Vektor2DDouble>, double>((Func<KeyValuePair<int, Vektor2DDouble>, double>) (kandidaat => kandidaat.Value.A)).FirstOrDefault<KeyValuePair<int, Vektor2DDouble>>();
      List<int> intList = new List<int>();
      intList.Add(keyValuePair.Key);
      int num1 = keyValuePair.Key;
      Vektor2DDouble vektor2Ddouble1 = keyValuePair.Value;
      double num2 = 0.25;
      while (true)
      {
        int index1 = num1;
        double num3 = 1.0;
        for (int index2 = 0; index2 < listePunkt.Length; ++index2)
        {
          if (index2 != num1)
          {
            double num4 = (Bib3.Geometrik.Geometrik.Rotatioon(listePunkt[index2] - vektor2Ddouble1) - num2 + 1.0) % 1.0;
            if (index1 == num1 || num4 < num3)
            {
              num3 = num4;
              index1 = index2;
            }
          }
        }
        if (index1 != keyValuePair.Key)
        {
          intList.Add(index1);
          Vektor2DDouble vektor2Ddouble2 = listePunkt[index1];
          num1 = index1;
          num2 = Bib3.Geometrik.Geometrik.Rotatioon(vektor2Ddouble2 - vektor2Ddouble1);
          vektor2Ddouble1 = vektor2Ddouble2;
        }
        else
          break;
      }
      return intList.ToArray();
    }

    public static double Rotatioon(Vektor2DDouble vektor)
    {
      vektor = vektor.Normalized();
      double num = Math.Acos(vektor.A) / Math.PI / 2.0;
      if (vektor.B < 0.0)
        num = 1.0 - num;
      return num;
    }

    public static double Rotatioon(Vektor2DDouble vektor0, Vektor2DDouble vektor1) => Math.Acos(Math.Min(1.0, Math.Max(-1.0, vektor0.Normalized().Skalarprodukt(vektor1.Normalized())))) / Math.PI / 2.0;

    public static int SaiteVonGeraadeZuPunkt(Vektor2DDouble geraadeRictung, Vektor2DDouble punkt) => Math.Sign(punkt.Skalarprodukt(new Vektor2DDouble(-geraadeRictung.B, geraadeRictung.A)));

    private static long[] ListeGrenzeAusÜberscnaidung1D(
      long regioonAScrankeMin,
      long regioonAScrankeMax,
      long regioonBScrankeMin,
      long regioonBScrankeMax)
    {
      KeyValuePair<long, long> keyValuePair1 = new KeyValuePair<long, long>(regioonAScrankeMin, regioonAScrankeMax);
      KeyValuePair<long, long> keyValuePair2 = new KeyValuePair<long, long>(regioonBScrankeMin, regioonBScrankeMax);
      return ((IEnumerable<long>) ((IEnumerable<KeyValuePair<long, KeyValuePair<long, long>>>) new KeyValuePair<long, KeyValuePair<long, long>>[4]
      {
        new KeyValuePair<long, KeyValuePair<long, long>>(regioonAScrankeMin, keyValuePair2),
        new KeyValuePair<long, KeyValuePair<long, long>>(regioonAScrankeMax, keyValuePair2),
        new KeyValuePair<long, KeyValuePair<long, long>>(regioonBScrankeMin, keyValuePair1),
        new KeyValuePair<long, KeyValuePair<long, long>>(regioonBScrankeMax, keyValuePair1)
      }).Where<KeyValuePair<long, KeyValuePair<long, long>>>((Func<KeyValuePair<long, KeyValuePair<long, long>>, bool>) (kandidaat => kandidaat.Value.Key <= kandidaat.Key && kandidaat.Key < kandidaat.Value.Value)).Select<KeyValuePair<long, KeyValuePair<long, long>>, long>((Func<KeyValuePair<long, KeyValuePair<long, long>>, long>) (kandidaat => kandidaat.Key)).ToArray<long>()).Distinct<long>().ToArray<long>();
    }

    public static IEnumerable<RectInt> Diferenz(this RectInt minuend, RectInt subtrahend)
    {
      KeyValuePair<Vektor2DInt, Vektor2DInt> MinuendMinMax = new KeyValuePair<Vektor2DInt, Vektor2DInt>(minuend.MinPoint(), minuend.MaxPoint());
      KeyValuePair<Vektor2DInt, Vektor2DInt> SubtrahendMinMax = new KeyValuePair<Vektor2DInt, Vektor2DInt>(subtrahend.MinPoint(), subtrahend.MaxPoint());
      if (MinuendMinMax.Value.A <= SubtrahendMinMax.Key.A || MinuendMinMax.Value.B <= SubtrahendMinMax.Key.B || SubtrahendMinMax.Value.A <= MinuendMinMax.Key.A || SubtrahendMinMax.Value.B <= MinuendMinMax.Key.B)
        yield return minuend;
      else if (MinuendMinMax.Value.A > SubtrahendMinMax.Value.A || MinuendMinMax.Value.B > SubtrahendMinMax.Value.B || SubtrahendMinMax.Key.A > MinuendMinMax.Key.A || SubtrahendMinMax.Key.B > MinuendMinMax.Key.B)
      {
        long[] RictungAMengeScranke = ((IEnumerable<long>) Bib3.Geometrik.Geometrik.ListeGrenzeAusÜberscnaidung1D(MinuendMinMax.Key.A, MinuendMinMax.Value.A, SubtrahendMinMax.Key.A, SubtrahendMinMax.Value.A)).OrderBy<long, long>((Func<long, long>) (t => t)).ToArray<long>();
        long[] RictungBMengeScranke = ((IEnumerable<long>) Bib3.Geometrik.Geometrik.ListeGrenzeAusÜberscnaidung1D(MinuendMinMax.Key.B, MinuendMinMax.Value.B, SubtrahendMinMax.Key.B, SubtrahendMinMax.Value.B)).OrderBy<long, long>((Func<long, long>) (t => t)).ToArray<long>();
        if (RictungAMengeScranke.Length < 1 || RictungBMengeScranke.Length < 1)
        {
          yield return minuend;
        }
        else
        {
          long[] RictungAMengeScrankeMitMinuendGrenze = ((IEnumerable<long>) new long[1]
          {
            MinuendMinMax.Key.A
          }).Concat<long>((IEnumerable<long>) RictungAMengeScranke).Concat<long>((IEnumerable<long>) new long[1]
          {
            MinuendMinMax.Value.A
          }).ToArray<long>();
          for (int RictungAScrankeIndex = 0; RictungAScrankeIndex < RictungAMengeScrankeMitMinuendGrenze.Length - 1; ++RictungAScrankeIndex)
          {
            long RictungAScrankeMinLaage = RictungAMengeScrankeMitMinuendGrenze[RictungAScrankeIndex];
            long RictungAScrankeMaxLaage = RictungAMengeScrankeMitMinuendGrenze[RictungAScrankeIndex + 1];
            if (SubtrahendMinMax.Value.A <= RictungAScrankeMinLaage || RictungAScrankeMaxLaage <= SubtrahendMinMax.Key.A)
            {
              yield return new RectInt(RictungAScrankeMinLaage, MinuendMinMax.Key.B, RictungAScrankeMaxLaage, MinuendMinMax.Value.B);
            }
            else
            {
              long RictungBMengeScrankeFrüheste = ((IEnumerable<long>) RictungBMengeScranke).First<long>();
              long RictungBMengeScrankeLezte = ((IEnumerable<long>) RictungBMengeScranke).Last<long>();
              if (MinuendMinMax.Key.B < SubtrahendMinMax.Key.B)
                yield return new RectInt(RictungAScrankeMinLaage, MinuendMinMax.Key.B, RictungAScrankeMaxLaage, SubtrahendMinMax.Key.B);
              if (SubtrahendMinMax.Value.B < MinuendMinMax.Value.B)
                yield return new RectInt(RictungAScrankeMinLaage, SubtrahendMinMax.Value.B, RictungAScrankeMaxLaage, MinuendMinMax.Value.B);
            }
          }
        }
      }
    }

    public static Vektor2DInt RotatedByMikro(this Vektor2DInt vector, int rotationMikro) => new Vektor2DInt((long) ((double) vector.A * Math.Cos((double) rotationMikro * Math.PI / 500000.0) - (double) vector.B * Math.Sin((double) rotationMikro * Math.PI / 500000.0)), (long) ((double) vector.A * Math.Sin((double) rotationMikro * Math.PI / 500000.0) + (double) vector.B * Math.Cos((double) rotationMikro * Math.PI / 500000.0)));
  }
}
