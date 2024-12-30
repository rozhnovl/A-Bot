// Decompiled with JetBrains decompiler
// Type: Bib3.SictDictZuList`2
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bib3
{
  [JsonObject]
  public class SictDictZuList<TKey, TValue> : 
    IDictionary<TKey, TValue>,
    ICollection<KeyValuePair<TKey, TValue>>,
    IEnumerable<KeyValuePair<TKey, TValue>>,
    IEnumerable
  {
    [JsonProperty]
    private readonly System.Collections.Generic.List<KeyValuePair<TKey, TValue>> List = new System.Collections.Generic.List<KeyValuePair<TKey, TValue>>();

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => (IEnumerator<KeyValuePair<TKey, TValue>>) this.List.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.List.GetEnumerator();

    public int Count => this.List.Count;

    public bool IsReadOnly => false;

    public void EntferneMengeAintraagSolangeAnzaalGrööserScranke<TOrderKey>(
      int mengeAintraagAnzaalScranke,
      Func<KeyValuePair<TKey, TValue>, TOrderKey> funkOrdnungKeySelector)
    {
      if (mengeAintraagAnzaalScranke < 1)
      {
        this.Clear();
      }
      else
      {
        if (funkOrdnungKeySelector == null)
          return;
        System.Collections.Generic.List<KeyValuePair<TKey, TValue>> list = this.List;
        if (list == null)
          return;
        KeyValuePair<TKey, TValue>[] array = list.OrderBy<KeyValuePair<TKey, TValue>, TOrderKey>(funkOrdnungKeySelector).ToArray<KeyValuePair<TKey, TValue>>();
        KeyValuePair<TKey, TValue>[] ListeZuEntferne = ((IEnumerable<KeyValuePair<TKey, TValue>>) array).Take<KeyValuePair<TKey, TValue>>(array.Length - mengeAintraagAnzaalScranke).ToArray<KeyValuePair<TKey, TValue>>();
        list.RemoveAll((Predicate<KeyValuePair<TKey, TValue>>) (kandidaat => ((IEnumerable<KeyValuePair<TKey, TValue>>) ListeZuEntferne).Any<KeyValuePair<TKey, TValue>>((Func<KeyValuePair<TKey, TValue>, bool>) (kandidaatZuEntferne => object.Equals((object) kandidaat.Key, (object) kandidaatZuEntferne.Key)))));
      }
    }

    public void Clear() => this.List.Clear();

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      for (int index = 0; index < this.List.Count; ++index)
      {
        if (object.Equals((object) this.List[index], (object) item))
          return true;
      }
      return false;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => this.List.CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      bool flag = false;
      for (int index = 0; index < this.List.Count; ++index)
      {
        if (object.Equals((object) this.List[index], (object) item))
        {
          this.List.RemoveAt(index--);
          flag = true;
        }
      }
      return flag;
    }

    public ICollection<TKey> Keys => (ICollection<TKey>) this.List.Select<KeyValuePair<TKey, TValue>, TKey>((Func<KeyValuePair<TKey, TValue>, TKey>) (t => t.Key)).ToArray<TKey>();

    public ICollection<TValue> Values => (ICollection<TValue>) this.List.Select<KeyValuePair<TKey, TValue>, TValue>((Func<KeyValuePair<TKey, TValue>, TValue>) (t => t.Value)).ToArray<TValue>();

    public TValue this[TKey key]
    {
      get
      {
        for (int index = 0; index < this.List.Count; ++index)
        {
          KeyValuePair<TKey, TValue> keyValuePair = this.List[index];
          if (object.Equals((object) keyValuePair.Key, (object) key))
            return keyValuePair.Value;
        }
        throw new KeyNotFoundException();
      }
      set
      {
        KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, value);
        for (int index = 0; index < this.List.Count; ++index)
        {
          if (object.Equals((object) this.List[index].Key, (object) key))
          {
            this.List[index] = keyValuePair;
            return;
          }
        }
        this.List.Add(keyValuePair);
      }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
      if (this.ContainsKey(item.Key))
        throw new ArgumentException("key already present");
      this.List.Add(item);
    }

    public void Add(TKey key, TValue value) => this.Add(new KeyValuePair<TKey, TValue>(key, value));

    public bool ContainsKey(TKey key)
    {
      for (int index = 0; index < this.List.Count; ++index)
      {
        if (object.Equals((object) this.List[index].Key, (object) key))
          return true;
      }
      return false;
    }

    public bool Remove(TKey key)
    {
      for (int index = 0; index < this.List.Count; ++index)
      {
        if (object.Equals((object) this.List[index].Key, (object) key))
        {
          this.List.RemoveAt(index);
          return true;
        }
      }
      return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      value = default (TValue);
      for (int index = 0; index < this.List.Count; ++index)
      {
        KeyValuePair<TKey, TValue> keyValuePair = this.List[index];
        if (object.Equals((object) keyValuePair.Key, (object) key))
        {
          value = keyValuePair.Value;
          return true;
        }
      }
      return false;
    }
  }
}
