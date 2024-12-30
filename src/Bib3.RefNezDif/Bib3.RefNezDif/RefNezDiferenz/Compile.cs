// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.Compile
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Bib3.RefNezDiferenz
{
  public class Compile
  {
    public static readonly List<Func<string, Stream>> MengeAssemblyResolver = new List<Func<string, Stream>>();
    private static Regex FürCodeProviderAssemblyNameFileRegex = new Regex(Regex.Escape("file:///") + "(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static Stream GetAssembly(string assemblyName)
    {
      List<Func<string, Stream>> assemblyResolver = Compile.MengeAssemblyResolver;
      Stream assembly1;
      if (assemblyResolver == null)
      {
        assembly1 = (Stream) null;
      }
      else
      {
        IEnumerable<Func<string, Stream>> source1 = assemblyResolver.WhereNotDefault<Func<string, Stream>>();
        if (source1 == null)
        {
          assembly1 = (Stream) null;
        }
        else
        {
          IEnumerable<Stream> source2 = source1.Select<Func<string, Stream>, Stream>((Func<Func<string, Stream>, Stream>) (resolver => resolver(assemblyName)));
          assembly1 = source2 != null ? source2.FirstOrDefault<Stream>((Func<Stream, bool>) (assembly => assembly != null)) : (Stream) null;
        }
      }
      return assembly1;
    }

    public static string FilePathFromAssemblyCodeBase(string codeBase)
    {
      if (codeBase == null)
        return (string) null;
      Match match = Compile.FürCodeProviderAssemblyNameFileRegex.Match(codeBase);
      if (match.Success)
        return match.Groups[1].Value;
      throw new NotImplementedException();
    }

    public static Func<string, Stream> AssemblyResolverLoadedAssemblyCodeBaseConstruct(
      Action<Exception> callbackException = null)
    {
      return (Func<string, Stream>) (assemblyName =>
      {
        Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        Assembly assembly = assemblies != null ? ((IEnumerable<Assembly>) assemblies).FirstOrDefault<Assembly>((Func<Assembly, bool>) (kandidaatAssembly => string.Equals(kandidaatAssembly.GetName().FullName, assemblyName, StringComparison.InvariantCultureIgnoreCase) || string.Equals(kandidaatAssembly.GetName().Name, assemblyName, StringComparison.InvariantCultureIgnoreCase))) : (Assembly) null;
        if ((Assembly) null == assembly)
          return (Stream) null;
        string path = Compile.FilePathFromAssemblyCodeBase(assembly.CodeBase);
        try
        {
          return (Stream) new FileStream(path, FileMode.Open, FileAccess.Read);
        }
        catch (Exception ex)
        {
          if (callbackException != null)
            callbackException(ex);
          return (Stream) null;
        }
      });
    }
  }
}
