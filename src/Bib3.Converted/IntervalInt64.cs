// Decompiled with JetBrains decompiler
// Type: Bib3.IntervalInt64
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3
{
  public class IntervalInt64 : IIntervalInt64
  {
    public long Low { set; get; }

    public long Up { set; get; }

    public IntervalInt64()
    {
    }

    public IntervalInt64(IIntervalInt64 @base)
    {
      this.Low = @base != null ? @base.Low : 0L;
      this.Up = @base != null ? @base.Up : 0L;
    }

    public IntervalInt64(long low, long up)
    {
      this.Low = low;
      this.Up = up;
    }
  }
}
