// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.RictliinieEnthaltAttribute
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;

namespace Bib3.RefNezDiferenz
{
  public class RictliinieEnthaltAttribute : SictMengeTypeBehandlungRictliinie
  {
    public RictliinieEnthaltAttribute(Type attributeType)
      : base((IZuMemberEntscaidungBinäär) new MemberRictliinieEnthaltAttribute(attributeType), typeAbbildFraigaabeRictliinie: (IZuTypeEntscaidungBinäär) new ZuTypeEntscaidungBinäärVonFunc((Func<Type, bool>) (t => true)))
    {
    }
  }
}
