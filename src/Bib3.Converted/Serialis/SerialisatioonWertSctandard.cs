// Decompiled with JetBrains decompiler
// Type: Bib3.Serialis.SerialisatioonWertSctandard
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;

namespace Bib3.Serialis
{
  public class SerialisatioonWertSctandard : Attribute
  {
    public readonly object WertSctandard;

    public SerialisatioonWertSctandard(object wertSctandard) => this.WertSctandard = wertSctandard;
  }
}
