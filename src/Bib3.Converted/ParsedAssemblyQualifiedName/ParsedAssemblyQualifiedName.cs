// Decompiled with JetBrains decompiler
// Type: ParsedAssemblyQualifiedName.ParsedAssemblyQualifiedName
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ParsedAssemblyQualifiedName
{
  public class ParsedAssemblyQualifiedName
  {
    public Lazy<AssemblyName> AssemblyNameDescriptor;
    public Lazy<Type> FoundType;
    public readonly string AssemblyDescriptionString;
    public readonly string TypeName;
    public readonly string ShortAssemblyName;
    public readonly string Version;
    public readonly string Culture;
    public readonly string PublicKeyToken;
    public readonly List<ParsedAssemblyQualifiedName> GenericParameters = new List<ParsedAssemblyQualifiedName>();
    public readonly Lazy<string> CSharpStyleName;
    public readonly Lazy<string> VBNetStyleName;
    private static readonly Lazy<Assembly[]> Assemblies = new Lazy<Assembly[]>((Func<Assembly[]>) (() => AppDomain.CurrentDomain.GetAssemblies()));

    public ParsedAssemblyQualifiedName(string assemblyQualifiedName)
    {
      if (assemblyQualifiedName == null)
        return;
      int length = -1;
      ParsedAssemblyQualifiedName.block block1 = new ParsedAssemblyQualifiedName.block();
      int num = 0;
      ParsedAssemblyQualifiedName.block block2 = block1;
      for (int index = 0; index < assemblyQualifiedName.Length; ++index)
      {
        char ch = assemblyQualifiedName[index];
        switch (ch)
        {
          case '[':
            if (assemblyQualifiedName[index + 1] == ']')
            {
              ++index;
              break;
            }
            ++num;
            ParsedAssemblyQualifiedName.block block3 = new ParsedAssemblyQualifiedName.block()
            {
              iStart = index + 1,
              level = num,
              parentBlock = block2
            };
            block2.innerBlocks.Add(block3);
            block2 = block3;
            break;
          case ']':
            block2.iEnd = index - 1;
            if (assemblyQualifiedName[block2.iStart] != '[')
            {
              block2.parsedAssemblyQualifiedName = new ParsedAssemblyQualifiedName(assemblyQualifiedName.Substring(block2.iStart, index - block2.iStart));
              if (num == 2)
                this.GenericParameters.Add(block2.parsedAssemblyQualifiedName);
            }
            block2 = block2.parentBlock;
            --num;
            break;
          default:
            if (num == 0 && ch == ',')
            {
              length = index;
              goto label_16;
            }
            else
              break;
        }
      }
label_16:
      this.TypeName = assemblyQualifiedName.Substring(0, length);
      this.CSharpStyleName = new Lazy<string>((Func<string>) (() => this.LanguageStyle("<", ">")));
      this.VBNetStyleName = new Lazy<string>((Func<string>) (() => this.LanguageStyle("(Of ", ")")));
      this.AssemblyDescriptionString = assemblyQualifiedName.Substring(length + 2);
      List<string> list = ((IEnumerable<string>) this.AssemblyDescriptionString.Split(',')).Select<string, string>((Func<string, string>) (x => x.Trim())).ToList<string>();
      this.Version = ParsedAssemblyQualifiedName.LookForPairThenRemove(list, nameof (Version));
      this.Culture = ParsedAssemblyQualifiedName.LookForPairThenRemove(list, nameof (Culture));
      this.PublicKeyToken = ParsedAssemblyQualifiedName.LookForPairThenRemove(list, nameof (PublicKeyToken));
      if (list.Count > 0)
        this.ShortAssemblyName = list[0];
      this.AssemblyNameDescriptor = new Lazy<AssemblyName>((Func<AssemblyName>) (() => new AssemblyName(this.AssemblyDescriptionString)));
      this.FoundType = new Lazy<Type>((Func<Type>) (() =>
      {
        Type type1 = Type.GetType(assemblyQualifiedName);
        if (type1 != (Type) null)
          return type1;
        foreach (Assembly assembly in ParsedAssemblyQualifiedName.Assemblies.Value)
        {
          Type type2 = assembly.GetType(assemblyQualifiedName);
          if (type2 != (Type) null)
            return type2;
        }
        return (Type) null;
      }));
    }

    internal string LanguageStyle(string prefix, string suffix)
    {
      if (this.GenericParameters.Count <= 0)
        return this.TypeName;
      StringBuilder stringBuilder = new StringBuilder(this.TypeName.Substring(0, this.TypeName.IndexOf('`')));
      stringBuilder.Append(prefix);
      bool flag = false;
      foreach (ParsedAssemblyQualifiedName genericParameter in this.GenericParameters)
      {
        if (flag)
          stringBuilder.Append(", ");
        stringBuilder.Append(genericParameter.LanguageStyle(prefix, suffix));
        flag = true;
      }
      stringBuilder.Append(suffix);
      return stringBuilder.ToString();
    }

    private static string LookForPairThenRemove(List<string> strings, string name)
    {
      for (int index = 0; index < strings.Count; ++index)
      {
        string str1 = strings[index];
        if (str1.IndexOf(name) == 0)
        {
          int num = str1.IndexOf('=');
          if (num > 0)
          {
            string str2 = str1.Substring(num + 1);
            strings.RemoveAt(index);
            return str2;
          }
        }
      }
      return (string) null;
    }

    public override string ToString() => this.CSharpStyleName.ToString();

    private class block
    {
      internal int iStart;
      internal int iEnd;
      internal int level;
      internal ParsedAssemblyQualifiedName.block parentBlock;
      internal List<ParsedAssemblyQualifiedName.block> innerBlocks = new List<ParsedAssemblyQualifiedName.block>();
      internal ParsedAssemblyQualifiedName parsedAssemblyQualifiedName;
    }
  }
}
