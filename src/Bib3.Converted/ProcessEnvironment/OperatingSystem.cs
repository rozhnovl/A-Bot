// Decompiled with JetBrains decompiler
// Type: Bib3.ProcessEnvironment.OperatingSystem
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;

namespace Bib3.ProcessEnvironment
{
  public class OperatingSystem
  {
    public readonly PlatformID Platform;
    public readonly Version Version;
    public readonly string ServicePack;

    public OperatingSystem()
    {
    }

    public OperatingSystem(PlatformID platform, Version version, string servicePack)
    {
      this.Platform = platform;
      this.Version = version;
      this.ServicePack = servicePack;
    }
  }
}
