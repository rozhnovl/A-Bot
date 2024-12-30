// Decompiled with JetBrains decompiler
// Type: Bib3.StructPropertyGenIntervalInt64`1
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3
{
  public struct StructPropertyGenIntervalInt64<ValueT> : 
    IPropertyGenIntervalInt64<ValueT>,
    IIntervalInt64
  {
    public ValueT Value { set; get; }

    public long Low { set; get; }

    public long Up { set; get; }

    public StructPropertyGenIntervalInt64(ValueT value, IIntervalInt64 @base = null)
    {
      this.Value = value;
      this.Low = @base != null ? @base.Low : 0L;
      this.Up = @base != null ? @base.Up : 0L;
    }

    public StructPropertyGenIntervalInt64(ValueT value, long low, long up)
    {
      this.Value = value;
      this.Low = low;
      this.Up = up;
    }

    public StructPropertyGenIntervalInt64(ValueT value, long point)
      : this(value, point, point)
    {
    }
  }
}
