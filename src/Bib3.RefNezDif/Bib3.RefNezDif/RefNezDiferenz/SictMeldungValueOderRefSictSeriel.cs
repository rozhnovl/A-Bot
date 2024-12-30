// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictMeldungValueOderRefSictSeriel
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

namespace Bib3.RefNezDiferenz
{
  public class SictMeldungValueOderRefSictSeriel
  {
    public string ValueSictJsonAbbild;
    public SictMeldungValueAstReferenzSictSeriel ValueBaumReferenz;
    public long? Referenz;

    public SictMeldungValueOderRefSictSeriel()
    {
    }

    public SictMeldungValueOderRefSictSeriel(
      string valueSictJsonAbbild,
      SictMeldungValueAstReferenzSictSeriel valueBaumReferenz,
      long? referenz)
    {
      this.ValueSictJsonAbbild = valueSictJsonAbbild;
      this.ValueBaumReferenz = valueBaumReferenz;
      this.Referenz = referenz;
    }
  }
}
