// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.NewtonsoftJson.SictMengeTypeBehandlungRictliinieNewtonsoftJson
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib3.RefNezDiferenz.NewtonsoftJson
{
  public static class SictMengeTypeBehandlungRictliinieNewtonsoftJson
  {
    public static SictMengeTypeBehandlungRictliinie KonstruktMengeTypeBehandlungRictliinie(
      KeyValuePair<Type, Type>[] mengeZuHerkunftTypeZiilType = null,
      IZuTypeEntscaidungBinäär typeAbbildFraigaabeRictliinie = null)
    {
      SictZuTypeBehandlungRictliinie[] mengeZuTypeBehandlungRictliinie = (SictZuTypeBehandlungRictliinie[]) null;
      if (mengeZuHerkunftTypeZiilType != null)
        mengeZuTypeBehandlungRictliinie = ((IEnumerable<KeyValuePair<Type, Type>>) mengeZuHerkunftTypeZiilType).Select<KeyValuePair<Type, Type>, SictZuTypeBehandlungRictliinie>((Func<KeyValuePair<Type, Type>, SictZuTypeBehandlungRictliinie>) (zuHerkunftTypeZiilType => new SictZuTypeBehandlungRictliinie(zuHerkunftTypeZiilType.Key, zuHerkunftTypeZiilType.Value))).ToArray<SictZuTypeBehandlungRictliinie>();
      return new SictMengeTypeBehandlungRictliinie((IZuMemberEntscaidungBinäär) new SictZuMemberBehandlungRictliinieNewtonsoftJson(), mengeZuTypeBehandlungRictliinie, typeAbbildFraigaabeRictliinie);
    }

    public static SictDiferenzSictParam KonstruktSictParam(
      KeyValuePair<Type, Type>[] mengeZuHerkunftTypeZiilType = null,
      IZuTypeEntscaidungBinäär typeAbbildFraigaabeRictliinie = null)
    {
      return new SictDiferenzSictParam(new SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer(SictMengeTypeBehandlungRictliinieNewtonsoftJson.KonstruktMengeTypeBehandlungRictliinie(mengeZuHerkunftTypeZiilType, typeAbbildFraigaabeRictliinie)));
    }
  }
}
