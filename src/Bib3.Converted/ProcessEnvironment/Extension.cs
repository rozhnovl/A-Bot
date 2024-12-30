// Decompiled with JetBrains decompiler
// Type: Bib3.ProcessEnvironment.Extension
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System.Diagnostics;

namespace Bib3.ProcessEnvironment
{
  public static class Extension
  {
    public static Version Serialized(this System.Version version) => (System.Version) null == version ? (Version) null : new Version(version.Major, version.MajorRevision, version.Minor, version.MinorRevision, version.Build, version.Revision);

    public static OperatingSystem Serialized(this System.OperatingSystem os) => os == null ? (OperatingSystem) null : new OperatingSystem(os.Platform, os.Version.Serialized(), os.ServicePack);

    public static ProcessInfo Serialized(this Process process)
    {
      if (process == null)
        return (ProcessInfo) null;
      string str = (string) null;
      byte[] dataiInhaltHashSHA1 = (byte[]) null;
      string commandLine = (string) null;
      ProcessModule mainModule = process.MainModule;
      ProcessStartInfo startInfo = process.StartInfo;
      if (mainModule != null)
      {
        str = mainModule.FileName;
        Glob.LaadeInhaltAusDataiPfaad(str, out byte[] _, out dataiInhaltHashSHA1);
      }
      if (startInfo != null)
        commandLine = startInfo.Arguments;
      return new ProcessInfo(str, dataiInhaltHashSHA1, commandLine);
    }
  }
}
