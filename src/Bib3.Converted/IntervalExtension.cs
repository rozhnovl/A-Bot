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
    public static WertZuZaitpunktStruct<TAus> CastGen<TAin, TAus>(
      this WertZuZaitpunktStruct<TAin> ain)
      where TAin : TAus
    {
      return new WertZuZaitpunktStruct<TAus>((TAus) ain.Wert, ain.Zait);
    }

    public static WertZuZaitpunktStruct<TAus>? CastGen<TAin, TAus>(
      this WertZuZaitpunktStruct<TAin>? ain)
      where TAin : TAus
    {
      return !ain.HasValue ? new WertZuZaitpunktStruct<TAus>?() : new WertZuZaitpunktStruct<TAus>?(ain.Value.CastGen<TAin, TAus>());
    }

    public static StructPropertyGenIntervalInt64<TAus> CastGen<TAin, TAus>(
      this StructPropertyGenIntervalInt64<TAin> ain)
      where TAin : TAus
    {
      return new StructPropertyGenIntervalInt64<TAus>((TAus) ain.Value, ain.Low, ain.Up);
    }

    public static StructPropertyGenIntervalInt64<TAus>? CastGen<TAin, TAus>(
      this StructPropertyGenIntervalInt64<TAin>? ain)
      where TAin : TAus
    {
      return !ain.HasValue ? new StructPropertyGenIntervalInt64<TAus>?() : new StructPropertyGenIntervalInt64<TAus>?(ain.Value.CastGen<TAin, TAus>());
    }

    public static StructPropertyGenIntervalInt64<TAus> CastScpez<TAin, TAus>(
      this StructPropertyGenIntervalInt64<TAin> ain)
      where TAus : class
    {
      return new StructPropertyGenIntervalInt64<TAus>((object) ain.Value as TAus, ain.Low, ain.Up);
    }

    public static StructPropertyGenIntervalInt64<TAus>? CastScpez<TAin, TAus>(
      this StructPropertyGenIntervalInt64<TAin>? ain)
      where TAus : class
    {
      return !ain.HasValue ? new StructPropertyGenIntervalInt64<TAus>?() : new StructPropertyGenIntervalInt64<TAus>?(ain.Value.CastScpez<TAin, TAus>());
    }

    public static PropertyGenIntervalInt64<TAus> MapValue<TAin, TAus>(
      this PropertyGenIntervalInt64<TAin> orig,
      Func<TAin, TAus> sict)
    {
      return orig == null ? (PropertyGenIntervalInt64<TAus>) null : new PropertyGenIntervalInt64<TAus>(sict(orig.Value), orig.Low, orig.Up);
    }

    public static PropertyGenTimespanInt64<TAus> MapValue<TAin, TAus>(
      this PropertyGenTimespanInt64<TAin> orig,
      Func<TAin, TAus> sict)
    {
      return orig == null ? (PropertyGenTimespanInt64<TAus>) null : new PropertyGenTimespanInt64<TAus>(sict(orig.Value), orig.Low, orig.Up);
    }

    public static PropertyGenIntervalInt64<TAus> CastScpez<TAin, TAus>(
      this PropertyGenIntervalInt64<TAin> ain)
      where TAus : class
    {
      return ain == null ? (PropertyGenIntervalInt64<TAus>) null : ain.MapValue<TAin, TAus>((Func<TAin, TAus>) (ainWert => (object) ainWert as TAus));
    }

    public static PropertyGenTimespanInt64<TAus> CastScpez<TAin, TAus>(
      this PropertyGenTimespanInt64<TAin> ain)
      where TAus : class
    {
      return ain == null ? (PropertyGenTimespanInt64<TAus>) null : IntervalExtension.MapValue<TAin, TAus>(ain, (Func<TAin, TAus>) (ainWert => (object) ainWert as TAus));
    }

    public static long Duration(this ITimespanInt64 timespan) => timespan.End - timespan.Begin;

    public static PropertyGenIntervalInt64<ValueT> WithIntervalInt64<ValueT>(
      this ValueT value,
      IIntervalInt64 interval)
    {
      return new PropertyGenIntervalInt64<ValueT>(value, interval);
    }

    public static PropertyGenTimespanInt64<ValueT> WithTimespanInt64<ValueT>(
      this ValueT value,
      IIntervalInt64 interval)
    {
      return new PropertyGenTimespanInt64<ValueT>(value, interval);
    }
  }
}
