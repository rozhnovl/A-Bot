// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.Typename
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Text.RegularExpressions;

namespace Bib3.RefNezDiferenz
{
  public static class Typename
  {
    private static readonly SictScatenscpaicerDict<string, string> DictVonTypeAssemblyQualifiedNameNaacNameCast = new SictScatenscpaicerDict<string, string>();

    public static string TypenameFürCast(Type type) => (Type) null == type ? (string) null : Typename.TypenameFürCast(type.AssemblyQualifiedName);

    public static string TypenameFürCast(string assemblyQualifiedName) => assemblyQualifiedName == null ? (string) null : Typename.DictVonTypeAssemblyQualifiedNameNaacNameCast.ValueFürKey(assemblyQualifiedName, (Func<string, string>) (t => Typename.TypenameFürCastKonstrukt(assemblyQualifiedName)));

    public static string TypenameFürCastKonstrukt(string assemblyQualifiedName) => Regex.Replace(new ParsedAssemblyQualifiedName.ParsedAssemblyQualifiedName(assemblyQualifiedName).CSharpStyleName.Value, "\\+", ".");
  }
}
