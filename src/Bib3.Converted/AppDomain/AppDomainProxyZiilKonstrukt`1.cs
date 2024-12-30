// Decompiled with JetBrains decompiler
// Type: Bib3.AppDomain.AppDomainProxyZiilKonstrukt`1
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;

namespace Bib3.AppDomain
{
  public class AppDomainProxyZiilKonstrukt<T>
  {
    public readonly string AssemblyName;
    public readonly string TypeName;
    public readonly string Exception;
    public readonly T ZiilRefClr;

    public AppDomainProxyZiilKonstrukt(
      string assemblyName,
      string typeName,
      string exception = null,
      T ziilRefClr = default)
    {
      this.AssemblyName = assemblyName;
      this.TypeName = typeName;
      this.Exception = exception;
      this.ZiilRefClr = ziilRefClr;
    }

    public static AppDomainProxyZiilKonstrukt<T> ZiilKonstrukt(string assemblyName, string typeName)
    {
      string exception = (string) null;
      ObjectHandle objectHandle = (ObjectHandle) null;
      T ziilRefClr = default (T);
      try
      {
        Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        Assembly assembly = assemblies != null ? ((IEnumerable<Assembly>) assemblies).FirstOrDefault<Assembly>((Func<Assembly, bool>) (kandidaat => string.Equals(kandidaat.FullName, assemblyName))) : (Assembly) null;
        if ((Assembly) null != assembly)
          ziilRefClr = (T) assembly.CreateInstance(typeName);
        if (objectHandle != null)
          ziilRefClr = (T) objectHandle.Unwrap();
      }
      catch (System.Exception ex)
      {
        exception = ex.SictString();
      }
      return new AppDomainProxyZiilKonstrukt<T>(assemblyName, typeName, exception, ziilRefClr);
    }
  }
}
