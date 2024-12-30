// Decompiled with JetBrains decompiler
// Type: Bib3.TimeSpanExtension.TimeSpanExtension
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;

namespace Bib3.TimeSpanExtension
{
  public static class TimeSpanExtension
  {
    public static TimeSpan TimeSpanFromMilliseconds(this long milliseconds) => TimeSpan.FromMilliseconds((double) milliseconds);

    public static TimeSpan TimeSpanFromSeconds(this long seconds) => TimeSpan.FromSeconds((double) seconds);

    public static DateTime? Add(this DateTime dateTime, TimeSpan? timeSpan)
    {
      if (!timeSpan.HasValue)
        return new DateTime?();
      DateTime dateTime1 = dateTime;
      TimeSpan? nullable = timeSpan;
      return !nullable.HasValue ? new DateTime?() : new DateTime?(dateTime1 + nullable.GetValueOrDefault());
    }
  }
}
