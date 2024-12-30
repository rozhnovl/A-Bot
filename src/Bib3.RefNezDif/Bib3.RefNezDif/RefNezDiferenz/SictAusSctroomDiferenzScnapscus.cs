// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictAusSctroomDiferenzScnapscus
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Bib3.RefBaumKopii;
using System.Collections.Generic;

namespace Bib3.RefNezDiferenz
{
  public class SictAusSctroomDiferenzScnapscus
  {
    private readonly SictRefNezSume SictSume;
    private readonly SictDiferenzSictParam SictParam;

    public SictAusSctroomDiferenzScnapscus(SictDiferenzSictParam sictParam)
    {
      this.SictParam = sictParam;
      this.SictSume = new SictRefNezSume(sictParam);
    }

    public KeyValuePair<object[], bool> ScnapscusBerecne(
      SictZuNezSictDiferenzScritAbbild diferenzScrit)
    {
      SictRefNezScritSumeErgeebnis scritSumeErgeebnis = this.SictSume.BerecneScritSumeListeWurzelRefClrUndErfolg(diferenzScrit);
      return new KeyValuePair<object[], bool>(SictRefNezKopii.ObjektKopiiErsctele<object[]>(scritSumeErgeebnis == null ? (object[]) null : scritSumeErgeebnis.ListeWurzelRefClr, (Param) null, new int?()), scritSumeErgeebnis.Volsctändig);
    }
  }
}
