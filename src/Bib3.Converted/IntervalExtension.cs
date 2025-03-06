// Decompiled with JetBrains decompiler
// Type: Bib3.IntervalExtension
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;

namespace Bib3
{
  public static class IntervalExtension
  {
    public static PropertyGenTimespanInt64<TAus> MapValue<TAin, TAus>(
      this PropertyGenTimespanInt64<TAin> orig,
      Func<TAin, TAus> sict)
    {
      return orig == null ? (PropertyGenTimespanInt64<TAus>) null : new PropertyGenTimespanInt64<TAus>(sict(orig.Value), orig.Low, orig.Up);
    }
  }
}
