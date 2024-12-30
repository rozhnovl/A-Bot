// Decompiled with JetBrains decompiler
// Type: Bib3.Kombinatoorik
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib3
{
  public static class Kombinatoorik
  {
    public static T[][] MengeKombinatioonTailmengeOoneOrdnung<T>(
      T[] menge,
      int mengeElementZuVerwendeAnzaal)
    {
      if (menge == null)
        return (T[][]) null;
      if (mengeElementZuVerwendeAnzaal <= 0)
        return new T[0][];
      int num = menge.Length - mengeElementZuVerwendeAnzaal;
      if (num <= 0)
        return new T[1][]{ menge };
      T[][] source = new T[menge.Length][];
      for (int index1 = 0; index1 < source.Length; ++index1)
      {
        T[] objArray = new T[menge.Length - 1];
        int index2 = 0;
        int index3 = 0;
        for (; index2 < menge.Length; ++index2)
        {
          if (index2 != index1)
          {
            objArray[index3] = menge[index2];
            ++index3;
          }
        }
        source[index1] = objArray;
      }
      return num <= 1 ? source : Glob.ArrayAusListeFeldGeflact<T[]>(((IEnumerable<T[]>) source).Select<T[], T[][]>((Func<T[], T[][]>) (tailmenge => Kombinatoorik.MengeKombinatioonTailmengeOoneOrdnung<T>(tailmenge, mengeElementZuVerwendeAnzaal))));
    }
  }
}
