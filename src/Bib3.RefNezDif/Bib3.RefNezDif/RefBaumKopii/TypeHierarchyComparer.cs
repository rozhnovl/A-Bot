// Decompiled with JetBrains decompiler
// Type: Bib3.RefBaumKopii.TypeHierarchyComparer
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;

namespace Bib3.RefBaumKopii
{
  public class TypeHierarchyComparer : IComparer<Type>
  {
    public int Compare(Type o0, Type o1)
    {
      if (o0 == o1 || (Type) null == o0 || (Type) null == o1)
        return 0;
      if (o0.IsAssignableFrom(o1))
        return 1;
      return o1.IsAssignableFrom(o0) ? -1 : 0;
    }
  }
}
