// Decompiled with JetBrains decompiler
// Type: Bib3.Costura
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bib3
{
  public class Costura
  {
    private const string CosturaAssemblyLoaderTypeName = "AssemblyLoader";

    public static Func<string, Stream> AssemblyResolverCosturaConstruct() => Costura.AssemblyResolverCosturaConstruct((Assembly) null);

    public static Func<string, Stream> AssemblyResolverCosturaConstruct(Assembly costuraAssembly)
    {
      string moduleName = Process.GetCurrentProcess().MainModule.ModuleName;
      Assembly[] assemblyArray;
      if (!((Assembly) null != costuraAssembly))
        assemblyArray = System.AppDomain.CurrentDomain.GetAssemblies();
      else
        assemblyArray = new Assembly[1]{ costuraAssembly };
      Assembly[] source1 = assemblyArray;
      Type[] typeArray;
      if (source1 == null)
      {
        typeArray = (Type[]) null;
      }
      else
      {
        IEnumerable<Type> enumerable = ((IEnumerable<Assembly>) source1).Select<Assembly, Type>((Func<Assembly, Type>) (assembly =>
        {
          try
          {
            return assembly?.GetType("Costura.AssemblyLoader", false, true);
          }
          catch
          {
            return (Type) null;
          }
        }));
        if (enumerable == null)
        {
          typeArray = (Type[]) null;
        }
        else
        {
          IEnumerable<Type> source2 = enumerable.WhereNotDefault<Type>();
          typeArray = source2 != null ? source2.ToArray<Type>() : (Type[]) null;
        }
      }
      Type[] source3 = typeArray;
      Type type1 = source3 != null ? ((IEnumerable<Type>) source3).FirstOrDefault<Type>((Func<Type, bool>) (type => string.Equals(type.Name, "AssemblyLoader", StringComparison.InvariantCultureIgnoreCase) && string.Equals(type.Namespace, nameof (Costura), StringComparison.InvariantCultureIgnoreCase))) : (Type) null;
      if ((Type) null == type1)
        return (Func<string, Stream>) null;
      BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
      object assemblyNames = type1.GetField("assemblyNames", bindingAttr).GetValue((object) null);
      MethodInfo Method = ((IEnumerable<MethodInfo>) type1.GetMethods(bindingAttr)).FirstOrDefault<MethodInfo>((Func<MethodInfo, bool>) (kandidaatMethod =>
      {
        if (!string.Equals("LoadStream", kandidaatMethod.Name, StringComparison.InvariantCultureIgnoreCase))
          return false;
        ParameterInfo[] parameters = kandidaatMethod.GetParameters();
        int? nullable = parameters != null ? new int?(((IEnumerable<ParameterInfo>) parameters).Count<ParameterInfo>()) : new int?();
        return 2 == nullable.GetValueOrDefault() && nullable.HasValue;
      }));
      return (MethodInfo) null == Method ? (Func<string, Stream>) null : (Func<string, Stream>) (assemblyName => Method.Invoke((object) null, new object[2]
      {
        assemblyNames,
        (object) assemblyName.ToLowerInvariant()
      }) as Stream);
    }
  }
}
