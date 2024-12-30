// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.MemberTransitNotification
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

namespace Bib3.RefNezDiferenz
{
  public class MemberTransitNotification
  {
    public readonly string MemberName;
    public readonly object MemberValueCurrent;
    public readonly object MemberValuePrev;

    public MemberTransitNotification(
      string memberName,
      object memberValueCurrent,
      object memberValuePrev)
    {
      this.MemberName = memberName;
      this.MemberValueCurrent = memberValueCurrent;
      this.MemberValuePrev = memberValuePrev;
    }
  }
}
