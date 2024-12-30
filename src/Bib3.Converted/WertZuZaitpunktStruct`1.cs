// Decompiled with JetBrains decompiler
// Type: Bib3.WertZuZaitpunktStruct`1
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3
{
  public struct WertZuZaitpunktStruct<T>
  {
    public T Wert;
    public long Zait;

    public WertZuZaitpunktStruct(T wert, long zait)
    {
      this.Wert = wert;
      this.Zait = zait;
    }
  }
}
