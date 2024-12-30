// Decompiled with JetBrains decompiler
// Type: Bib3.ObjectChangeToken
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;

namespace Bib3
{
  public class ObjectChangeToken : IDisposable
  {
    public readonly object Object;
    public readonly long TimeoutStopwatchMili;
    public readonly Action<object> ReleaseCallback;

    public ObjectChangeToken(
      object @object,
      long timeoutStopwatchMili,
      Action<object> releaseCallback)
    {
      this.Object = @object;
      this.TimeoutStopwatchMili = timeoutStopwatchMili;
      this.ReleaseCallback = releaseCallback;
    }

    public void Dispose()
    {
      if (this.ReleaseCallback == null)
        return;
      this.ReleaseCallback(this.Object);
    }
  }
}
