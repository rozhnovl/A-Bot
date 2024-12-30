// Decompiled with JetBrains decompiler
// Type: Bib3.ProcessEnvironment.ProcessInfo
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3.ProcessEnvironment
{
  public class ProcessInfo
  {
    public readonly string MainModuleFileFullName;
    public readonly byte[] MainModuleHashSHA1;
    public readonly string CommandLine;

    public ProcessInfo()
    {
    }

    public ProcessInfo(
      string mainModuleFileFullName,
      byte[] mainModuleHashSHA1,
      string commandLine)
    {
      this.MainModuleFileFullName = mainModuleFileFullName;
      this.MainModuleHashSHA1 = mainModuleHashSHA1;
      this.CommandLine = commandLine;
    }
  }
}
