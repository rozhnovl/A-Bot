// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SequenzFunk
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Bib3.AppDomain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Bib3.RefNezDiferenz
{
  public static class SequenzFunk
  {
    private static readonly SictScatenscpaicerDict<Type, SequenzFunk.ZuElementTypeMethod> DictZuElementTypeAssembly = new SictScatenscpaicerDict<Type, SequenzFunk.ZuElementTypeMethod>();
    private static readonly SictScatenscpaicerDict<Type, MethodInfo> DictZuElementTypeMethodToArray = new SictScatenscpaicerDict<Type, MethodInfo>();
    private static readonly string SequenzFunkTypeName = typeof (SequenzFunk).FullName;
    private static readonly string SequenzTypeName = typeof (Sequenz).FullName;
    private static readonly string IEnumerableTypeName = typeof (IEnumerable).FullName;
    private const string InterfaceMethodKopiireName = "Kopiire";
    private const string InterfaceMethodVerglaicName = "Verglaic";
    private const string TypeNameArrayRegexGroupBegin = "begin";
    private const string TypeNameArrayRegexGroupArray = "array";
    private static readonly Regex TypeNameArrayRegex = new Regex("(?<begin>.*)(?<array>(\\[\\])+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static Stream AssemblyAusAppDomainProxy(string assemblyName)
    {
      byte[] buffer = AppDomainProxy.AssemblyZuName(assemblyName);
      return buffer == null ? (Stream) null : (Stream) new MemoryStream(buffer);
    }

    static SequenzFunk()
    {
      Compile.MengeAssemblyResolver.Add(new Func<string, Stream>(SequenzFunk.AssemblyAusAppDomainProxy));
      Compile.MengeAssemblyResolver.Add(Costura.AssemblyResolverCosturaConstruct());
      Compile.MengeAssemblyResolver.Add(Compile.AssemblyResolverLoadedAssemblyCodeBaseConstruct());
    }

    public static Array ArrayKopiireNaacGröösere(Array zuKopiire, int listeElementAnzaal)
    {
      if (zuKopiire == null)
        return (Array) null;
      Array instance = Array.CreateInstance(zuKopiire.GetType().GetElementType(), Math.Max(listeElementAnzaal, zuKopiire.Length));
      zuKopiire.CopyTo(instance, 0);
      return instance;
    }

    private static string AusdrukArrayCreate(Type elementType, int listeElementAnzaal)
    {
      string input = Typename.TypenameFürCast(elementType);
      string str1 = input;
      string str2 = (string) null;
      Match match = SequenzFunk.TypeNameArrayRegex.Match(input);
      if (match.Success)
      {
        str1 = match.Groups["begin"].Value;
        str2 = match.Groups["array"].Value;
      }
      return "new " + str1 + "[" + (object) listeElementAnzaal + "]" + str2;
    }

    public static string KopiireMethodCSFürElementType(Type elementType)
    {
      if ((Type) null == elementType)
        return (string) null;
      string str1 = Typename.TypenameFürCast(elementType);
      string str2 = "ListeElement";
      string str3 = "ListeElementAnzaal";
      string str4 = "Enumerable";
      string str5 = "Element";
      string str6 = str1 + "[]";
      return string.Join(Environment.NewLine, new string[14]
      {
        "static public " + SequenzFunk.SequenzTypeName + " Kopiire(" + SequenzFunk.IEnumerableTypeName + " " + str4 + ")",
        "{",
        "var " + str2 + " = " + SequenzFunk.AusdrukArrayCreate(elementType, 4) + ";",
        "int " + str3 + " = 0;",
        "foreach(var " + str5 + " in " + str4 + ")",
        "{",
        "if(" + str2 + ".Length <= " + str3 + ")",
        "{",
        str2 + " = (" + str6 + ")" + SequenzFunk.SequenzFunkTypeName + ".ArrayKopiireNaacGröösere(" + str2 + "," + str3 + " * 2);",
        "}",
        str2 + "[" + str3 + "++] = (" + str1 + ")" + str5 + ";",
        "}",
        "return new " + SequenzFunk.SequenzTypeName + "(" + str2 + ", " + str3 + ");",
        "}"
      });
    }

    public static string VerglaicMethodCSFürElementType(Type elementType)
    {
      if ((Type) null == elementType)
        return (string) null;
      string str1 = "O0";
      string str2 = "O1";
      string str3 = str1 + "Array";
      string str4 = str2 + "Array";
      string str5 = Typename.TypenameFürCast(elementType) + "[]";
      return string.Join(Environment.NewLine, new string[13]
      {
        "static public bool Verglaic(" + SequenzFunk.SequenzTypeName + " " + str1 + ", " + SequenzFunk.SequenzTypeName + " " + str2 + ")",
        "{",
        "var " + str3 + " = (" + str5 + ")" + str1 + ".ListeElement;",
        "var " + str4 + " = (" + str5 + ")" + str2 + ".ListeElement;",
        "for(var i = 0; i < " + str1 + ".ListeElementAnzaal; ++i)",
        "{",
        "if(!object.Equals(" + str3 + "[i], " + str4 + "[i]))",
        "{",
        "return false;",
        "}",
        "}",
        "return true;",
        "}"
      });
    }

    public static string TypeCSFürElementType(Type elementType) => string.Join(Environment.NewLine, new string[6]
    {
      "static public class CSType",
      "{",
      SequenzFunk.KopiireMethodCSFürElementType(elementType),
      "",
      SequenzFunk.VerglaicMethodCSFürElementType(elementType),
      "}"
    });

    private static IEnumerable<string> FürCodeProviderMengeAssemblyName(Assembly assembly)
    {
      if (!((Assembly) null == assembly))
      {
        string CodeBase = assembly.CodeBase;
        if (CodeBase != null)
          yield return Path.GetFileName(CodeBase);
        yield return assembly.GetName().Name + ".dll";
      }
    }

    private static SequenzFunk.ZuElementTypeMethod AssemblyFürElementTypeKonstrukt(Type elementType) => throw new NotImplementedException();

    private static SequenzFunk.ZuElementTypeMethod AssemblyFürElementType(Type elementType)
    {
      if ((Type) null == elementType)
        return (SequenzFunk.ZuElementTypeMethod) null;
      try
      {
        return SequenzFunk.DictZuElementTypeAssembly.ValueFürKey(elementType, (Func<Type, SequenzFunk.ZuElementTypeMethod>) (t => SequenzFunk.AssemblyFürElementTypeKonstrukt(elementType)));
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Type=" + elementType.FullName, ex);
      }
    }

    private static MethodInfo MethodToArrayFürElementType(Type elementType)
    {
      if ((Type) null == elementType)
        return (MethodInfo) null;
      try
      {
        return SequenzFunk.DictZuElementTypeMethodToArray.ValueFürKey(elementType, (Func<Type, MethodInfo>) (t => SequenzFunk.MethodToArrayFürElementTypeKonstrukt(elementType)));
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Type=" + elementType.FullName, ex);
      }
    }

    private static MethodInfo MethodToArrayFürElementTypeKonstrukt(Type elementType)
    {
      if ((Type) null == elementType)
        return (MethodInfo) null;
      return typeof (Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(elementType);
    }

    private static MethodInfo KopiireMethodFürElementType(Type elementType) => SequenzFunk.AssemblyFürElementType(elementType).MethodKopiire;

    private static MethodInfo VerglaicMethodFürElementType(Type elementType) => SequenzFunk.AssemblyFürElementType(elementType).MethodVerglaic;

    public static Sequenz SequenzKopiire(IEnumerable enumerable)
    {
      if (enumerable == null)
        return (Sequenz) null;
      if (enumerable is Array array)
        return new Sequenz(array.ArraySegment(0, array.Length), array.Length);
      Type[] source = enumerable.GetType().ListeTypeArgumentZuBaseOderInterface(typeof (IEnumerable<>));
      Type type = source != null ? ((IEnumerable<Type>) source).FirstOrDefault<Type>() : (Type) null;
      if ((object) type == null)
        type = typeof (object);
      Type elementType = type;
      Array listeElement;
      if ((Type) null == elementType)
        listeElement = (Array) enumerable.Cast<object>().ToArray<object>();
      else
        listeElement = (Array) SequenzFunk.MethodToArrayFürElementType(elementType).Invoke((object) null, new object[1]
        {
          (object) enumerable
        });
      return new Sequenz(listeElement, listeElement.Length);
    }

    public static bool SequenzPrüüfeGlaicwertig(Sequenz o0, Sequenz o1)
    {
      if (o0 == o1)
        return true;
      if (o0 == null || o1 == null || o0.ListeElementAnzaal != o1.ListeElementAnzaal || o0.ListeElement == null || o1.ListeElement == null)
        return false;
      o0.ListeElement.GetType().GetElementType();
      for (int index = 0; index < o0.ListeElementAnzaal; ++index)
      {
        if (!object.Equals(o0.ListeElement.GetValue(index), o1.ListeElement.GetValue(index)))
          return false;
      }
      return true;
    }

    private class ZuElementTypeMethod
    {
      public readonly MethodInfo MethodKopiire;
      public readonly MethodInfo MethodVerglaic;

      public ZuElementTypeMethod(MethodInfo methodKopiire, MethodInfo methodVerglaic)
      {
        this.MethodKopiire = methodKopiire;
        this.MethodVerglaic = methodVerglaic;
      }
    }
  }
}
