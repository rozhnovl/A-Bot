// Decompiled with JetBrains decompiler
// Type: Bib3.ProcessEnvironment.ProcessEnvironment
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Globalization;

namespace Bib3.ProcessEnvironment
{
  public class ProcessEnvironment
  {
    public static readonly long StatBeginTimeStopwatchMilli = Glob.StopwatchZaitMiliSictInt();
    public static readonly DateTime BeginZaitDateTime = DateTime.Now;
    public static readonly Bib3.ProcessEnvironment.ProcessEnvironment AppDomainEnvironment = Bib3.ProcessEnvironment.ProcessEnvironment.FromCurrentEnvironment();
    public readonly long StartTimeStopwatchMilli;
    public readonly ProcessInfo Process;
    public readonly Version ClrVersion;
    public readonly OperatingSystem OSVersion;
    public readonly bool Is64BitProcess;
    public readonly int ProcessorCount;
    public readonly string MachineName;
    public string UserDefaultLocaleName;
    public string CurrentCultureName;

    public ProcessEnvironment()
    {
    }

    public ProcessEnvironment(
      long startTimeStopwatchMilli,
      ProcessInfo process,
      Version clrVersion,
      OperatingSystem osVersion,
      bool is64BitProcess,
      int processorCount,
      string machineName)
    {
      this.StartTimeStopwatchMilli = startTimeStopwatchMilli;
      this.Process = process;
      this.ClrVersion = clrVersion;
      this.OSVersion = osVersion;
      this.Is64BitProcess = is64BitProcess;
      this.ProcessorCount = processorCount;
      this.MachineName = machineName;
    }

    public static Bib3.ProcessEnvironment.ProcessEnvironment FromCurrentEnvironment()
    {
      long timeStopwatchMilli = Bib3.ProcessEnvironment.ProcessEnvironment.StatBeginTimeStopwatchMilli;
      System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
      ProcessInfo process = currentProcess != null ? currentProcess.Serialized() : (ProcessInfo) null;
      System.Version version = Environment.Version;
      Version clrVersion = (object) version != null ? version.Serialized() : (Version) null;
      System.OperatingSystem osVersion1 = Environment.OSVersion;
      OperatingSystem osVersion2 = osVersion1 != null ? osVersion1.Serialized() : (OperatingSystem) null;
      int num = Environment.Is64BitProcess ? 1 : 0;
      int processorCount = Environment.ProcessorCount;
      string machineName = Environment.MachineName;
      return new Bib3.ProcessEnvironment.ProcessEnvironment(timeStopwatchMilli, process, clrVersion, osVersion2, num != 0, processorCount, machineName)
      {
        CurrentCultureName = CultureInfo.DefaultThreadCurrentCulture?.Name ?? CultureInfo.CurrentCulture?.Name
      };
    }
  }
}
