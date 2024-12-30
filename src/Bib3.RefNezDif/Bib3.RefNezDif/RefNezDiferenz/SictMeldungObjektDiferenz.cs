// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictMeldungObjektDiferenz
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;

namespace Bib3.RefNezDiferenz
{
  public class SictMeldungObjektDiferenz
  {
    public int Type;
    public SictMeldungValueOderRef BoxedWert;
    public KeyValuePair<int, SictMeldungValueOderRef>[] MengeZuMemberIdentWert;
    public SictMeldungValueOderRef[] CollectionListeElementWert;
    public byte[] AbbildListeByte;
    public bool SequenzÄnderung;

    public bool BerictLeer() => this.Type == 0 && this.BoxedWert == null && this.MengeZuMemberIdentWert.IsNullOrEmpty() && this.CollectionListeElementWert == null;

    public static SictMeldungObjektDiferenz SequenzInterpretiirt(
      int type,
      SictMeldungValueOderRef boxedWert,
      KeyValuePair<int, SictMeldungValueOderRef>[] mengeZuMemberIdentWert,
      SictMeldungValueOderRef[] collectionListeElementWert,
      byte[] abbildListeByte)
    {
      return new SictMeldungObjektDiferenz(type, boxedWert, mengeZuMemberIdentWert.NullIfEmpty<KeyValuePair<int, SictMeldungValueOderRef>[]>(), collectionListeElementWert.NullIfEmpty<SictMeldungValueOderRef[]>(), abbildListeByte.NullIfEmpty<byte[]>(), abbildListeByte != null || collectionListeElementWert != null);
    }

    public SictMeldungObjektDiferenz(
      int type,
      SictMeldungValueOderRef boxedWert,
      KeyValuePair<int, SictMeldungValueOderRef>[] mengeZuMemberIdentWert,
      SictMeldungValueOderRef[] collectionListeElementWert,
      byte[] abbildListeByte,
      bool sequenzÄnderung)
    {
      this.Type = type;
      this.BoxedWert = boxedWert;
      this.MengeZuMemberIdentWert = mengeZuMemberIdentWert;
      this.CollectionListeElementWert = collectionListeElementWert;
      this.AbbildListeByte = abbildListeByte;
      this.SequenzÄnderung = sequenzÄnderung;
    }

    public void MengeReferenzClrVerwendetFüügeAinNaacList(List<object> list)
    {
      if (list == null)
        return;
      SictMeldungValueOderRef boxedWert = this.BoxedWert;
      KeyValuePair<int, SictMeldungValueOderRef>[] zuMemberIdentWert = this.MengeZuMemberIdentWert;
      SictMeldungValueOderRef[] listeElementWert = this.CollectionListeElementWert;
      boxedWert?.MengeReferenzClrVerwendetFüügeAinNaacList(list);
      if (zuMemberIdentWert != null)
      {
        foreach (KeyValuePair<int, SictMeldungValueOderRef> keyValuePair in zuMemberIdentWert)
        {
          if (keyValuePair.Value != null)
            keyValuePair.Value.MengeReferenzClrVerwendetFüügeAinNaacList(list);
        }
      }
      if (listeElementWert == null)
        return;
      foreach (SictMeldungValueOderRef meldungValueOderRef in listeElementWert)
        meldungValueOderRef?.MengeReferenzClrVerwendetFüügeAinNaacList(list);
    }

    public void BerecneTailReferenz(
      Func<object, long> funkReferenzTransportBerecneAusReferenzClr)
    {
      SictMeldungValueOderRef boxedWert = this.BoxedWert;
      KeyValuePair<int, SictMeldungValueOderRef>[] zuMemberIdentWert = this.MengeZuMemberIdentWert;
      SictMeldungValueOderRef[] listeElementWert = this.CollectionListeElementWert;
      boxedWert?.BerecneTailReferenz(funkReferenzTransportBerecneAusReferenzClr);
      if (zuMemberIdentWert != null)
      {
        foreach (KeyValuePair<int, SictMeldungValueOderRef> keyValuePair in zuMemberIdentWert)
        {
          if (keyValuePair.Value != null)
            keyValuePair.Value.BerecneTailReferenz(funkReferenzTransportBerecneAusReferenzClr);
        }
      }
      if (listeElementWert == null)
        return;
      foreach (SictMeldungValueOderRef meldungValueOderRef in listeElementWert)
        meldungValueOderRef?.BerecneTailReferenz(funkReferenzTransportBerecneAusReferenzClr);
    }
  }
}
