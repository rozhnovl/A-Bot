// Decompiled with JetBrains decompiler
// Type: Bib3.Extension
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

#nullable enable
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bib3
{
  public static class Extension
  {
    private static readonly Regex AssemblyShortNameRegex = new Regex("^([^,]+)\\s*,", RegexOptions.Compiled);

    public static IEnumerable<T> WhereNotDefault<T>(this IEnumerable<T> enumerable) => enumerable == null ? (IEnumerable<T>) null : enumerable.Where<T>((Func<T, bool>) (element => !object.Equals((object) default (T), (object) element)));

    public static IEnumerable<TKey> Keys<TKey, TValue>(
      this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
    {
      return enumerable != null ? enumerable.Select<KeyValuePair<TKey, TValue>, TKey>((Func<KeyValuePair<TKey, TValue>, TKey>) (element => element.Key)) : (IEnumerable<TKey>) null;
    }

    public static IEnumerable<TValue> Values<TKey, TValue>(
      this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
    {
      return enumerable != null ? enumerable.Select<KeyValuePair<TKey, TValue>, TValue>((Func<KeyValuePair<TKey, TValue>, TValue>) (element => element.Value)) : (IEnumerable<TValue>) null;
    }

    public static double? TryParseDouble(this string text, NumberStyles numberStyle = NumberStyles.Float) => text.TryParseDouble(Glob.NumberFormat, numberStyle);

    public static double? TryParseDouble(
      this string text,
      NumberFormatInfo numberFormat,
      NumberStyles numberStyle = NumberStyles.Float)
    {
      double result;
      return !double.TryParse(text, numberStyle, (IFormatProvider) numberFormat, out result) ? new double?() : new double?(result);
    }

    public static int? TryParseInt(this string zuParsende) => zuParsende.TryParseInt(NumberStyles.Integer);

    public static int? TryParseInt(this string zuParsende, NumberStyles numberStyle)
    {
      long? int64 = zuParsende.TryParseInt64(NumberFormatInfo.CurrentInfo, numberStyle);
      return !int64.HasValue ? new int?() : new int?((int) int64.GetValueOrDefault());
    }

    public static int? TryParseInt(
      this string zuParsende,
      NumberFormatInfo numberFormat,
      NumberStyles numberStyle = NumberStyles.Integer)
    {
      long? int64 = zuParsende.TryParseInt64(numberFormat, numberStyle);
      return int64.HasValue ? new int?((int) int64.GetValueOrDefault()) : new int?();
    }

    public static long? TryParseInt64(this string zuParsende, NumberStyles numberStyle = NumberStyles.Integer) => zuParsende.TryParseInt64(NumberFormatInfo.CurrentInfo, numberStyle);

    public static long? TryParseInt64(
      this string zuParsende,
      NumberFormatInfo numberFormat,
      NumberStyles numberStyle = NumberStyles.Integer)
    {
      long result;
      return !long.TryParse(zuParsende, numberStyle, (IFormatProvider) numberFormat, out result) ? new long?() : new long?(result);
    }

    public static IEnumerable<T> Reversed<T>(this IEnumerable<T> enumerable) => enumerable == null ? (IEnumerable<T>) null : enumerable.Reverse<T>();

    public static string EnsureEndsWith(this string @string, string end) => @string != null && @string.EndsWith(end) ? @string : @string + end;

    public static string PathToFilesysChild(this string directoryPath, string child) => directoryPath == null ? (string) null : directoryPath.EnsureEndsWith(Path.DirectorySeparatorChar.ToString()) + child;

    public static bool? AllHaveValue<T>(this IEnumerable<T?> enumerable) where T : struct => enumerable == null ? new bool?() : new bool?(enumerable.All<T?>((Func<T?, bool>) (element => element.HasValue)));

    public static T[] ToArrayIfNotEmpty<T>(this IEnumerable<T> enumerable)
    {
      T[] array = enumerable != null ? enumerable.ToArray<T>() : (T[]) null;
      int? length = array?.Length;
      return (0 < length.GetValueOrDefault() ? (length.HasValue ? 1 : 0) : 0) == 0 ? (T[]) null : array;
    }

    public static void ForEach<T>(this IEnumerable<T> menge, Action<T, int> aktioon)
    {
      if (menge == null || aktioon == null)
        return;
      int num = 0;
      foreach (T obj in menge)
      {
        aktioon(obj, num);
        ++num;
      }
    }

    public static void ForEach<T>(this IEnumerable<T> menge, Action<T> aktioon)
    {
      if (menge == null || aktioon == null)
        return;
      foreach (T obj in menge)
        aktioon(obj);
    }

    public static IEnumerable<TAus> SelectWhereNullable<TAin, TAus>(
      this IEnumerable<TAin> seq,
      Extension.SelectWhereFunc<TAin, TAus> map)
    {
      return seq.SelectWhereNullable<TAin, TAus>((Extension.SelectWhereFuncMitIndex<TAin, TAus>) ((TAin ain, int index, out bool inkludiire) => map(ain, out inkludiire)));
    }

    public static IEnumerable<TAus> SelectWhereNullable<TAin, TAus>(
      this IEnumerable<TAin> seq,
      Extension.SelectWhereFuncMitIndex<TAin, TAus> map)
    {
      if (seq != null)
      {
        int index = 0;
        foreach (TAin ain in seq)
        {
          TAin Element = ain;
          bool Inkludiire;
          TAus elementAbbild = map(Element, index, out Inkludiire);
          ++index;
          if (Inkludiire)
          {
            yield return elementAbbild;
            elementAbbild = default (TAus);
            Element = default (TAin);
          }
        }
      }
    }

    public static IEnumerable<TAus> SelectWhereNullable<TAin, TAus>(
      this IEnumerable<TAin> seq,
      Func<TAin, int, KeyValuePair<TAus, bool>> map)
    {
      return seq == null ? (IEnumerable<TAus>) null : seq.Select<TAin, KeyValuePair<TAus, bool>>((Func<TAin, int, KeyValuePair<TAus, bool>>) ((element, index) => map(element, index))).Where<KeyValuePair<TAus, bool>>((Func<KeyValuePair<TAus, bool>, bool>) (element => element.Value)).Select<KeyValuePair<TAus, bool>, TAus>((Func<KeyValuePair<TAus, bool>, TAus>) (element => element.Key));
    }

    public static IEnumerable<T> ConcatNullable<T>(this IEnumerable<IEnumerable<T>> seq)
    {
      if (seq != null)
      {
        foreach (IEnumerable<T> objs in seq)
        {
          IEnumerable<T> item = objs;
          if (item != null)
          {
            foreach (T obj in item)
            {
              T item1 = obj;
              yield return item1;
              item1 = default (T);
            }
            item = (IEnumerable<T>) null;
          }
        }
      }
    }

    public static IEnumerable<T> ConcatNullable<T>(this IEnumerable<T> seq0, IEnumerable<T> seq1)
    {
      if (seq0 == null)
        return seq1;
      return seq1 == null ? seq0 : seq0.Concat<T>(seq1);
    }

    public static void ListeKürzeBegin<T>(this Queue<T> queue, int? itemCountMax)
    {
      if (queue == null)
        return;
      int count = queue.Count;
      int? nullable1 = itemCountMax;
      int? nullable2 = nullable1.HasValue ? new int?(count - nullable1.GetValueOrDefault()) : new int?();
      if (!nullable2.HasValue)
        return;
      for (int index = 0; index < nullable2.Value; ++index)
        queue.Dequeue();
    }

    public static void ListeKürzeBegin<T>(this ConcurrentQueue<T> queue, int? itemCountMax)
    {
      if (queue == null)
        return;
      while (true)
      {
        int? nullable = itemCountMax;
        int? count = queue?.Count;
        if (nullable.GetValueOrDefault() < count.GetValueOrDefault() && nullable.HasValue & count.HasValue)
          queue.TryDequeueOrDefault<T>();
        else
          break;
      }
    }

    public static void ListeKürzeBegin<T>(this IList<T> list, int? itemCountMax)
    {
      if (list == null)
        return;
      List<T> objList = list as List<T>;
      int count = list.Count;
      int? nullable1 = itemCountMax;
      int? nullable2 = nullable1.HasValue ? new int?(count - nullable1.GetValueOrDefault()) : new int?();
      if (!nullable2.HasValue)
        return;
      int? nullable3 = nullable2;
      if (0 < nullable3.GetValueOrDefault() && nullable3.HasValue)
      {
        if (objList != null)
        {
          objList.RemoveRange(0, nullable2.Value);
        }
        else
        {
          for (int index = 0; index < nullable2.Value; ++index)
            list.RemoveAt(0);
        }
      }
    }

    public static void ListeKürzeBegin<T>(this IList<T> liste, Func<T, bool> predicate)
    {
      if (liste == null)
        return;
      List<T> objList = liste as List<T>;
      while (true)
      {
        if (liste.Count >= 1)
        {
          T obj = liste[0];
          if (predicate(obj))
            liste.RemoveAt(0);
          else
            goto label_1;
        }
        else
          break;
      }
      return;
label_1:;
    }

    public static void ListeKürzeBegin<T>(this Queue<T> queue, Func<T, bool> predicate)
    {
      if (queue == null)
        return;
      List<T> objList = queue as List<T>;
      while (true)
      {
        if (queue.Count >= 1)
        {
          T obj = queue.Peek();
          if (predicate(obj))
            queue.Dequeue();
          else
            goto label_1;
        }
        else
          break;
      }
      return;
label_1:;
    }

    public static bool IsNullOrEmpty(this IEnumerable seq) => seq == null || !seq.GetEnumerator().MoveNext();

    [Obsolete("IsNullOrEmpty")]
    public static bool NullOderLeer(this IEnumerable enumerable) => enumerable.IsNullOrEmpty();

    public static IEnumerable<T> EnumerateNodeFromTreeBFirst<T>(
      this T root,
      Func<T, IEnumerable<T>> callbackEnumerateChildInNode,
      int? depthMax = null,
      int? depthMin = null)
    {
      IEnumerable<T[]> nodeFromTreeBfirst = root.EnumeratePathToNodeFromTreeBFirst<T>(callbackEnumerateChildInNode);
      IEnumerable<T> objs;
      if (nodeFromTreeBfirst == null)
      {
        objs = (IEnumerable<T>) null;
      }
      else
      {
        IEnumerable<T[]> source1 = nodeFromTreeBfirst.SkipWhile<T[]>((Func<T[], bool>) (toNodePath =>
        {
          if (!depthMin.HasValue)
            return false;
          int? nullable1 = depthMin;
          int? nullable2 = toNodePath != null ? new int?(((IEnumerable<T>) toNodePath).Count<T>()) : new int?();
          return nullable1.GetValueOrDefault() <= nullable2.GetValueOrDefault() && nullable1.HasValue & nullable2.HasValue;
        }));
        if (source1 == null)
        {
          objs = (IEnumerable<T>) null;
        }
        else
        {
          IEnumerable<T[]> source2 = source1.TakeWhile<T[]>((Func<T[], bool>) (toNodePath =>
          {
            if (!depthMax.HasValue)
              return true;
            if (toNodePath == null)
              return false;
            int num = ((IEnumerable<T>) toNodePath).Count<T>();
            int? nullable = depthMax;
            int valueOrDefault = nullable.GetValueOrDefault();
            return num <= valueOrDefault && nullable.HasValue;
          }));
          objs = source2 != null ? source2.Select<T[], T>((Func<T[], T>) (toNodePath => ((IEnumerable<T>) toNodePath).LastOrDefault<T>())) : (IEnumerable<T>) null;
        }
      }
      return objs;
    }

    [Obsolete("use EnumerateNodeFromTreeBFirst instead.", true)]
    public static IEnumerable<T> EnumerateNodeFromTree<T>(
      this T root,
      Func<T, IEnumerable<T>> callbackEnumerateChildInNode,
      int? depthMax = null,
      int? depthMin = null)
    {
      return root.EnumerateNodeFromTreeBFirst<T>(callbackEnumerateChildInNode, depthMax, depthMin);
    }

    public static IEnumerable<T[]> EnumeratePathToNodeFromTreeBFirst<T>(
      this T root,
      Func<T, IEnumerable<T>> callbackEnumerateChildInNode)
    {
      Queue<T[]> SclangePfaad = new Queue<T[]>();
      SclangePfaad.Enqueue(new T[1]{ root });
      while (0 < SclangePfaad.Count)
      {
        T[] AstPfaad = SclangePfaad.Dequeue();
        yield return AstPfaad;
        if (callbackEnumerateChildInNode != null)
        {
          T Ast = ((IEnumerable<T>) AstPfaad).LastOrDefault<T>();
          IEnumerable<T> AstListeKind = callbackEnumerateChildInNode(Ast);
          if (AstListeKind != null)
          {
            foreach (T obj in AstListeKind)
            {
              T AstKind = obj;
              SclangePfaad.Enqueue(((IEnumerable<T>) AstPfaad).Concat<T>((IEnumerable<T>) new T[1]
              {
                AstKind
              }).ToArray<T>());
              AstKind = default (T);
            }
            AstPfaad = (T[]) null;
            Ast = default (T);
            AstListeKind = (IEnumerable<T>) null;
          }
        }
      }
    }

    public static IEnumerable<T[]> EnumeratePathToNodeFromTree<T>(
      this T root,
      Func<T, IEnumerable<T>> callbackEnumerateChildInNode)
    {
      return root.EnumeratePathToNodeFromTreeBFirst<T>(callbackEnumerateChildInNode);
    }

    public static IEnumerable<T[]> EnumeratePathToNodeFromTreeDFirst<T>(
      this T root,
      Func<T, IEnumerable<T>> callbackEnumerateChildInNode)
    {
      yield return new T[1]{ root };
      foreach (T obj in callbackEnumerateChildInNode(root).EmptyIfNull<T>())
      {
        T child = obj;
        foreach (T[] childPath in child.EnumeratePathToNodeFromTreeDFirst<T>(callbackEnumerateChildInNode).EmptyIfNull<T[]>())
          yield return ((IEnumerable<T>) new T[1]{ root }).Concat<T>((IEnumerable<T>) childPath).ToArray<T>();
        child = default (T);
      }
    }

    public static IEnumerable<T> EnumerateNodeFromTreeDFirst<T>(
      this T root,
      Func<T, IEnumerable<T>> callbackEnumerateChildInNode,
      int? depthMax = null,
      int? depthMin = null)
    {
      IEnumerable<T[]> nodeFromTreeDfirst = root.EnumeratePathToNodeFromTreeDFirst<T>(callbackEnumerateChildInNode);
      IEnumerable<T> objs;
      if (nodeFromTreeDfirst == null)
      {
        objs = (IEnumerable<T>) null;
      }
      else
      {
        IEnumerable<T[]> source1 = nodeFromTreeDfirst.SkipWhile<T[]>((Func<T[], bool>) (toNodePath =>
        {
          if (!depthMin.HasValue)
            return false;
          int? nullable1 = depthMin;
          int? nullable2 = toNodePath != null ? new int?(((IEnumerable<T>) toNodePath).Count<T>()) : new int?();
          return nullable1.GetValueOrDefault() <= nullable2.GetValueOrDefault() && nullable1.HasValue & nullable2.HasValue;
        }));
        if (source1 == null)
        {
          objs = (IEnumerable<T>) null;
        }
        else
        {
          IEnumerable<T[]> source2 = source1.TakeWhile<T[]>((Func<T[], bool>) (toNodePath =>
          {
            if (!depthMax.HasValue)
              return true;
            if (toNodePath == null)
              return false;
            int num = ((IEnumerable<T>) toNodePath).Count<T>();
            int? nullable = depthMax;
            int valueOrDefault = nullable.GetValueOrDefault();
            return num <= valueOrDefault && nullable.HasValue;
          }));
          objs = source2 != null ? source2.Select<T[], T>((Func<T[], T>) (toNodePath => ((IEnumerable<T>) toNodePath).LastOrDefault<T>())) : (IEnumerable<T>) null;
        }
      }
      return objs;
    }

    public static T? CastToNullable<T>(this object @ref) where T : struct => @ref is T obj ? new T?(obj) : new T?();

    public static IEnumerable<KeyValuePair<KeyT, ValueT?>> ElementValueCastToNullable<KeyT, ValueT>(
      this IEnumerable<KeyValuePair<KeyT, ValueT>> orig)
      where ValueT : struct
    {
      return orig == null ? (IEnumerable<KeyValuePair<KeyT, ValueT?>>) null : orig.Select<KeyValuePair<KeyT, ValueT>, KeyValuePair<KeyT, ValueT?>>((Func<KeyValuePair<KeyT, ValueT>, KeyValuePair<KeyT, ValueT?>>) (origElement => new KeyValuePair<KeyT, ValueT?>(origElement.Key, new ValueT?(origElement.Value))));
    }

    public static Decimal ZaitMikroSictDecimal(this Stopwatch stopwatch) => (Decimal) stopwatch.ElapsedTicks * 1000000M / (Decimal) Stopwatch.Frequency;

    public static long ZaitMikroSictInt(this Stopwatch stopwatch) => (long) stopwatch.ZaitMikroSictDecimal();

    public static void AddRangeNullable<T>(this List<T> list, IEnumerable<T> seq)
    {
      if (list == null || seq == null)
        return;
      list.AddRange(seq);
    }

    public static IEnumerable<byte[]> LeeseMengeBlok(this Stream stream, int blokListeOktetAnzaal)
    {
      if (stream != null)
      {
        byte[] Pufer = new byte[blokListeOktetAnzaal];
        while (true)
        {
          int AnzaalGeleese = stream.Read(Pufer, 0, Pufer.Length);
          if (AnzaalGeleese >= 1)
          {
            byte[] Blok = new byte[AnzaalGeleese];
            Buffer.BlockCopy((Array) Pufer, 0, (Array) Blok, 0, AnzaalGeleese);
            yield return Blok;
            Blok = (byte[]) null;
          }
          else
            break;
        }
      }
    }

    public static T[] ListeArrayAgregiire<T>(this T[][] arrayArray)
    {
      if (arrayArray == null)
        return (T[]) null;
      T[][] array1 = ((IEnumerable<T[]>) arrayArray).WhereNotDefault<T[]>().ToArray<T[]>();
      T[] objArray = new T[((IEnumerable<T[]>) array1).Select<T[], int>((Func<T[], int>) (array => array.Length)).Sum()];
      if (objArray.Length < 1)
        return objArray;
      Type elementType = array1 != null ? ((IEnumerable<T[]>) array1).FirstOrDefault<T[]>().GetType().GetElementType() : (Type) null;
      int num = Marshal.SizeOf(elementType);
      int destinationIndex = 0;
      for (int index = 0; index < array1.Length; ++index)
      {
        T[] array2 = arrayArray[index];
        if (array2.Length != 0)
        {
          if (elementType.IsPrimitive)
            Buffer.BlockCopy((Array) array2, 0, (Array) objArray, num * destinationIndex, num * array2.Length);
          else
            Array.ConstrainedCopy((Array) array2, 0, (Array) objArray, destinationIndex, array2.Length);
          destinationIndex += array2.Length;
        }
      }
      return objArray;
    }

    public static byte[] LeeseGesamt(this Stream stream, int blokListeOktetAnzaal = 65536)
    {
      if (stream == null)
        return (byte[]) null;
      byte[][] array = stream.LeeseMengeBlok(blokListeOktetAnzaal).ToArray<byte[]>();
      return array.Length <= 1 ? ((IEnumerable<byte[]>) array).FirstOrDefault<byte[]>() : array.ListeArrayAgregiire<byte>();
    }

    public static Array ArraySegment(
      this Array array,
      int segmentBeginElementIndex,
      int segmentListeElementAnzaalScrankeMax)
    {
      if (array == null)
        return (Array) null;
      segmentBeginElementIndex = Math.Max(0, segmentBeginElementIndex);
      int length = Math.Max(0, Math.Min(segmentListeElementAnzaalScrankeMax, array.Length - segmentBeginElementIndex));
      Array instance = Array.CreateInstance(array.GetType().GetElementType(), length);
      Array.ConstrainedCopy(array, segmentBeginElementIndex, instance, 0, length);
      return instance;
    }

    public static byte[] DeflateKompres(this byte[] listeOktet, CompressionLevel compressionLevel = 0)
    {
      if (listeOktet == null)
        return (byte[]) null;
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (DeflateStream deflateStream = new DeflateStream((Stream) memoryStream, compressionLevel, true))
          deflateStream.Write(listeOktet, 0, listeOktet.Length);
        memoryStream.Seek(0L, SeekOrigin.Begin);
        return memoryStream.ToArray();
      }
    }

    public static byte[] DeflateDeKompres(this byte[] listeOktet, long listeOktetAnzaalScrankeMax)
    {
      if (listeOktet == null)
        return (byte[]) null;
      using (MemoryStream memoryStream = new MemoryStream(listeOktet))
      {
        using (DeflateStream stream = new DeflateStream((Stream) memoryStream, CompressionMode.Decompress))
        {
          int blokListeOktetAnzaal = 65536;
          long count = (listeOktetAnzaalScrankeMax - 1L) / (long) blokListeOktetAnzaal + 1L;
          return ((IEnumerable<IEnumerable<byte>>) stream.LeeseMengeBlok(blokListeOktetAnzaal).Take<byte[]>((int) count).ToArray<byte[]>()).ConcatNullable<byte>().ToArray<byte>();
        }
      }
    }

    public static void EnqueueSeq<T>(this Queue<T> queue, IEnumerable<T> sequence)
    {
      if (sequence == null || queue == null)
        return;
      foreach (T obj in sequence)
        queue.Enqueue(obj);
    }

    public static void Enqueue<T>(this Queue<T> queue, IEnumerable<T> sequence) => queue.EnqueueSeq<T>(sequence);

    public static IEnumerable<T> DequeueEnumerator<T>(
      this Queue<T> queue,
      Func<T, bool> peekPredicate = null)
    {
      if (queue != null)
      {
        while (0 < queue.Count && (peekPredicate == null || peekPredicate(queue.Peek())))
          yield return queue.Dequeue();
      }
    }

    public static IEnumerable<T> DequeueEnumerator<T>(this Queue<T> queue, Func<bool> predicate)
    {
      Func<T, bool> peekPredicate = predicate == null ? (Func<T, bool>) null : (Func<T, bool>) (t => predicate());
      return queue.DequeueEnumerator<T>(peekPredicate);
    }

    public static IEnumerable<T> DequeueEnum<T>(this Queue<T> queue) => queue.DequeueEnumerator<T>();

    public static void TypeClrAssemblyQualifiedNameExtraktAssemblyNameUndTypeFullName(
      string typeClrAssemblyQualifiedName,
      out string assemblyName,
      out string typeFullName)
    {
      ParsedAssemblyQualifiedName.ParsedAssemblyQualifiedName assemblyQualifiedName = new ParsedAssemblyQualifiedName.ParsedAssemblyQualifiedName(typeClrAssemblyQualifiedName);
      assemblyName = assemblyQualifiedName.ShortAssemblyName;
      typeFullName = assemblyQualifiedName.TypeName;
    }

    public static Type TypeAusTypeClrAssemblyQualifiedNameBerecne(
      string typeClrAssemblyQualifiedName)
    {
      if (typeClrAssemblyQualifiedName == null)
        return (Type) null;
      string typeFullName;
      string AssemblyName;
      Extension.TypeClrAssemblyQualifiedNameExtraktAssemblyNameUndTypeFullName(typeClrAssemblyQualifiedName, out AssemblyName, out typeFullName);
      Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
      Assembly assembly = assemblies != null ? ((IEnumerable<Assembly>) assemblies).FirstOrDefault<Assembly>((Func<Assembly, bool>) (c => string.Equals(c.GetName().Name, AssemblyName))) : (Assembly) null;
      if ((Assembly) null == assembly)
        return Type.GetType(typeClrAssemblyQualifiedName);
      Type type = assembly.GetType(typeFullName);
      if (!((Type) null == type))
        ;
      return type;
    }

    public static CallRateScranke CallRateScrankeStopwatchMili(
      this Action action,
      int distanzScrankeMinMili)
    {
      return new CallRateScranke(action, new Func<long>(Glob.StopwatchZaitMiliSictInt), distanzScrankeMinMili);
    }

    public static IEnumerable<T> Yield<T>(this T w)
    {
      yield return w;
    }

    public static WertZuZaitpunktStruct<T>? AlsBeginZaitpunktStruct<T>(
      this PropertyGenIntervalInt64<T> w)
    {
      return w == null ? new WertZuZaitpunktStruct<T>?() : new WertZuZaitpunktStruct<T>?(new WertZuZaitpunktStruct<T>(w.Value, w.Low));
    }

    public static WertZuZaitpunktStruct<T>? AlsEndeZaitpunktStruct<T>(
      this PropertyGenIntervalInt64<T> w)
    {
      return w == null ? new WertZuZaitpunktStruct<T>?() : new WertZuZaitpunktStruct<T>?(new WertZuZaitpunktStruct<T>(w.Value, w.Up));
    }

    public static IEnumerable<T> WhereNotNullSelectValue<T>(this IEnumerable<T?> sequenz) where T : struct
    {
      IEnumerable<T> objs;
      if (sequenz == null)
      {
        objs = (IEnumerable<T>) null;
      }
      else
      {
        IEnumerable<T?> source = sequenz.Where<T?>((Func<T?, bool>) (t => t.HasValue));
        objs = source != null ? source.Select<T?, T>((Func<T?, T>) (t => t.Value)) : (IEnumerable<T>) null;
      }
      return objs;
    }

    public static string AssemblyShortNameVonAssemblyName(this string assemblyName)
    {
      if (assemblyName == null)
        return (string) null;
      System.Text.RegularExpressions.Match match = Extension.AssemblyShortNameRegex.Match(assemblyName);
      return !match.Success ? assemblyName : match.Groups[1].Value;
    }

    public static T NullIfEmpty<T>(this T seq) where T : IEnumerable => seq.IsNullOrEmpty() ? default (T) : seq;

    public static void RGBKonvertiirtNaacHueSatVal(
      int r,
      int g,
      int b,
      int rgbWerteberaicMax,
      int hueSatValWerteberaicMax,
      out int hue,
      out int sat,
      out int val)
    {
      hue = 0;
      int num1 = Math.Min(r, Math.Min(g, b));
      int num2 = Math.Max(r, Math.Max(g, b));
      int num3 = num2 - num1;
      val = num2 * hueSatValWerteberaicMax / rgbWerteberaicMax;
      if (num3 == 0)
      {
        hue = 0;
        sat = 0;
      }
      else
      {
        sat = num3 * hueSatValWerteberaicMax * hueSatValWerteberaicMax / num2 / rgbWerteberaicMax;
        int num4 = ((num2 - r) / 6 + num3 / 2) * hueSatValWerteberaicMax * hueSatValWerteberaicMax / num3 / rgbWerteberaicMax;
        int num5 = ((num2 - g) / 6 + num3 / 2) * hueSatValWerteberaicMax * hueSatValWerteberaicMax / num3 / rgbWerteberaicMax;
        int num6 = ((num2 - b) / 6 + num3 / 2) * hueSatValWerteberaicMax * hueSatValWerteberaicMax / num3 / rgbWerteberaicMax;
        if (r == num2)
          hue = num6 - num5;
        else if (g == num2)
          hue = hueSatValWerteberaicMax / 3 + num4 - num6;
        else if (b == num2)
          hue = hueSatValWerteberaicMax * 2 / 3 + num5 - num4;
        if (hue < 0)
          ++hue;
        if (hue > 1)
          --hue;
      }
    }

    public static T? EnumParseNullable<T>(this string enumSictString, bool ignoreCase = false) where T : struct, IConvertible, IComparable, IFormattable
    {
      if (enumSictString == null)
        return new T?();
      if (!typeof (T).IsEnum)
        throw new ArgumentException("!typeof(T).IsEnum");
      T result;
      return Enum.TryParse<T>(enumSictString, ignoreCase, out result) ? new T?(result) : new T?();
    }

    public static TValue TryGetValueOrDefault<TKey, TValue>(
      this IDictionary<TKey, TValue> dict,
      TKey key)
    {
      TValue obj;
      return dict != null && dict.TryGetValue(key, out obj) ? obj : default (TValue);
    }

    public static TValue? TryGetValueNullable<TKey, TValue>(
      this IDictionary<TKey, TValue> dict,
      TKey key)
      where TValue : struct
    {
      TValue obj;
      return dict != null && dict.TryGetValue(key, out obj) ? new TValue?(obj) : new TValue?();
    }

    public static T? FirstOrNull<T>(this IEnumerable<T> seq) where T : struct
    {
      if (seq == null)
        return new T?();
      using (IEnumerator<T> enumerator = seq.GetEnumerator())
      {
        if (enumerator.MoveNext())
          return new T?(enumerator.Current);
      }
      return new T?();
    }

    public static T? FirstOrNull<T>(this IEnumerable<T> seq, Func<T, bool> predicate) where T : struct
    {
      if (seq == null)
        return new T?();
      foreach (T obj in seq)
      {
        if (predicate(obj))
          return new T?(obj);
      }
      return new T?();
    }

    public static bool SequenceEqual(
      this IEnumerable collection0,
      IEnumerable collection1,
      Func<object, object, bool> callbackElementEqual)
    {
      if (object.Equals((object) collection0, (object) collection1))
        return true;
      if (collection0 == null || collection1 == null)
        return false;
      IEnumerator enumerator1 = collection0.GetEnumerator();
      IEnumerator enumerator2 = collection1.GetEnumerator();
      bool flag1;
      bool flag2;
      do
      {
        flag1 = enumerator1.MoveNext();
        flag2 = enumerator2.MoveNext();
        if (!(flag1 & flag2))
          goto label_5;
      }
      while ((callbackElementEqual != null ? (callbackElementEqual(enumerator1.Current, enumerator2.Current) ? 1 : 0) : 0) != 0);
      goto label_9;
label_5:
      return !(flag1 | flag2);
label_9:
      return false;
    }

    public static bool SequenceEqualPerReferenceEqual(this IEnumerable seq0, IEnumerable seq1) => seq0.SequenceEqual(seq1, new Func<object, object, bool>(object.ReferenceEquals));

    public static bool SequenceEqualPerObjectEquals(this IEnumerable seq0, IEnumerable seq1) => seq0.SequenceEqual(seq1, new Func<object, object, bool>(object.Equals));

    public static object ReturnValueOrException<InT, ReturnT>(this Func<InT, ReturnT> funk, InT arg)
    {
      try
      {
        return (object) funk(arg);
      }
      catch (Exception ex)
      {
        return (object) ex;
      }
    }

    public static object ReturnValueOrException<ReturnT>(this Func<ReturnT> funk)
    {
      try
      {
        return (object) funk();
      }
      catch (Exception ex)
      {
        return (object) ex;
      }
    }

    public static ReturnT ReturnDefaultIfThrows<ReturnT>(this Func<ReturnT> @delegate)
    {
      try
      {
        return @delegate();
      }
      catch
      {
        return default (ReturnT);
      }
    }

    public static IEnumerable<T> ExceptionCatch<T>(
      this IEnumerable<T> seq,
      Action<Exception> callbackException = null)
    {
      using (IEnumerator<T> enumerator = seq.GetEnumerator())
      {
        bool next = true;
        while (next)
        {
          try
          {
            next = enumerator.MoveNext();
          }
          catch (Exception ex)
          {
            Action<Exception> action = callbackException;
            if (action != null)
            {
              action(ex);
              continue;
            }
            continue;
          }
          if (next)
            yield return enumerator.Current;
        }
      }
    }

    public static IEnumerable<T> CatchException<T>(
      this IEnumerable<T> seq,
      Action<Exception> callbackException = null)
    {
      return seq.ExceptionCatch<T>(callbackException);
    }

    public static IEnumerable<T> ExceptionMap<T>(
      this IEnumerable<T> seq,
      Func<Exception, T> callbackExceptionMap = null)
    {
      using (IEnumerator<T> enumerator = seq.GetEnumerator())
      {
        bool next = true;
        while (next)
        {
          T ExceptionMapped = default (T);
          bool Catched = false;
          try
          {
            next = enumerator.MoveNext();
          }
          catch (Exception ex)
          {
            ExceptionMapped = callbackExceptionMap(ex);
            Catched = true;
          }
          if (Catched)
            yield return ExceptionMapped;
          else if (next)
            yield return enumerator.Current;
          ExceptionMapped = default (T);
        }
      }
    }

    public static IEnumerable<T> EnumGetValues<T>() => Enum.GetValues(typeof (T)).OfType<T>();

    public static IEnumerable<T?> CastToNullable<T>(this IEnumerable<T> source) where T : struct => source == null ? (IEnumerable<T?>) null : source.Select<T, T?>((Func<T, T?>) (element => new T?(element)));

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source) => source == null ? (IEnumerable<T>) new T[0] : source;

    public static WertZuZaitpunktStruct<T>? Max<T>(this IEnumerable<WertZuZaitpunktStruct<T>> source) => new WertZuZaitpunktStruct<T>?(source.OrderByDescending<WertZuZaitpunktStruct<T>, long>((Func<WertZuZaitpunktStruct<T>, long>) (t => t.Zait)).FirstOrDefault<WertZuZaitpunktStruct<T>>());

    public static WertZuZaitpunktStruct<T> Max<T>(
      WertZuZaitpunktStruct<T> o0,
      WertZuZaitpunktStruct<T> o1)
    {
      return ((IEnumerable<WertZuZaitpunktStruct<T>>) new WertZuZaitpunktStruct<T>[2]
      {
        o0,
        o1
      }).Max<T>() ?? new WertZuZaitpunktStruct<T>();
    }

    public static void WriteToFileAndCreateDirectoryIfNotExisting(this string filePath, byte[] file)
    {
      if (filePath == null)
        return;
      FileInfo fileInfo = new FileInfo(filePath);
      if (!fileInfo.Directory.Exists)
        fileInfo.Directory.Create();
      FileStream fileStream = fileInfo.Create();
      try
      {
        fileStream.Write(file, 0, file.Length);
      }
      finally
      {
        fileStream.Close();
      }
    }

    public static void Push<T>(this Stack<T> dest, IEnumerable<T> source)
    {
      if (dest == null || source == null)
        return;
      foreach (T obj in source)
        dest.Push(obj);
    }

    public static int PropagiireList<T>(
      this IEnumerable<T> source,
      IList<T> destination,
      Func<T, T, bool> callbackEquals)
    {
      return source.PropagiireList(destination as IList, (Func<object, object, bool>) ((a, b) => callbackEquals((T) a, (T) b)));
    }

    public static int PropagiireListObjectEquals<T>(
      this IEnumerable<T> source,
      IList<T> destination)
    {
      return source.PropagiireList<T>(destination, (Func<T, T, bool>) ((a, b) => object.Equals((object) a, (object) b)));
    }

    public static int PropagiireList(
      this IEnumerable source,
      IList dest,
      Func<object, object, bool> callbackEquals)
    {
      if (dest == null)
        return 0;
      if (source == null)
      {
        try
        {
          return dest.Count;
        }
        finally
        {
          dest.Clear();
        }
      }
      else
      {
        int num = 0;
        int index = 0;
        foreach (object obj in source)
        {
          try
          {
            if (dest.Count <= index)
            {
              ++num;
              dest.Add(obj);
            }
            else if (!callbackEquals(obj, dest[index]))
            {
              ++num;
              dest.RemoveAt(index);
              dest.Insert(index, obj);
            }
          }
          finally
          {
            ++index;
          }
        }
        while (index < dest.Count)
        {
          ++num;
          dest.RemoveAt(index);
        }
        return num;
      }
    }

    public static int PropagiireListObjectEquals(this IEnumerable source, IList dest) => source.PropagiireList(dest, (Func<object, object, bool>) ((a, b) => object.Equals(a, b)));

    public static DateTime? ZaitKalenderAlsDateTime(this string timeCal, int componentCountMin = 3)
    {
      if (timeCal == null)
        return new DateTime?();
      int[] array = ((IEnumerable<string>) timeCal.Split('.')).Select<string, int?>((Func<string, int?>) (komponenteString => komponenteString.TryParseInt())).TakeWhile<int?>((Func<int?, bool>) (komponenteWert => komponenteWert.HasValue)).WhereNotNullSelectValue<int>().ToArray<int>();
      return array.Length < componentCountMin ? new DateTime?() : new DateTime?(new DateTime(((IEnumerable<int>) array).ElementAtOrDefault<int>(0), ((IEnumerable<int>) array).ElementAtOrDefault<int>(1) + 1, ((IEnumerable<int>) array).ElementAtOrDefault<int>(2) + 1, ((IEnumerable<int>) array).ElementAtOrDefault<int>(3), ((IEnumerable<int>) array).ElementAtOrDefault<int>(4), ((IEnumerable<int>) array).ElementAtOrDefault<int>(5)));
    }

    public static int? IndexOfElementClosest<T>(
      this IEnumerable<T> sequence,
      Func<T, long> callbackDistanceOfElement)
    {
      if (sequence == null)
        return new int?();
      IEnumerable<KeyValuePair<int, T>> source1 = sequence.Select<T, KeyValuePair<int, T>>((Func<T, int, KeyValuePair<int, T>>) ((item, index) => new KeyValuePair<int, T>(index, item)));
      if (source1 == null)
        return new int?();
      IOrderedEnumerable<KeyValuePair<int, T>> source2 = source1.OrderBy<KeyValuePair<int, T>, long>((Func<KeyValuePair<int, T>, long>) (indexAndElement => Math.Abs(callbackDistanceOfElement(indexAndElement.Value))));
      if (source2 == null)
        return new int?();
      IEnumerable<int?> source3 = source2.Select<KeyValuePair<int, T>, int?>((Func<KeyValuePair<int, T>, int?>) (indexAndElement => new int?(indexAndElement.Key)));
      return source3 == null ? new int?() : source3.FirstOrDefault<int?>();
    }

    public static IEnumerable<KeyValuePair<int, T>> SubSequenceAroundElementClosest<T>(
      this IEnumerable<T> sequence,
      Func<T, long> callbackDistanceOfElement,
      int previousElementCountMax = 0,
      int followingElementCountMax = 0)
    {
      int? ClosestElementIndex = sequence.IndexOfElementClosest<T>(callbackDistanceOfElement);
      if (!ClosestElementIndex.HasValue)
        return (IEnumerable<KeyValuePair<int, T>>) null;
      int IndexLow = Math.Max(0, ClosestElementIndex.Value - previousElementCountMax);
      return sequence.Skip<T>(IndexLow).Select<T, KeyValuePair<int, T>>((Func<T, int, KeyValuePair<int, T>>) ((element, index) => new KeyValuePair<int, T>(IndexLow + index - ClosestElementIndex.Value, element))).Take<KeyValuePair<int, T>>(ClosestElementIndex.Value + followingElementCountMax - IndexLow + 1);
    }

    public static IEnumerable<KeyValuePair<string, byte[]>> MengeDataiAusVerzaicnisPfaadUndInhalt(
      this string directoryPath)
    {
      DirectoryInfo Verzaicnis = new DirectoryInfo(directoryPath);
      FileInfo[] fileInfoArray = Verzaicnis.GetFiles();
      for (int index = 0; index < fileInfoArray.Length; ++index)
      {
        FileInfo Datai = fileInfoArray[index];
        byte[] DataiInhalt = Glob.InhaltAusDataiMitPfaad(Datai.FullName);
        yield return new KeyValuePair<string, byte[]>(Datai.FullName, DataiInhalt);
        DataiInhalt = (byte[]) null;
        Datai = (FileInfo) null;
      }
      fileInfoArray = (FileInfo[]) null;
    }

    public static IEnumerable<string> MengeDataiAusVerzaicnisInhaltAlsUTF8(this string directoryPath)
    {
      IEnumerable<KeyValuePair<string, byte[]>> source = directoryPath.MengeDataiAusVerzaicnisPfaadUndInhalt();
      return source == null ? (IEnumerable<string>) null : source.Select<KeyValuePair<string, byte[]>, string>((Func<KeyValuePair<string, byte[]>, string>) (dataiPfaadUndInhalt => Encoding.UTF8.GetString(dataiPfaadUndInhalt.Value)));
    }

    public static Regex AlsRegex(this string regexPattern, RegexOptions regexOptions) => regexPattern != null ? new Regex(regexPattern, regexOptions) : (Regex) null;

    public static Regex AlsRegexIgnoreCaseCompiled(
      this string regexPattern,
      RegexOptions regexOptions = RegexOptions.None)
    {
      return regexPattern != null ? new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | regexOptions) : (Regex) null;
    }

    public static T TryPeekOrDefault<T>(this ConcurrentQueue<T> queue)
    {
      T result = default (T);
      return queue != null && queue.TryPeek(out result) ? result : default (T);
    }

    public static T TryDequeueOrDefault<T>(this ConcurrentQueue<T> queue)
    {
      T result = default (T);
      return queue != null && queue.TryDequeue(out result) ? result : default (T);
    }

    public static long GetInt64(this RandomNumberGenerator source)
    {
      byte[] data = new byte[8];
      source.GetBytes(data);
      return BitConverter.ToInt64(data, 0);
    }

    public static IEnumerable<KeyValuePair<string[], FileInfo>> EnumFileFromDirectory(
      this string directoryPath,
      int? depthMax = null)
    {
      DirectoryInfo Directory = new DirectoryInfo(directoryPath);
      if (Directory.Exists)
      {
        FileInfo[] fileInfoArray = Directory.GetFiles();
        for (int index = 0; index < fileInfoArray.Length; ++index)
        {
          FileInfo File = fileInfoArray[index];
          yield return new KeyValuePair<string[], FileInfo>(new string[1]
          {
            File.Name
          }, File);
          File = (FileInfo) null;
        }
        fileInfoArray = (FileInfo[]) null;
        DirectoryInfo[] directoryInfoArray = Directory.GetDirectories();
        for (int index = 0; index < directoryInfoArray.Length; ++index)
        {
          DirectoryInfo SubDirectory = directoryInfoArray[index];
          string fullName = SubDirectory.FullName;
          int? nullable = depthMax;
          int? depthMax1 = nullable.HasValue ? new int?(nullable.GetValueOrDefault() - 1) : new int?();
          foreach (KeyValuePair<string[], FileInfo> keyValuePair in fullName.EnumFileFromDirectory(depthMax1))
          {
            KeyValuePair<string[], FileInfo> SubEntry = keyValuePair;
            yield return new KeyValuePair<string[], FileInfo>(SubDirectory.Name.Yield<string>().ConcatNullable<string>((IEnumerable<string>) SubEntry.Key).ToArray<string>(), SubEntry.Value);
            SubEntry = new KeyValuePair<string[], FileInfo>();
          }
          SubDirectory = (DirectoryInfo) null;
        }
        directoryInfoArray = (DirectoryInfo[]) null;
      }
    }

    public static string Truncate(this string orig, int lengthMax)
    {
      int num = lengthMax;
      int? length = orig?.Length;
      int valueOrDefault = length.GetValueOrDefault();
      return (num < valueOrDefault ? (length.HasValue ? 1 : 0) : 0) == 0 ? orig : orig.Substring(0, lengthMax);
    }

    public static IEnumerable<KeyValuePair<OutT, ValueT>> KeyMap<InT, OutT, ValueT>(
      this IEnumerable<KeyValuePair<InT, ValueT>> seq,
      Func<InT, OutT> map)
    {
      return seq == null ? (IEnumerable<KeyValuePair<OutT, ValueT>>) null : seq.Select<KeyValuePair<InT, ValueT>, KeyValuePair<OutT, ValueT>>((Func<KeyValuePair<InT, ValueT>, KeyValuePair<OutT, ValueT>>) (elem => new KeyValuePair<OutT, ValueT>(map(elem.Key), elem.Value)));
    }

    public static IEnumerable<KeyValuePair<KeyT, OutT>> ValueMap<KeyT, InT, OutT>(
      this IEnumerable<KeyValuePair<KeyT, InT>> seq,
      Func<InT, OutT> map)
    {
      return seq == null ? (IEnumerable<KeyValuePair<KeyT, OutT>>) null : seq.Select<KeyValuePair<KeyT, InT>, KeyValuePair<KeyT, OutT>>((Func<KeyValuePair<KeyT, InT>, KeyValuePair<KeyT, OutT>>) (elem => new KeyValuePair<KeyT, OutT>(elem.Key, map(elem.Value))));
    }

    public static T? ToNullable<T>(this T @in) where T : struct => new T?(@in);

    public static bool CountAtLeast<T>(this IEnumerable<T> seq, long bound)
    {
      long num = 0;
      foreach (T obj in seq.EmptyIfNull<T>())
      {
        ++num;
        if (bound <= num)
          return true;
      }
      return false;
    }

    public static IEnumerable<T> TakeWhileNotDefault<T>(this IEnumerable<T> seq) => seq == null ? (IEnumerable<T>) null : seq.TakeWhile<T>((Func<T, bool>) (element => !object.Equals((object) element, (object) default (T))));

    public static IEnumerable<T> YieldInvokeRecurring<T>(this Func<T> generateElement)
    {
      while (true)
        yield return generateElement();
    }

    public static void ThrowContainStackTraceIfNotNull(this Exception exception)
    {
      if (exception != null)
        throw new Exception("StackTraceContainer", exception);
    }

    public static IEnumerable<KeyValuePair<ValueT, KeyT>> SwapKeyValue<KeyT, ValueT>(
      this IEnumerable<KeyValuePair<KeyT, ValueT>> seq)
    {
      return seq == null ? (IEnumerable<KeyValuePair<ValueT, KeyT>>) null : seq.Select<KeyValuePair<KeyT, ValueT>, KeyValuePair<ValueT, KeyT>>((Func<KeyValuePair<KeyT, ValueT>, KeyValuePair<ValueT, KeyT>>) (element => new KeyValuePair<ValueT, KeyT>(element.Value, element.Key)));
    }

    public static int? FirstIndexOrNull<T>(this IEnumerable<T> seq, Func<T, bool> predicate)
    {
      if (seq == null)
        return new int?();
      if (predicate == null)
        return new int?();
      int num = 0;
      foreach (T obj in seq)
      {
        if (predicate(obj))
          return new int?(num);
        ++num;
      }
      return new int?();
    }

    public static int? FirstIndexOrNullPerEquals<T>(this IEnumerable<T> liste, T valueId) => liste.FirstIndexOrNull<T>((Func<T, bool>) (kandidaat => object.Equals((object) kandidaat, (object) (T) valueId)));

    public static int? LastIndexOrNull<T>(this IEnumerable<T> seq, Func<T, bool> predicate)
    {
      if (seq == null)
        return new int?();
      if (predicate == null)
        return new int?();
      int num = 0;
      int? nullable = new int?();
      foreach (T obj in seq)
      {
        if (predicate(obj))
          nullable = new int?(num);
        ++num;
      }
      return nullable;
    }

    public static T DefaultIfNull<T>(this T? orig) where T : struct => !orig.HasValue ? default (T) : orig.Value;

    public static void WaitWithTimeoutPerException(this Task task, int timeoutMilli)
    {
      if (task == null)
        return;
      long num1 = Glob.StopwatchZaitMiliSictInt();
      while (!task.IsCompleted)
      {
        long num2 = Glob.StopwatchZaitMiliSictInt() - num1;
        if ((long) timeoutMilli < num2)
          throw new TimeoutException(task?.ToString());
        Thread.Sleep(11);
      }
    }

    public static IEqualityComparer<T> EqualityComparerFromFunc<T>(
      this Func<T, T, bool> equals,
      Func<T, int> getHashCode = null)
    {
      return (IEqualityComparer<T>) new Extension.EqualityComparer<T>()
      {
        EqualsImpl = equals,
        GetHashCodeImpl = getHashCode
      };
    }

    public static bool ContainsAll<RequiredT, T1>(
      this IEnumerable<RequiredT> requiredSet,
      IEnumerable<T1> set1,
      Func<RequiredT, T1, bool> callbackMatch)
    {
      return requiredSet == null || requiredSet.All<RequiredT>((Func<RequiredT, bool>) (requiredItem =>
      {
        IEnumerable<T1> source = set1;
        return source != null && source.Any<T1>((Func<T1, bool>) (set1Item =>
        {
          Func<RequiredT, T1, bool> func = callbackMatch;
          return func != null && func(requiredItem, set1Item);
        }));
      }));
    }

    public static bool ContainsAll<T>(
      this IEnumerable<T> requiredSet,
      IEnumerable<T> set1,
      IEqualityComparer<T> equalityComparer)
    {
      return requiredSet.ContainsAll<T, T>(set1, (Func<T, T, bool>) ((requiredItem, set1Item) =>
      {
        IEqualityComparer<T> equalityComparer1 = equalityComparer;
        return equalityComparer1 != null && equalityComparer1.Equals(requiredItem, set1Item);
      }));
    }

    public delegate TAus SelectWhereFuncMitIndex<in TAin, out TAus>(
      TAin ain,
      int index,
      out bool inkludiire);

    public delegate TAus SelectWhereFunc<in TAin, out TAus>(TAin ain, out bool inkludiire);

    private class EqualityComparer<T> : IEqualityComparer<T>
    {
      public Func<T, T, bool> EqualsImpl;
      public Func<T, int> GetHashCodeImpl;

      public bool Equals(T x, T y)
      {
        Func<T, T, bool> equalsImpl = this.EqualsImpl;
        return equalsImpl != null && equalsImpl(x, y);
      }

      public int GetHashCode(T obj)
      {
        if (this.GetHashCodeImpl != null)
          return this.GetHashCodeImpl(obj);
        return (object) obj == null ? 0 : obj.GetHashCode();
      }
    }
  }
}
