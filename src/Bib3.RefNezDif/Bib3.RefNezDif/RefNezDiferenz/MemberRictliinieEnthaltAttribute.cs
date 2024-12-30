// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.MemberRictliinieEnthaltAttribute
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bib3.RefNezDiferenz
{
  public class MemberRictliinieEnthaltAttribute : IZuMemberEntscaidungBinäär
  {
    public readonly Type AttributeType;

    public MemberRictliinieEnthaltAttribute(Type attributeType) => this.AttributeType = attributeType;

    public bool MemberBehandlung(MemberInfo memberInfo)
    {
      foreach (object obj in ((object) memberInfo != null ? CustomAttributeExtensions.GetCustomAttributes(memberInfo) : (IEnumerable<Attribute>) null).EmptyIfNull<Attribute>())
      {
        if (obj.GetType().InheritsOrImplementsOrEquals(this.AttributeType))
          return true;
      }
      return false;
    }
  }
}
