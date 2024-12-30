// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictSumeZuTypeAgrInfo
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib3.RefNezDiferenz
{
  public class SictSumeZuTypeAgrInfo
  {
    public readonly int TransportIdent;
    public string ClrName;
    public Type ClrType;
    public SictZuTypeBehandlung TypeBehandlung;
    public readonly IDictionary<int, string> MengeZuMemberIdentClrName = (IDictionary<int, string>) new Dictionary<int, string>();
    public readonly IDictionary<int, SictZuMemberBehandlung> DictZuMemberIdentBehandlung = (IDictionary<int, SictZuMemberBehandlung>) new Dictionary<int, SictZuMemberBehandlung>();
    private static SictScatenscpaicerDict<string, Type> ScatescpaicerVonTypeClrAssemblyQualifiedNameNaacType = new SictScatenscpaicerDict<string, Type>();

    public SictSumeZuTypeAgrInfo(int transportIdent) => this.TransportIdent = transportIdent;

    public static string TypeClrAssemblyQualifiedNameBerecne(Type type) => (Type) null == type ? (string) null : type.AssemblyQualifiedName;

    public static Type TypeAusTypeClrAssemblyQualifiedName(string typeClrAssemblyQualifiedName) => typeClrAssemblyQualifiedName == null ? (Type) null : SictSumeZuTypeAgrInfo.ScatescpaicerVonTypeClrAssemblyQualifiedNameNaacType.ValueFürKey(typeClrAssemblyQualifiedName, new Func<string, Type>(Bib3.Extension.TypeAusTypeClrAssemblyQualifiedNameBerecne));

    public void DictZuMemberIdentBehandlungErgänze(
      IEnumerable<KeyValuePair<int, string>> mengeZuMemberIdentClrName)
    {
      SictZuTypeBehandlung typeBehandlung = this.TypeBehandlung;
      if (typeBehandlung == null)
        return;
      foreach (KeyValuePair<int, string> keyValuePair in mengeZuMemberIdentClrName)
      {
        KeyValuePair<int, string> ZuMemberIdentClrName = keyValuePair;
        SictZuMemberBehandlung[] memberBehandlung1 = typeBehandlung.MengeMemberBehandlung;
        SictZuMemberBehandlung memberBehandlung2 = memberBehandlung1 != null ? ((IEnumerable<SictZuMemberBehandlung>) memberBehandlung1).FirstOrDefault<SictZuMemberBehandlung>((Func<SictZuMemberBehandlung, bool>) (kandidaat => string.Equals(kandidaat.HerkunftMemberName, ZuMemberIdentClrName.Value))) : (SictZuMemberBehandlung) null;
        if (memberBehandlung2 != null)
          this.DictZuMemberIdentBehandlung[ZuMemberIdentClrName.Key] = memberBehandlung2;
      }
    }

    public void AingangAusScritInfo(
      SictMeldungType ausScritInfo,
      Func<string, SictZuTypeBehandlung> callbackZuTypeClrNameTypeBehandlung)
    {
      if (ausScritInfo == null)
        return;
      if (this.ClrName == null && ausScritInfo.ClrName != null)
      {
        this.ClrName = ausScritInfo.ClrName;
        this.ClrType = Bib3.Extension.TypeAusTypeClrAssemblyQualifiedNameBerecne(this.ClrName);
        this.TypeBehandlung = callbackZuTypeClrNameTypeBehandlung(this.ClrName);
        this.DictZuMemberIdentBehandlungErgänze((IEnumerable<KeyValuePair<int, string>>) this.MengeZuMemberIdentClrName);
      }
      KeyValuePair<int, string>[] memberIdentClrName = ausScritInfo.MengeZuMemberIdentClrName;
      if (memberIdentClrName.IsNullOrEmpty())
        return;
      if (this.TypeBehandlung == null)
      {
        foreach (KeyValuePair<int, string> keyValuePair in memberIdentClrName)
          this.MengeZuMemberIdentClrName[keyValuePair.Key] = keyValuePair.Value;
      }
      else
        this.DictZuMemberIdentBehandlungErgänze((IEnumerable<KeyValuePair<int, string>>) memberIdentClrName);
    }
  }
}
