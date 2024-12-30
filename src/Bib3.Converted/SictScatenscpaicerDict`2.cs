// Decompiled with JetBrains decompiler
// Type: Bib3.SictScatenscpaicerDict`2
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bib3
{
  public class SictScatenscpaicerDict<TKey, TValue> : 
    IReadOnlyDictionary<TKey, TValue>,
    IReadOnlyCollection<KeyValuePair<TKey, TValue>>,
    IEnumerable<KeyValuePair<TKey, TValue>>,
    IEnumerable
  {
    private readonly object Lock = new object();
    private readonly Dictionary<TKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>> Dict = new Dictionary<TKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>>();

    public int Count => this.Dict.Count;

    public IEnumerable<TKey> Keys => (IEnumerable<TKey>) this.Dict.Keys;

    public IEnumerable<TValue> Values => this.Dict.Values.Select<SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>, TValue>((Func<SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>, TValue>) (t => t.Wert));

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) ((IEnumerable<KeyValuePair<TKey, TValue>>) this).GetEnumerator();

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => (IEnumerator<KeyValuePair<TKey, TValue>>) new SictScatenscpaicerDict<TKey, TValue>.SictScatenscpaicerDictEnumerator<TKey, TValue>(this.Dict);

    public IEnumerable<TKey> MengeKeyBerecne() => (IEnumerable<TKey>) this.Dict.Keys;

    public void Leere() => this.Dict.Clear();

    public TValue this[TKey key] => this.Dict[key].Wert;

    public bool ContainsKey(TKey key) => this.Dict.ContainsKey(key);

    public bool TryGetValue(TKey key, out TValue value)
    {
      SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue> scatenscpaicerAintraagInfo;
      bool flag = this.Dict.TryGetValue(key, out scatenscpaicerAintraagInfo);
      value = !flag ? default (TValue) : scatenscpaicerAintraagInfo.Wert;
      return flag;
    }

    public TValue ValueFürKey(TKey key, Func<TKey, TValue> funkBescafFalsNictGescpaicert = null)
    {
      SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue> scatenscpaicerAintraagInfo = (SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>) null;
      this.Dict.TryGetValue(key, out scatenscpaicerAintraagInfo);
      long zaitMili = Glob.StopwatchZaitMiliSictInt();
      if (scatenscpaicerAintraagInfo != null)
      {
        scatenscpaicerAintraagInfo.VerwendungLezteZaitMili = zaitMili;
        return scatenscpaicerAintraagInfo.Wert;
      }
      if (funkBescafFalsNictGescpaicert == null)
        return default (TValue);
      TValue obj = funkBescafFalsNictGescpaicert(key);
      this.AintraagErsctele(key, obj, zaitMili);
      return obj;
    }

    public void AintraagErsctele(TKey key, TValue value) => this.AintraagErsctele(key, value, Glob.StopwatchZaitMiliSictInt());

    private void AintraagErsctele(TKey key, TValue value, long zaitMili)
    {
      lock (this.Lock)
      {
        SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue> scatenscpaicerAintraagInfo = new SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>(value, zaitMili);
        this.Dict[key] = scatenscpaicerAintraagInfo;
      }
    }

    public void EntferneMengeAintraagVonVorStopwatchZaitMili(long aintraagStopwatchZaitScrankeMili)
    {
      foreach (KeyValuePair<TKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>> keyValuePair in this.Dict.Where<KeyValuePair<TKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>>>((Func<KeyValuePair<TKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>>, bool>) (kandidaat => kandidaat.Value.AnlaageZaitMili < aintraagStopwatchZaitScrankeMili)).ToArray<KeyValuePair<TKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>>>())
        this.Dict.Remove(keyValuePair.Key);
    }

    public void BescrankeEntferneLängerNitVerwendete(
      long? mengeAintraagAnzaalScranke,
      long? mengeAintraagKapazitäätAggrScranke = null,
      Func<TValue, long> funkFürAintraagBerecneKapazitäät = null,
      TValue[] mengeAintraagZuErhalte = null)
    {
      KeyValuePair<TKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>>[] array = this.Dict.OrderByDescending<KeyValuePair<TKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>>, long>((Func<KeyValuePair<TKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>>, long>) (aintraag => aintraag.Value.VerwendungLezteZaitMili)).ToArray<KeyValuePair<TKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>>>();
      long num1 = 0;
      bool flag = false;
      for (int index = 0; index < array.Length; ++index)
      {
        KeyValuePair<TKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<TValue>> keyValuePair = array[index];
        if (!flag)
        {
          int num2 = flag ? 1 : 0;
          long? nullable = mengeAintraagAnzaalScranke;
          long num3 = (long) index;
          int num4 = nullable.GetValueOrDefault() <= num3 ? (nullable.HasValue ? 1 : 0) : 0;
          flag = (num2 | num4) != 0;
          if (funkFürAintraagBerecneKapazitäät != null && mengeAintraagKapazitäätAggrScranke.HasValue)
          {
            num1 += funkFürAintraagBerecneKapazitäät(keyValuePair.Value.Wert);
            int num5 = flag ? 1 : 0;
            nullable = mengeAintraagKapazitäätAggrScranke;
            long num6 = num1;
            int num7 = nullable.GetValueOrDefault() < num6 ? (nullable.HasValue ? 1 : 0) : 0;
            flag = (num5 | num7) != 0;
          }
        }
        if (flag)
          this.Dict.Remove(keyValuePair.Key);
      }
    }

    private class SictZuScatenscpaicerAintraagInfo<T>
    {
      public readonly T Wert;
      public readonly long AnlaageZaitMili;
      public long VerwendungLezteZaitMili;

      public SictZuScatenscpaicerAintraagInfo(T wert, long anlaageZaitMili)
      {
        this.Wert = wert;
        this.AnlaageZaitMili = anlaageZaitMili;
        this.VerwendungLezteZaitMili = anlaageZaitMili;
      }
    }

    private class SictScatenscpaicerDictEnumerator<EnumeratorTKey, EnumeratorTValue> : 
      IEnumerator<KeyValuePair<EnumeratorTKey, EnumeratorTValue>>,
      IDisposable,
      IEnumerator
    {
      private readonly Dictionary<EnumeratorTKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<EnumeratorTValue>> Dict;
      private Dictionary<EnumeratorTKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<EnumeratorTValue>>.Enumerator DictEnumerator;

      public SictScatenscpaicerDictEnumerator(
        Dictionary<EnumeratorTKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<EnumeratorTValue>> dict)
      {
        this.Dict = dict;
        this.Reset();
      }

      public KeyValuePair<EnumeratorTKey, EnumeratorTValue> Current
      {
        get
        {
          KeyValuePair<EnumeratorTKey, SictScatenscpaicerDict<TKey, TValue>.SictZuScatenscpaicerAintraagInfo<EnumeratorTValue>> current = this.DictEnumerator.Current;
          return new KeyValuePair<EnumeratorTKey, EnumeratorTValue>(current.Key, current.Value.Wert);
        }
      }

      object IEnumerator.Current => (object) this.Current;

      public void Dispose() => this.DictEnumerator.Dispose();

      public bool MoveNext() => this.DictEnumerator.MoveNext();

      public void Reset() => this.DictEnumerator = this.Dict.GetEnumerator();
    }
  }
}
