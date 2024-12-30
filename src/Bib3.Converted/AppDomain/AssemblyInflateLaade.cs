// Decompiled with JetBrains decompiler
// Type: Bib3.AppDomain.AssemblyInflateLaade
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3.AppDomain
{
  public class AssemblyInflateLaade : IFuncVoid
  {
    public readonly byte[] Assembly;

    public AssemblyInflateLaade()
    {
    }

    public AssemblyInflateLaade(byte[] assembly) => this.Assembly = assembly;

    public object FüüreAus()
    {
      AppDomainProxy.AssemblyLaade(this.Assembly.DeflateDeKompres(10000000L));
      return (object) null;
    }
  }
}
