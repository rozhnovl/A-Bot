// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictRefNezScritSumeErgeebnis
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib3.RefNezDiferenz
{
  public class SictRefNezScritSumeErgeebnis
  {
    public readonly KeyValuePair<long, object>[] ListeWurzelRefTransportUndRefClr;
    public readonly bool VolsctändigTailMeldungScritIndex;
    public readonly bool Volsctändig;

    public object[] ListeWurzelRefClr
    {
      get
      {
        KeyValuePair<long, object>[] transportUndRefClr = this.ListeWurzelRefTransportUndRefClr;
        object[] listeWurzelRefClr;
        if (transportUndRefClr == null)
        {
          listeWurzelRefClr = (object[]) null;
        }
        else
        {
          IEnumerable<object> source = ((IEnumerable<KeyValuePair<long, object>>) transportUndRefClr).Select<KeyValuePair<long, object>, object>((Func<KeyValuePair<long, object>, object>) (refTransportUndRefClr => refTransportUndRefClr.Value));
          listeWurzelRefClr = source != null ? source.ToArray<object>() : (object[]) null;
        }
        return listeWurzelRefClr;
      }
    }

    public SictRefNezScritSumeErgeebnis(
      KeyValuePair<long, object>[] listeWurzelRefTransportUndRefClr,
      bool volsctändigTailMeldungScritIndex,
      bool volsctändig)
    {
      this.ListeWurzelRefTransportUndRefClr = listeWurzelRefTransportUndRefClr;
      this.VolsctändigTailMeldungScritIndex = volsctändigTailMeldungScritIndex;
      this.Volsctändig = volsctändig;
    }
  }
}
