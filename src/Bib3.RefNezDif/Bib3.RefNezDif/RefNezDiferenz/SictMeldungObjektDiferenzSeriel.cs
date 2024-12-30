// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictMeldungObjektDiferenzSeriel
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System.Collections.Generic;

namespace Bib3.RefNezDiferenz
{
  public class SictMeldungObjektDiferenzSeriel
  {
    public int Type;
    public SictMeldungValueOderRefSictSeriel BoxedWert;
    public KeyValuePair<int, SictMeldungValueOderRefSictSeriel>[] MengeZuMemberIdentWert;
    public SictMeldungValueOderRefSictSeriel[] CollectionListeElementWert;
    public byte[] AbbildListeByte;
    public bool SequenzÄnderung;

    public SictMeldungObjektDiferenzSeriel()
    {
    }

    public SictMeldungObjektDiferenzSeriel(
      int typeIdent,
      SictMeldungValueOderRefSictSeriel boxedWert,
      KeyValuePair<int, SictMeldungValueOderRefSictSeriel>[] mengeZuMemberIdentWert,
      SictMeldungValueOderRefSictSeriel[] collectionListeElementWert,
      byte[] abbildListeByte,
      bool sequenzÄnderung)
    {
      this.Type = typeIdent;
      this.BoxedWert = boxedWert;
      this.MengeZuMemberIdentWert = mengeZuMemberIdentWert;
      this.CollectionListeElementWert = collectionListeElementWert;
      this.AbbildListeByte = abbildListeByte;
      this.SequenzÄnderung = sequenzÄnderung;
    }
  }
}
