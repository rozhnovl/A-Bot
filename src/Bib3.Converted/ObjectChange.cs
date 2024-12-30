// Decompiled with JetBrains decompiler
// Type: Bib3.ObjectChange
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;

namespace Bib3
{
  public class ObjectChange
  {
    private IDictionary<object, ObjectChangeToken> Dict = (IDictionary<object, ObjectChangeToken>) new Dictionary<object, ObjectChangeToken>();

    private void ReleaseCallback(object @object)
    {
      ObjectChangeToken objectChangeToken = (ObjectChangeToken) null;
      if (!this.Dict.TryGetValue(@object, out objectChangeToken))
        return;
      this.Dict.Remove(@object);
    }

    public ObjectChangeToken GetChangeToken(object @object, int leebensdauerScrankeMili = 0)
    {
      long num = Glob.StopwatchZaitMiliSictInt();
      ObjectChangeToken objectChangeToken = (ObjectChangeToken) null;
      if (this.Dict.TryGetValue(@object, out objectChangeToken) && objectChangeToken != null && num < objectChangeToken.TimeoutStopwatchMili)
        return (ObjectChangeToken) null;
      ObjectChangeToken changeToken = new ObjectChangeToken(@object, num + (long) leebensdauerScrankeMili, new Action<object>(this.ReleaseCallback));
      this.Dict[@object] = changeToken;
      return changeToken;
    }
  }
}
