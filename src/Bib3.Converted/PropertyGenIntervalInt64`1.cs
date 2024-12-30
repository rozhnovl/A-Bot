// Decompiled with JetBrains decompiler
// Type: Bib3.PropertyGenIntervalInt64`1
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3
{
  public class PropertyGenIntervalInt64<ValueT> : 
    IntervalInt64,
    IPropertyGenIntervalInt64<ValueT>,
    IIntervalInt64
  {
    public ValueT Value { set; get; }

    public PropertyGenIntervalInt64()
    {
    }

    public PropertyGenIntervalInt64(ValueT value, IIntervalInt64 @base = null)
      : base(@base)
    {
      this.Value = value;
    }

    public PropertyGenIntervalInt64(ValueT value, long low, long up)
      : base(low, up)
    {
      this.Value = value;
    }

    public PropertyGenIntervalInt64(ValueT value, long point)
      : base(point, point)
    {
      this.Value = value;
    }
  }
}
