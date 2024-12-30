// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.ZuTypeEntscaidungBinäärEnthalteInAssemblyOderReferenziirte
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bib3.RefNezDiferenz
{
  public class ZuTypeEntscaidungBinäärEnthalteInAssemblyOderReferenziirte : IZuTypeEntscaidungBinäär
  {
    private readonly Assembly[] MengeWurzelAssembly;
    private readonly bool FraigaabeFürTypeAusReferenziirteAssembly;
    private readonly Assembly[] MengeAssembly;

    public static IEnumerable<Assembly> MengeAssemblyBerecne(
      Assembly[] mengeWurzelAssembly,
      bool füügeAinReferenziirteAssembly)
    {
      if (mengeWurzelAssembly == null)
        return (IEnumerable<Assembly>) null;
      List<Assembly> source = new List<Assembly>();
      Queue<Assembly> assemblyQueue = new Queue<Assembly>((IEnumerable<Assembly>) mengeWurzelAssembly);
      Assembly[] AppDomainMengeAssembly = System.AppDomain.CurrentDomain.GetAssemblies();
      while (0 < assemblyQueue.Count)
      {
        Assembly assembly1 = assemblyQueue.Dequeue();
        if (!((Assembly) null == assembly1) && !source.Contains(assembly1))
        {
          source.Add(assembly1);
          if (füügeAinReferenziirteAssembly)
          {
            foreach (Assembly assembly2 in ((IEnumerable<AssemblyName>) assembly1.GetReferencedAssemblies()).Select<AssemblyName, Assembly>((Func<AssemblyName, Assembly>) (assemblyName => ((IEnumerable<Assembly>) AppDomainMengeAssembly).FirstOrDefault<Assembly>((Func<Assembly, bool>) (kandidaatAssembly => string.Equals(kandidaatAssembly.FullName, assemblyName.FullName))))).ToArray<Assembly>())
            {
              if (!((Assembly) null == assembly2))
                ;
              assemblyQueue.Enqueue(assembly2);
            }
          }
        }
      }
      return source.Distinct<Assembly>();
    }

    public ZuTypeEntscaidungBinäärEnthalteInAssemblyOderReferenziirte(
      Assembly wurzelAssembly,
      bool fraigaabeFürTypeAusReferenziirteAssembly)
      : this(new Assembly[1]{ wurzelAssembly }, fraigaabeFürTypeAusReferenziirteAssembly)
    {
    }

    public ZuTypeEntscaidungBinäärEnthalteInAssemblyOderReferenziirte(
      Assembly[] mengeWurzelAssembly,
      bool fraigaabeFürTypeAusReferenziirteAssembly)
    {
      this.MengeWurzelAssembly = mengeWurzelAssembly;
      this.FraigaabeFürTypeAusReferenziirteAssembly = fraigaabeFürTypeAusReferenziirteAssembly;
      IEnumerable<Assembly> source = ZuTypeEntscaidungBinäärEnthalteInAssemblyOderReferenziirte.MengeAssemblyBerecne(mengeWurzelAssembly, fraigaabeFürTypeAusReferenziirteAssembly);
      this.MengeAssembly = source != null ? source.ToArray<Assembly>() : (Assembly[]) null;
    }

    public bool TypeBehandlung(Type type)
    {
      if ((Type) null == type)
        return false;
      Assembly[] mengeAssembly = this.MengeAssembly;
      return mengeAssembly != null && ((IEnumerable<Assembly>) mengeAssembly).Contains<Assembly>(type.Assembly);
    }
  }
}
