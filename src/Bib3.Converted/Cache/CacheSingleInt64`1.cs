// Decompiled with JetBrains decompiler
// Type: Bib3.Cache.CacheSingleInt64`1
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;

namespace Bib3.Cache
{
  public class CacheSingleInt64<CachedT>
  {
    private readonly object Lock = new object();
    public Func<CachedT> RefreshDelegate;
    public int AgeMax;
    public Func<long> GetTimeDelegate;
    private KeyValuePair<long, CachedT>? RefreshLast;

    public CachedT Value
    {
      get
      {
        Func<long> getTimeDelegate = this.GetTimeDelegate;
        return this.GetValue(getTimeDelegate != null ? getTimeDelegate() : 0L);
      }
    }

    public CachedT GetValue(long time)
    {
      lock (this.Lock)
      {
        long num = time;
        ref KeyValuePair<long, CachedT>? local = ref this.RefreshLast;
        long? nullable1 = local.HasValue ? new long?(local.GetValueOrDefault().Key) : new long?();
        long? nullable2 = nullable1.HasValue ? new long?(num - nullable1.GetValueOrDefault()) : new long?();
        long ageMax = (long) this.AgeMax;
        if (nullable2.GetValueOrDefault() <= ageMax && nullable2.HasValue)
          return this.RefreshLast.Value.Value;
        CachedT cachedT = default (CachedT);
        Func<CachedT> refreshDelegate = this.RefreshDelegate;
        if (refreshDelegate != null)
          cachedT = refreshDelegate();
        this.RefreshLast = new KeyValuePair<long, CachedT>?(new KeyValuePair<long, CachedT>(time, cachedT));
        return cachedT;
      }
    }
  }
}
