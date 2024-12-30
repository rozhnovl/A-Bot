// Decompiled with JetBrains decompiler
// Type: Bib3.SictMesungZaitraumAusStopwatch
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3
{
  public struct SictMesungZaitraumAusStopwatch
  {
    private long? InternBeginZaitMikro;
    private long? InternEndeZaitMikro;

    public long? BeginZaitMikro => this.InternBeginZaitMikro;

    public long? EndeZaitMikro => this.InternEndeZaitMikro;

    public long? DauerMikro
    {
      get
      {
        long? internEndeZaitMikro = this.InternEndeZaitMikro;
        long? internBeginZaitMikro = this.InternBeginZaitMikro;
        return internEndeZaitMikro.HasValue & internBeginZaitMikro.HasValue ? new long?(internEndeZaitMikro.GetValueOrDefault() - internBeginZaitMikro.GetValueOrDefault()) : new long?();
      }
    }

    public SictMesungZaitraumAusStopwatch(bool beginJezt = false)
      : this()
    {
      if (!beginJezt)
        return;
      this.BeginSezeJezt();
    }

    public void BeginSezeJezt() => this.InternBeginZaitMikro = new long?(Glob.StopwatchZaitMikroSictInt());

    public void EndeSezeJezt() => this.InternEndeZaitMikro = new long?(Glob.StopwatchZaitMikroSictInt());
  }
}
