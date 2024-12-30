// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictMeldungType
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System.Collections.Generic;

namespace Bib3.RefNezDiferenz
{
  public class SictMeldungType
  {
    public string ClrName;
    public KeyValuePair<int, string>[] MengeZuMemberIdentClrName;

    public SictMeldungType()
    {
    }

    public SictMeldungType(
      string clrName,
      KeyValuePair<int, string>[] mengeZuMemberIdentClrName)
    {
      this.ClrName = clrName;
      this.MengeZuMemberIdentClrName = mengeZuMemberIdentClrName;
    }
  }
}
