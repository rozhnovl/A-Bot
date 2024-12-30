// Decompiled with JetBrains decompiler
// Type: Bib3.RefBaumKopii.ZuMemberMengeDelegate
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Fasterflect;
using System;

namespace Bib3.RefBaumKopii
{
  public class ZuMemberMengeDelegate
  {
    public readonly string MemberName;
    public readonly Type MemberType;
    public readonly bool ErfordertKopiiRekurs;
    public readonly Type DeclaringType;
    public readonly MemberGetter Getter;
    public readonly MemberSetter Setter;

    public ZuMemberMengeDelegate(
      Type declaringType,
      string memberName,
      Type memberType,
      bool erfordertKopiiRekurs,
      MemberGetter getter,
      MemberSetter setter)
    {
      this.DeclaringType = declaringType;
      this.MemberName = memberName;
      this.MemberType = memberType;
      this.ErfordertKopiiRekurs = erfordertKopiiRekurs;
      this.Getter = getter;
      this.Setter = setter;
    }
  }
}
