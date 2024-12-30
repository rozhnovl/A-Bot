// Decompiled with JetBrains decompiler
// Type: Bib3.StringEqualityComparerCaseInsensitive
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;

namespace Bib3
{
  public class StringEqualityComparerCaseInsensitive : IEqualityComparer<string>
  {
    bool IEqualityComparer<string>.Equals(string x, string y) => string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase);

    int IEqualityComparer<string>.GetHashCode(string obj) => obj == null ? 0 : obj.ToLowerInvariant().GetHashCode();
  }
}
