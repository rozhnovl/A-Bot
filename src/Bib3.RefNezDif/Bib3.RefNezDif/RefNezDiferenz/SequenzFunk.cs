// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SequenzFunk
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System.Collections;
using System.Reflection;

namespace Bib3.RefNezDiferenz
{
  public static class SequenzFunk
  {
    private static readonly SictScatenscpaicerDict<Type, SequenzFunk.ZuElementTypeMethod> DictZuElementTypeAssembly = new SictScatenscpaicerDict<Type, SequenzFunk.ZuElementTypeMethod>();
    private static readonly SictScatenscpaicerDict<Type, MethodInfo> DictZuElementTypeMethodToArray = new SictScatenscpaicerDict<Type, MethodInfo>();

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
