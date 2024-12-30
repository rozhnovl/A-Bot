// Decompiled with JetBrains decompiler
// Type: Bib3.SictIdentInt64Fabrik
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3
{
  public class SictIdentInt64Fabrik
  {
    private readonly object Lock = new object();
    private long Lezte = 0;

    public SictIdentInt64Fabrik()
    {
    }

    public SictIdentInt64Fabrik(long begin) => this.Lezte = begin;

    public long IdentBerecne()
    {
      lock (this.Lock)
        return this.Lezte++;
    }
  }
}
