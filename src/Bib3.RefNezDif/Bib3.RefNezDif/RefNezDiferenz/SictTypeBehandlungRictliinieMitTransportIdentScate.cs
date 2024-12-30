// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;

namespace Bib3.RefNezDiferenz
{
  public class SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer
  {
    private readonly SictIdentInt64Fabrik TypeIdentFabrik = new SictIdentInt64Fabrik(1L);
    public readonly SictMengeTypeBehandlungRictliinie Rictliinie;
    private readonly SictScatenscpaicerDict<Type, int> ScatenscpaicerZuClrTypeTransportIdent = new SictScatenscpaicerDict<Type, int>();
    private readonly SictScatenscpaicerDict<Type, KeyValuePair<int, SictZuTypeBehandlung>> Scatenscpaicer;

    public SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer(
      SictMengeTypeBehandlungRictliinie rictliinie)
    {
      this.Rictliinie = rictliinie;
      this.Scatenscpaicer = new SictScatenscpaicerDict<Type, KeyValuePair<int, SictZuTypeBehandlung>>();
    }

    public int ZuTypeTransportIdent(Type clrType) => this.ScatenscpaicerZuClrTypeTransportIdent.ValueFürKey(clrType, (Func<Type, int>) (t => (int) this.TypeIdentFabrik.IdentBerecne()));

    private void CallbackZuTypeBehanldungErgeebnis(Type clrType, SictZuTypeBehandlung behandlung) => this.Scatenscpaicer.AintraagErsctele(clrType, new KeyValuePair<int, SictZuTypeBehandlung>(this.ZuTypeTransportIdent(clrType), behandlung));

    private KeyValuePair<int, SictZuTypeBehandlung> ZuTypeBehandlungBerecne(Type type) => new KeyValuePair<int, SictZuTypeBehandlung>(this.ZuTypeTransportIdent(type), SictMengeTypeBehandlungRictliinie.ZuTypeBehandlungBerecne(type, this.Rictliinie, new Func<Type, SictZuTypeBehandlung>(this.ZuTypeBehandlung), new Action<Type, SictZuTypeBehandlung>(this.CallbackZuTypeBehanldungErgeebnis)));

    public SictZuTypeBehandlung ZuTypeBehandlung(Type type) => this.ZuTypeIdentUndBehandlung(type).Value;

    public KeyValuePair<int, SictZuTypeBehandlung> ZuTypeIdentUndBehandlung(Type type) => (Type) null == type ? new KeyValuePair<int, SictZuTypeBehandlung>() : this.Scatenscpaicer.ValueFürKey(type, new Func<Type, KeyValuePair<int, SictZuTypeBehandlung>>(this.ZuTypeBehandlungBerecne));

    public static KeyValuePair<int, SictZuTypeBehandlung> ZuTypeIdentUndBehandlung(
      Type type,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinie)
    {
      return rictliinie == null ? new KeyValuePair<int, SictZuTypeBehandlung>() : rictliinie.ZuTypeIdentUndBehandlung(type);
    }

    public static SictZuTypeBehandlung ZuTypeBehandlung(
      Type type,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinie)
    {
      return SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer.ZuTypeIdentUndBehandlung(type, rictliinie).Value;
    }
  }
}
