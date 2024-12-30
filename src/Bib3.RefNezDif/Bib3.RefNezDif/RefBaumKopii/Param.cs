// Decompiled with JetBrains decompiler
// Type: Bib3.RefBaumKopii.Param
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Bib3.RefNezDiferenz;
using System;

namespace Bib3.RefBaumKopii
{
  public class Param
  {
    private readonly SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer TypeBehandlungRictliinie;
    public readonly bool ReferenzLaseAus;
    public Profile Profile;

    public SictZuTypeBehandlung ZuTypeBehandlung(Type type) => this.TypeBehandlungRictliinie == null ? (SictZuTypeBehandlung) null : this.TypeBehandlungRictliinie.ZuTypeBehandlung(type);

    public Param(
      Profile profile,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer typeBehandlungRictliinie,
      bool referenzLaseAus = false)
    {
      this.Profile = profile;
      this.TypeBehandlungRictliinie = typeBehandlungRictliinie;
      this.ReferenzLaseAus = referenzLaseAus;
    }
  }
}
