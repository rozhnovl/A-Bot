// Decompiled with JetBrains decompiler
// Type: Bib3.PropertyGenTimespanInt64`1
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3
{
  public class PropertyGenTimespanInt64<ValueT> : PropertyGenIntervalInt64<ValueT>, ITimespanInt64
  {
    public long Begin
    {
      set => this.Low = value;
      get => this.Low;
    }

    public long End
    {
      set => this.Up = value;
      get => this.Up;
    }

    public PropertyGenTimespanInt64()
    {
    }

    public PropertyGenTimespanInt64(ValueT value, long begin, long end)
      : base(value, begin, end)
    {
    }

    public PropertyGenTimespanInt64(ValueT value, long time)
      : this(value, time, time)
    {
    }

    public PropertyGenTimespanInt64(ValueT value, IIntervalInt64 interval)
      : base(value, interval)
    {
    }
  }
}
