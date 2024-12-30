// Decompiled with JetBrains decompiler
// Type: Bib3.AppDomain.AppDomainProxy
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Bib3.AppDomain
{
  public static class AppDomainProxy
  {
    private static readonly List<AppDomainProxy.AssemblyGelaade> MengeAssemblyGelaade = new List<AppDomainProxy.AssemblyGelaade>();

    public static byte[][] MengeAssemblyGelaadeIdentKopii()
    {
      List<AppDomainProxy.AssemblyGelaade> mengeAssemblyGelaade = AppDomainProxy.MengeAssemblyGelaade;
      byte[][] numArray;
      if (mengeAssemblyGelaade == null)
      {
        numArray = (byte[][]) null;
      }
      else
      {
        IEnumerable<byte[]> source = mengeAssemblyGelaade.Select<AppDomainProxy.AssemblyGelaade, byte[]>((Func<AppDomainProxy.AssemblyGelaade, byte[]>) (assemblyGelaadeIdent => (byte[]) assemblyGelaadeIdent.AssemblyIdent.Clone()));
        numArray = source != null ? source.ToArray<byte[]>() : (byte[][]) null;
      }
      return numArray;
    }

    public static byte[] IdentBerecne(byte[] assembly) => assembly == null ? (byte[]) null : new SHA1Managed().ComputeHash(assembly);

    public static Assembly AssemblyLaade(byte[] assembly)
    {
      if (assembly == null)
        return (Assembly) null;
      byte[] AssemblyIdent = AppDomainProxy.IdentBerecne(assembly);
      List<AppDomainProxy.AssemblyGelaade> mengeAssemblyGelaade = AppDomainProxy.MengeAssemblyGelaade;
      AppDomainProxy.AssemblyGelaade assemblyGelaade = mengeAssemblyGelaade != null ? mengeAssemblyGelaade.FirstOrDefault<AppDomainProxy.AssemblyGelaade>((Func<AppDomainProxy.AssemblyGelaade, bool>) (kandidaat => ((IEnumerable<byte>) AssemblyIdent).SequenceEqual<byte>((IEnumerable<byte>) kandidaat.AssemblyIdent))) : (AppDomainProxy.AssemblyGelaade) null;
      if (assemblyGelaade != null)
        return assemblyGelaade.AssemblySctrukt;
      Assembly assemblySctrukt = Assembly.Load(assembly);
      AppDomainProxy.MengeAssemblyGelaade.Add(new AppDomainProxy.AssemblyGelaade((byte[]) null, AssemblyIdent, assemblySctrukt));
      return assemblySctrukt;
    }

    public static byte[] AssemblyZuName(string assemblyName)
    {
      List<AppDomainProxy.AssemblyGelaade> mengeAssemblyGelaade = AppDomainProxy.MengeAssemblyGelaade;
      return (mengeAssemblyGelaade != null ? mengeAssemblyGelaade.FirstOrDefault<AppDomainProxy.AssemblyGelaade>((Func<AppDomainProxy.AssemblyGelaade, bool>) (kandidaat => string.Equals(assemblyName, kandidaat.AssemblySctrukt.GetName().Name, StringComparison.InvariantCultureIgnoreCase))) : (AppDomainProxy.AssemblyGelaade) null)?.Assembly;
    }

    private class AssemblyGelaade
    {
      public readonly byte[] Assembly;
      public readonly byte[] AssemblyIdent;
      public readonly Assembly AssemblySctrukt;

      public AssemblyGelaade(byte[] assembly, byte[] assemblyIdent, Assembly assemblySctrukt)
      {
        this.Assembly = assembly;
        this.AssemblyIdent = assemblyIdent;
        this.AssemblySctrukt = assemblySctrukt;
      }
    }
  }
}
