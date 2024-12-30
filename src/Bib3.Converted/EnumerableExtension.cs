// Decompiled with JetBrains decompiler
// Type: Bib3.EnumerableExtension
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bib3
{
  public static class EnumerableExtension
  {
    private static readonly MethodInfo EnumerableCastMethod = typeof (Enumerable).GetMethod("Cast");
    private static readonly MethodInfo EnumeratorCastMethod = typeof (EnumerableExtension).GetMethod("Cast");

    public static IEnumerable MakeGenericMethodCast(this IEnumerable seq, Type type)
    {
      MethodInfo methodInfo = EnumerableExtension.EnumerableCastMethod.MakeGenericMethod(type);
      object obj;
      if ((object) methodInfo == null)
      {
        obj = (object) null;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        obj = (methodInfo.Invoke((object) null, (object[]) new IEnumerable[1]
        {
          seq
        }));
      }
      return obj as IEnumerable;
    }

    public static IEnumerator MakeGenericMethodCast(this IEnumerator enumerator, Type type) => EnumerableExtension.EnumeratorCastMethod.MakeGenericMethod(type).Invoke((object) null, (object[]) new IEnumerator[1]
    {
      enumerator
    }) as IEnumerator;

    public static IEnumerator<T> Cast<T>(this IEnumerator enumerator)
    {
      while (enumerator.MoveNext())
        yield return (T) enumerator.Current;
    }

    public static IEnumerable AsIEnumerable(this IEnumerator enumerator)
    {
      while (enumerator.MoveNext())
        yield return enumerator.Current;
    }

    public static IEnumerable<T> AsIEnumerable<T>(this IEnumerator<T> enumerator)
    {
      while (enumerator.MoveNext())
        yield return enumerator.Current;
    }

    public static bool ContainsAny<T>(this IEnumerable<T> container, IEnumerable<T> set) => set != null && set.Any<T>((Func<T, bool>) (element =>
    {
      IEnumerable<T> source = container;
      return source != null && source.Contains<T>(element);
    }));
  }
}
