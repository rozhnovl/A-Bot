// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictWertAst
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Fasterflect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib3.RefNezDiferenz
{
  public class SictWertAst
  {
    public readonly IEnumerable<KeyValuePair<string, SictWertAst>> MengeZuMemberNameAst;
    public readonly object BlatReferenzClr;

    public long? BlatReferenzTransport { private set; get; }

    public SictWertAst(
      IEnumerable<KeyValuePair<string, SictWertAst>> mengeZuMemberNameAst,
      object blatReferenzClr,
      long? blatReferenzTransport = null)
    {
      this.MengeZuMemberNameAst = mengeZuMemberNameAst;
      this.BlatReferenzClr = blatReferenzClr;
      this.BlatReferenzTransport = blatReferenzTransport;
    }

    public void BerecneTailReferenz(
      Func<object, long> funkReferenzTransportBerecneAusReferenzClr)
    {
      object blatReferenzClr = this.BlatReferenzClr;
      IEnumerable<KeyValuePair<string, SictWertAst>> mengeZuMemberNameAst = this.MengeZuMemberNameAst;
      if (blatReferenzClr != null)
        this.BlatReferenzTransport = new long?(funkReferenzTransportBerecneAusReferenzClr(blatReferenzClr));
      if (mengeZuMemberNameAst == null)
        return;
      foreach (KeyValuePair<string, SictWertAst> keyValuePair in mengeZuMemberNameAst)
      {
        if (keyValuePair.Value != null)
          keyValuePair.Value.BerecneTailReferenz(funkReferenzTransportBerecneAusReferenzClr);
      }
    }

    public void MengeReferenzClrVerwendetFüügeAinNaacList(List<object> list)
    {
      if (list == null)
        return;
      object blatReferenzClr = this.BlatReferenzClr;
      if (blatReferenzClr != null)
        list.Add(blatReferenzClr);
      IEnumerable<KeyValuePair<string, SictWertAst>> mengeZuMemberNameAst = this.MengeZuMemberNameAst;
      if (mengeZuMemberNameAst == null)
        return;
      foreach (KeyValuePair<string, SictWertAst> keyValuePair in mengeZuMemberNameAst)
      {
        if (keyValuePair.Value != null)
          keyValuePair.Value.MengeReferenzClrVerwendetFüügeAinNaacList(list);
      }
    }

    public static SictWertAst KonstruktFürStruct(
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieScatescpaicer,
      object boxedStruct)
    {
      if (boxedStruct == null)
        return (SictWertAst) null;
      object obj1 = ValueTypeExtensions.WrapIfValueType(boxedStruct);
      SictZuTypeBehandlung zuTypeBehandlung1 = SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer.ZuTypeBehandlung(boxedStruct.GetType(), rictliinieScatescpaicer);
      if (zuTypeBehandlung1 == null)
        return (SictWertAst) null;
      SictZuMemberBehandlung[] memberBehandlung1 = zuTypeBehandlung1.MengeMemberBehandlung;
      List<KeyValuePair<string, SictWertAst>> mengeZuMemberNameAst = new List<KeyValuePair<string, SictWertAst>>();
      if (memberBehandlung1 != null)
      {
        foreach (SictZuMemberBehandlung memberBehandlung2 in memberBehandlung1)
        {
          string herkunftMemberName = memberBehandlung2.HerkunftMemberName;
          object obj2 = memberBehandlung2.HerkunftTypeMemberGetter?.Invoke(obj1);
          if (obj2 != null)
          {
            SictZuTypeBehandlung zuTypeBehandlung2 = SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer.ZuTypeBehandlung(obj2?.GetType(), rictliinieScatescpaicer);
            if ((zuTypeBehandlung2 != null ? (zuTypeBehandlung2.ErfordertKopiiRekurs ? 1 : 0) : 0) != 0)
            {
              SictWertAst sictWertAst;
              if (zuTypeBehandlung2.BehandlungAlsReferenz)
              {
                if (obj2 != null)
                  ;
                sictWertAst = new SictWertAst((IEnumerable<KeyValuePair<string, SictWertAst>>) null, obj2);
              }
              else
                sictWertAst = SictWertAst.KonstruktFürStruct(rictliinieScatescpaicer, obj2);
              mengeZuMemberNameAst.Add(new KeyValuePair<string, SictWertAst>(herkunftMemberName, sictWertAst));
            }
          }
        }
      }
      return new SictWertAst((IEnumerable<KeyValuePair<string, SictWertAst>>) mengeZuMemberNameAst, (object) null);
    }

    public void BlatReferenzTransportBerecne(
      Func<object, long> funkBlatReferenzTransportBerecne)
    {
      IEnumerable<KeyValuePair<string, SictWertAst>> mengeZuMemberNameAst = this.MengeZuMemberNameAst;
      object blatReferenzClr = this.BlatReferenzClr;
      if (blatReferenzClr != null)
        this.BlatReferenzTransport = funkBlatReferenzTransportBerecne != null ? new long?(funkBlatReferenzTransportBerecne(blatReferenzClr)) : throw new ArgumentNullException("FunkBlatReferenzTransportBerecne");
      if (mengeZuMemberNameAst == null)
        return;
      foreach (KeyValuePair<string, SictWertAst> keyValuePair in mengeZuMemberNameAst)
      {
        if (keyValuePair.Value != null)
          keyValuePair.Value.BlatReferenzTransportBerecne(funkBlatReferenzTransportBerecne);
      }
    }

    public void Appliziire(
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieScatescpaicer,
      ref object ziilObjektReferenzClr,
      Func<long, object> funkAusReferenzTransportBerecneReferenzClr)
    {
      if (funkAusReferenzTransportBerecneReferenzClr == null)
        throw new ArgumentNullException("FunkAusReferenzTransportBerecneReferenzClr");
      if (ziilObjektReferenzClr == null)
        throw new ArgumentNullException("ZiilObjektReferenzClr");
      IEnumerable<KeyValuePair<string, SictWertAst>> mengeZuMemberNameAst = this.MengeZuMemberNameAst;
      if (mengeZuMemberNameAst == null)
        return;
      Type type = ziilObjektReferenzClr.GetType();
      object obj1 = ValueTypeExtensions.WrapIfValueType(ziilObjektReferenzClr);
      SictZuTypeBehandlung zuTypeBehandlung = rictliinieScatescpaicer != null ? rictliinieScatescpaicer.ZuTypeBehandlung(type) : throw new ArgumentNullException("RictliinieScatescpaicer");
      if (zuTypeBehandlung == null)
        return;
      SictZuMemberBehandlung[] memberBehandlung1 = zuTypeBehandlung.MengeMemberBehandlung;
      foreach (KeyValuePair<string, SictWertAst> keyValuePair in mengeZuMemberNameAst)
      {
        string MemberNaame = keyValuePair.Key;
        SictWertAst sictWertAst = keyValuePair.Value;
        if (MemberNaame != null && sictWertAst != null)
        {
          SictZuMemberBehandlung memberBehandlung2 = ((IEnumerable<SictZuMemberBehandlung>) memberBehandlung1).FirstOrDefault<SictZuMemberBehandlung>((Func<SictZuMemberBehandlung, bool>) (kandidaatMemberBehandlung => string.Equals(kandidaatMemberBehandlung.HerkunftMemberName, MemberNaame)));
          MemberSetter memberSetter = memberBehandlung2 != null ? memberBehandlung2.ZiilTypeMemberSetter : throw new ArgumentNullException("MemberBehandlung");
          MemberGetter typeMemberGetter = memberBehandlung2.HerkunftTypeMemberGetter;
          long? referenzTransport = sictWertAst.BlatReferenzTransport;
          if (referenzTransport.HasValue)
          {
            object obj2 = funkAusReferenzTransportBerecneReferenzClr(referenzTransport.Value);
            memberSetter.Invoke(obj1, obj2);
          }
          else if (sictWertAst.MengeZuMemberNameAst != null)
          {
            object ziilObjektReferenzClr1 = typeMemberGetter.Invoke(obj1);
            sictWertAst.Appliziire(rictliinieScatescpaicer, ref ziilObjektReferenzClr1, funkAusReferenzTransportBerecneReferenzClr);
            memberSetter.Invoke(obj1, ValueTypeExtensions.UnwrapIfWrapped(ziilObjektReferenzClr1));
          }
        }
      }
      ziilObjektReferenzClr = ValueTypeExtensions.UnwrapIfWrapped(obj1);
    }
  }
}
