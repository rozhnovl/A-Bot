// Decompiled with JetBrains decompiler
// Type: Bib3.CallRateScranke
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;

namespace Bib3
{
  public class CallRateScranke
  {
    private readonly Action Action;
    private readonly Func<long> FunkZait;
    public readonly int DistanzScrankeMin;
    public long ActionLezteZait;

    public CallRateScranke(Action action, Func<long> funkZait, int distanzScrankeMin)
    {
      this.Action = action;
      this.FunkZait = funkZait;
      this.DistanzScrankeMin = distanzScrankeMin;
    }

    public void Call()
    {
      if (this.FunkZait == null || this.Action == null)
        return;
      long num = this.FunkZait();
      if (num - this.ActionLezteZait < (long) this.DistanzScrankeMin)
        return;
      this.ActionLezteZait = num;
      this.Action();
    }
  }
}
