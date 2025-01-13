﻿// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictZuNezSictDiferenzScritAbbild
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System.Collections.Generic;

namespace Bib3.RefNezDiferenz
{
  public class SictZuNezSictDiferenzScritAbbild
  {
    public KeyValuePair<int, SictMeldungType>[] MengeZuTypeMeldung;
    public long[] ListeWurzelReferenz;
    public KeyValuePair<long, SictMeldungObjektDiferenzSeriel>[] MengeZuReferenzDiferenz;
    public long? ScritIndex;
    public long? VerwendeteFrüühesteScritIndex;

    public SictZuNezSictDiferenzScritAbbild()
    {
    }

    public SictZuNezSictDiferenzScritAbbild(
      KeyValuePair<int, SictMeldungType>[] mengeZuTypeMeldung,
      long[] listeWurzelReferenz,
      KeyValuePair<long, SictMeldungObjektDiferenzSeriel>[] mengeZuObjektDiferenz = null,
      long? scritIndex = null,
      long? verwendeteFrüühesteScritIndex = null)
    {
      this.MengeZuTypeMeldung = mengeZuTypeMeldung;
      this.ListeWurzelReferenz = listeWurzelReferenz;
      this.MengeZuReferenzDiferenz = mengeZuObjektDiferenz;
      this.ScritIndex = scritIndex;
      this.VerwendeteFrüühesteScritIndex = verwendeteFrüühesteScritIndex;
    }
  }
}