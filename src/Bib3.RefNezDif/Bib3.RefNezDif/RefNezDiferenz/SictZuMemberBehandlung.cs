// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictZuMemberBehandlung
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Fasterflect;
using System;

namespace Bib3.RefNezDiferenz
{
  public class SictZuMemberBehandlung
  {
    public readonly string HerkunftMemberName;
    public readonly Type HerkunftMemberType;
    public readonly Type HerkunftMemberDeclaringType;
    public readonly MemberGetter HerkunftTypeMemberGetter;
    public readonly MemberSetter ZiilTypeMemberSetter;
    public readonly int SictDiferenzTransportMemberIdent;

    public SictZuMemberBehandlung(
      Type herkunftMemberDeclaringType,
      string herkunftMemberName,
      Type herkunftMemberType,
      MemberGetter herkunftTypeMemberGetter,
      MemberSetter ziilTypeMemberSetter,
      int sictDiferenzTransportMemberIdent = -1)
    {
      this.HerkunftMemberDeclaringType = herkunftMemberDeclaringType;
      this.HerkunftMemberName = herkunftMemberName;
      this.HerkunftMemberType = herkunftMemberType;
      this.HerkunftTypeMemberGetter = herkunftTypeMemberGetter;
      this.ZiilTypeMemberSetter = ziilTypeMemberSetter;
      this.SictDiferenzTransportMemberIdent = sictDiferenzTransportMemberIdent;
    }
  }
}
