// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictMeldungValueAstReferenzSictSeriel
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib3.RefNezDiferenz
{
  public class SictMeldungValueAstReferenzSictSeriel
  {
    public KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>[] MengeZuMemberNameAst;
    public long? Referenz;

    public SictMeldungValueAstReferenzSictSeriel()
    {
    }

    public SictMeldungValueAstReferenzSictSeriel(
      KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>[] mengeZuMemberNameAst,
      long? referenz)
    {
      this.MengeZuMemberNameAst = mengeZuMemberNameAst;
      this.Referenz = referenz;
    }

    public static SictMeldungValueAstReferenzSictSeriel Konstrukt(SictWertAst ast)
    {
      if (ast == null)
        return (SictMeldungValueAstReferenzSictSeriel) null;
      IEnumerable<KeyValuePair<string, SictWertAst>> mengeZuMemberNameAst1 = ast.MengeZuMemberNameAst;
      KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>[] mengeZuMemberNameAst2;
      if (mengeZuMemberNameAst1 == null)
      {
        mengeZuMemberNameAst2 = (KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>[]) null;
      }
      else
      {
        IEnumerable<KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>> source = mengeZuMemberNameAst1.Select<KeyValuePair<string, SictWertAst>, KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>>((Func<KeyValuePair<string, SictWertAst>, KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>>) (zuMemberNameAst => new KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>(zuMemberNameAst.Key, SictMeldungValueAstReferenzSictSeriel.Konstrukt(zuMemberNameAst.Value))));
        mengeZuMemberNameAst2 = source != null ? source.ToArray<KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>>() : (KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>[]) null;
      }
      long? referenzTransport = ast.BlatReferenzTransport;
      return new SictMeldungValueAstReferenzSictSeriel(mengeZuMemberNameAst2, referenzTransport);
    }

    public static SictWertAst KonstruktZurük(SictMeldungValueAstReferenzSictSeriel astSictJson)
    {
      if (astSictJson == null)
        return (SictWertAst) null;
      KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>[] mengeZuMemberNameAst1 = astSictJson.MengeZuMemberNameAst;
      KeyValuePair<string, SictWertAst>[] mengeZuMemberNameAst2;
      if (mengeZuMemberNameAst1 == null)
      {
        mengeZuMemberNameAst2 = (KeyValuePair<string, SictWertAst>[]) null;
      }
      else
      {
        IEnumerable<KeyValuePair<string, SictWertAst>> source = ((IEnumerable<KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>>) mengeZuMemberNameAst1).Select<KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>, KeyValuePair<string, SictWertAst>>((Func<KeyValuePair<string, SictMeldungValueAstReferenzSictSeriel>, KeyValuePair<string, SictWertAst>>) (zuMemberNameAst => new KeyValuePair<string, SictWertAst>(zuMemberNameAst.Key, SictMeldungValueAstReferenzSictSeriel.KonstruktZurük(zuMemberNameAst.Value))));
        mengeZuMemberNameAst2 = source != null ? source.ToArray<KeyValuePair<string, SictWertAst>>() : (KeyValuePair<string, SictWertAst>[]) null;
      }
      long? referenz = astSictJson.Referenz;
      return new SictWertAst((IEnumerable<KeyValuePair<string, SictWertAst>>) mengeZuMemberNameAst2, (object) null, referenz);
    }
  }
}
