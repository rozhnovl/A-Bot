// Decompiled with JetBrains decompiler
// Type: Bib3.Glob
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using Bib3.Synchronization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Bib3
{
  public static class Glob
  {
    public static T[] ArrayAusscnit<T>(
      this T[] source,
      long ausscnitBeginIndex,
      long ausscnitLänge)
    {
      if (source == null || ausscnitBeginIndex < 0L || ausscnitLänge < 0L)
        return (T[]) null;
      long num = ausscnitBeginIndex + ausscnitLänge;
      if ((long) source.Length < num)
        return (T[]) null;
      T[] destinationArray = new T[ausscnitLänge];
      Array.Copy((Array) source, ausscnitBeginIndex, (Array) destinationArray, 0L, ausscnitLänge);
      return destinationArray;
    }

    public static T[] AusListeListeAusscnit<T>(
      this IEnumerable<T[]> listeListe,
      long ausscnitBeginIndex,
      long ausscnitLänge,
      bool erlaubeTailmengeBegin = false)
    {
      if (ausscnitLänge < 0L)
        return (T[]) null;
      long num1 = ausscnitBeginIndex + ausscnitLänge;
      if (listeListe == null || ausscnitBeginIndex < 0L)
        return (T[]) null;
      T[] objArray = new T[ausscnitLänge];
      long num2 = 0;
      foreach (T[] sourceArray in listeListe)
      {
        if (sourceArray != null)
        {
          long num3 = num2 + (long) sourceArray.Length;
          long destinationIndex = Math.Max(0L, num2 - ausscnitBeginIndex);
          long sourceIndex = ausscnitBeginIndex - num2;
          long length = num1 - num2;
          if (sourceIndex < 0L)
          {
            if (length >= 0L)
            {
              if (length < (long) sourceArray.Length)
                Array.Copy((Array) sourceArray, 0L, (Array) objArray, destinationIndex, length);
              else
                Array.Copy((Array) sourceArray, 0L, (Array) objArray, destinationIndex, (long) sourceArray.Length);
            }
            else
              break;
          }
          else if ((long) sourceArray.Length > sourceIndex)
          {
            if (length < (long) sourceArray.Length)
              Array.Copy((Array) sourceArray, sourceIndex, (Array) objArray, destinationIndex, length - sourceIndex);
            else
              Array.Copy((Array) sourceArray, sourceIndex, (Array) objArray, destinationIndex, (long) sourceArray.Length - sourceIndex);
          }
          else
            continue;
          num2 = num3;
        }
      }
      if (num2 >= num1)
        return objArray;
      if (!erlaubeTailmengeBegin)
        return (T[]) null;
      T[] destinationArray = new T[num2 - ausscnitBeginIndex];
      Array.Copy((Array) objArray, (Array) destinationArray, (long) destinationArray.Length);
      return destinationArray;
    }

    public static IEnumerable<int> IntZertaile(this int @int, int tailGrööseScranke)
    {
      int TailGrööse;
      for (int Rest = @int; 0 < Rest; Rest -= TailGrööse)
      {
        TailGrööse = Math.Min(tailGrööseScranke, Rest);
        yield return TailGrööse;
      }
    }

    public static long? Min(IEnumerable<long?> enumerable)
    {
      if (enumerable == null)
        return new long?();
      long? nullable1 = new long?();
      foreach (long? nullable2 in enumerable)
      {
        if (nullable2.HasValue)
          nullable1 = !nullable1.HasValue ? new long?(nullable2.Value) : new long?(Math.Min(nullable1.Value, nullable2.Value));
      }
      return nullable1;
    }

    public static int? MaxAusSelector<T>(IEnumerable<T> enumerable, Func<T, int?> selector)
    {
      if (enumerable == null)
        return new int?();
      return selector == null ? new int?() : Glob.Max(enumerable.Select<T, int?>(selector));
    }

    public static long? MaxAusSelector<T>(IEnumerable<T> enumerable, Func<T, long?> selector)
    {
      if (enumerable == null)
        return new long?();
      return selector == null ? new long?() : Glob.Max(enumerable.Select<T, long?>(selector));
    }

    public static DateTime Max(DateTime zait0, DateTime zait1) => zait0 < zait1 ? zait1 : zait0;

    public static int? Max(IEnumerable<int?> enumerable)
    {
      long? nullable1 = Glob.Max(enumerable != null ? enumerable.Select<int?, long?>((Func<int?, long?>) (t =>
      {
        int? nullable2 = t;
        return !nullable2.HasValue ? new long?() : new long?((long) nullable2.GetValueOrDefault());
      })) : (IEnumerable<long?>) null);
      return nullable1.HasValue ? new int?((int) nullable1.GetValueOrDefault()) : new int?();
    }

    public static long? Max(long? o0, long? o1) => Glob.Max((IEnumerable<long?>) new long?[2]
    {
      o0,
      o1
    });

    public static int? Min(IEnumerable<int?> enumerable)
    {
      long? nullable1 = Glob.Min(enumerable != null ? enumerable.Select<int?, long?>((Func<int?, long?>) (t =>
      {
        int? nullable2 = t;
        return !nullable2.HasValue ? new long?() : new long?((long) nullable2.GetValueOrDefault());
      })) : (IEnumerable<long?>) null);
      return nullable1.HasValue ? new int?((int) nullable1.GetValueOrDefault()) : new int?();
    }

    public static long? Min(long? o0, long? o1) => Glob.Min((IEnumerable<long?>) new long?[2]
    {
      o0,
      o1
    });

    public static int? Max(int? o0, int? o1)
    {
      int? nullable1 = o0;
      long? o0_1 = nullable1.HasValue ? new long?((long) nullable1.GetValueOrDefault()) : new long?();
      nullable1 = o1;
      long? o1_1 = nullable1.HasValue ? new long?((long) nullable1.GetValueOrDefault()) : new long?();
      long? nullable2 = Glob.Max(o0_1, o1_1);
      int? nullable3;
      if (!nullable2.HasValue)
      {
        nullable1 = new int?();
        nullable3 = nullable1;
      }
      else
        nullable3 = new int?((int) nullable2.GetValueOrDefault());
      return nullable3;
    }

    public static int? Min(int? o0, int? o1)
    {
      int? nullable1 = o0;
      long? o0_1 = nullable1.HasValue ? new long?((long) nullable1.GetValueOrDefault()) : new long?();
      nullable1 = o1;
      long? o1_1 = nullable1.HasValue ? new long?((long) nullable1.GetValueOrDefault()) : new long?();
      long? nullable2 = Glob.Min(o0_1, o1_1);
      int? nullable3;
      if (!nullable2.HasValue)
      {
        nullable1 = new int?();
        nullable3 = nullable1;
      }
      else
        nullable3 = new int?((int) nullable2.GetValueOrDefault());
      return nullable3;
    }

    public static float? Max(float? o0, float? o1) => o0.HasValue ? (o1.HasValue ? new float?(Math.Max(o0.Value, o1.Value)) : o0) : (o1.HasValue ? o1 : new float?());

    public static float? Min(float? o0, float? o1) => o0.HasValue ? (o1.HasValue ? new float?(Math.Min(o0.Value, o1.Value)) : o0) : (o1.HasValue ? o1 : new float?());

    public static double? Max(double? o0, double? o1) => o0.HasValue ? (o1.HasValue ? new double?(Math.Max(o0.Value, o1.Value)) : o0) : (o1.HasValue ? o1 : new double?());

    public static double? Min(double? o0, double? o1) => o0.HasValue ? (o1.HasValue ? new double?(Math.Min(o0.Value, o1.Value)) : o0) : (o1.HasValue ? o1 : new double?());

    public static float? Max(IEnumerable<float?> enumerable)
    {
      if (enumerable == null)
        return new float?();
      float? nullable1 = new float?();
      foreach (float? nullable2 in enumerable)
      {
        if (nullable2.HasValue)
          nullable1 = !nullable1.HasValue ? new float?(nullable2.Value) : new float?(Math.Max(nullable1.Value, nullable2.Value));
      }
      return nullable1;
    }

    public static long? Max(IEnumerable<long?> enumerable)
    {
      if (enumerable == null)
        return new long?();
      long? nullable1 = new long?();
      foreach (long? nullable2 in enumerable)
      {
        if (nullable2.HasValue)
          nullable1 = !nullable1.HasValue ? new long?(nullable2.Value) : new long?(Math.Max(nullable1.Value, nullable2.Value));
      }
      return nullable1;
    }

    public static bool SequenceEqualPerObjectEquals(
      IEnumerable collection0,
      IEnumerable collection1)
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
      while (object.Equals(enumerator1.Current, enumerator2.Current));
      goto label_9;
label_5:
      return !(flag1 | flag2);
label_9:
      return false;
    }

    public static IEnumerable<Type> MengeTypeVerwandtEnum(this Type type)
    {
      if (!((Type) null == type))
      {
        if (type.IsGenericType)
        {
          Type[] GenericArguments = type.GetGenericArguments();
          Type[] typeArray = GenericArguments;
          for (int index = 0; index < typeArray.Length; ++index)
          {
            Type Arg = typeArray[index];
            if (!((Type) null == Arg))
            {
              yield return Arg;
              Arg = (Type) null;
            }
          }
          typeArray = (Type[]) null;
          GenericArguments = (Type[]) null;
        }
        Type[] Interfaces = type.GetInterfaces();
        if (Interfaces != null)
        {
          Type[] typeArray = Interfaces;
          for (int index = 0; index < typeArray.Length; ++index)
          {
            Type Interface = typeArray[index];
            if (!((Type) null == Interface))
            {
              yield return Interface;
              Interface = (Type) null;
            }
          }
          typeArray = (Type[]) null;
        }
        Type Base = type.BaseType;
        if ((Type) null != Base)
          yield return Base;
      }
    }

    public static IEnumerable<Type> MengeTypeVerwandtEnumTransitiiv(this Type type)
    {
      Dictionary<Type, bool> Dict = new Dictionary<Type, bool>();
      Queue<Type> Queue = new Queue<Type>(type.Yield<Type>());
      while (0 < Queue.Count)
      {
        Type Next = Queue.Dequeue();
        if (!((Type) null == Next) && !Dict.ContainsKey(Next))
        {
          Dict[Next] = true;
          yield return Next;
          IEnumerable<Type> menge = Next.MengeTypeVerwandtEnum();
          if (menge != null)
            menge.ForEach<Type>(new Action<Type>(Queue.Enqueue));
          Next = (Type) null;
        }
      }
    }

    public static Type[] ListeTypeArgumentZuBaseOderInterface(
      this Type type,
      Type kandidaatBaseGenericTypeDefinition)
    {
      if ((Type) null == type || (Type) null == kandidaatBaseGenericTypeDefinition)
        return (Type[]) null;
      if (kandidaatBaseGenericTypeDefinition.IsInterface)
      {
        Type[] interfaces = type.GetInterfaces();
        Type[] seq1;
        if (!type.IsInterface)
          seq1 = (Type[]) null;
        else
          seq1 = new Type[1]{ type };
        IEnumerable<Type> source = ((IEnumerable<Type>) interfaces).ConcatNullable<Type>((IEnumerable<Type>) seq1);
        Type[] array = source != null ? source.ToArray<Type>() : (Type[]) null;
        if (array != null)
        {
          foreach (Type type1 in array)
          {
            if (type1.IsGenericType && type1.GetGenericTypeDefinition().Equals(kandidaatBaseGenericTypeDefinition))
              return type1.GenericTypeArguments;
          }
        }
      }
      else
      {
        for (Type type2 = type; (Type) null != type2; type2 = type2.BaseType)
        {
          if (type2.IsGenericType && type2.GetGenericTypeDefinition().Equals(kandidaatBaseGenericTypeDefinition))
            return type2.GenericTypeArguments;
        }
      }
      return (Type[]) null;
    }

    public static Type GenericTypeZuBaseOderInterfaceGenericTypeDefinition(
      this Type type,
      Type kandidaatBaseGenericTypeDefinition)
    {
      if ((Type) null == type || (Type) null == kandidaatBaseGenericTypeDefinition)
        return (Type) null;
      if (kandidaatBaseGenericTypeDefinition.IsInterface)
      {
        Type[] interfaces = type.GetInterfaces();
        if (interfaces != null)
        {
          foreach (Type type1 in interfaces)
          {
            if (type1.IsGenericType && type1.GetGenericTypeDefinition().Equals(kandidaatBaseGenericTypeDefinition))
              return type1;
          }
        }
      }
      else
      {
        for (Type type2 = type; (Type) null != type2; type2 = type2.BaseType)
        {
          if (type2.IsGenericType && type2.GetGenericTypeDefinition().Equals(kandidaatBaseGenericTypeDefinition))
            return type2;
        }
      }
      return (Type) null;
    }

    public static IEnumerable<T> ListeEnumerableAgregiirt<T>(
      IEnumerable<T> enum0,
      IEnumerable<T> enum1,
      IEnumerable<T> enum2 = null)
    {
      return ((IEnumerable<IEnumerable<T>>) new IEnumerable<T>[3]
      {
        enum0,
        enum1,
        enum2
      }).ListeEnumerableAgregiirt<T>();
    }

    public static IEnumerable<T> ListeEnumerableAgregiirt<T>(
      this IEnumerable<IEnumerable<T>> listeEnumerable)
    {
      if (listeEnumerable == null)
        return (IEnumerable<T>) null;
      IEnumerable<T> first = (IEnumerable<T>) null;
      foreach (IEnumerable<T> liste in listeEnumerable)
      {
        if (liste != null)
          first = first != null ? first.Concat<T>(liste) : liste;
      }
      return first;
    }

    public static DirectoryInfo SubDirectoryMitPfaad(this DirectoryInfo directory, string pfaad)
    {
      if (pfaad == null)
        return (DirectoryInfo) null;
      string[] strArray = pfaad.Split(Path.DirectorySeparatorChar);
      DirectoryInfo directoryInfo = directory;
      for (int index = 0; index < strArray.Length; ++index)
      {
        if (directoryInfo == null)
          return (DirectoryInfo) null;
        string PfaadKomponente = strArray[index];
        directoryInfo = ((IEnumerable<DirectoryInfo>) directoryInfo.GetDirectories(PfaadKomponente)).FirstOrDefault<DirectoryInfo>((Func<DirectoryInfo, bool>) (kandidaat => string.Equals(kandidaat.Name, PfaadKomponente, StringComparison.InvariantCultureIgnoreCase)));
      }
      return directoryInfo;
    }

    public static void OrdnungRicte<T>(this IList<T> liste, IComparer<T> comparer)
    {
      if (liste == null || comparer == null)
        return;
      bool flag1 = false;
      while (!flag1)
      {
        flag1 = true;
        bool flag2 = false;
        for (int index = 0; index < liste.Count - 1; ++index)
        {
          T x = liste[index];
          T y = liste[index + 1];
          if (comparer.Compare(x, y) < 0)
          {
            liste[index] = y;
            liste[index + 1] = x;
            flag2 = true;
            break;
          }
        }
        if (flag2)
          flag1 = false;
      }
    }

    public static SictPropagiireListeRepräsentatioonProfile PropagiireListeRepräsentatioon<KweleTyp, ZiilTyp>(
      this IList<KweleTyp> kweleListe,
      IList<ZiilTyp> ziilListe,
      Func<KweleTyp, ZiilTyp> funktioonRepräsentatioonKonstruktioon,
      Func<ZiilTyp, KweleTyp> funktioonErmitlungRepräsentiirte,
      Func<KweleTyp, KweleTyp, bool> funktioonVerglaic = null,
      Action<ZiilTyp> aktioonFürBeraitsVorhandeneRepräsentatioon = null)
      where ZiilTyp : class
    {
      return kweleListe.PropagiireListeRepräsentatioon<KweleTyp, ZiilTyp>(ziilListe as IList, funktioonRepräsentatioonKonstruktioon, funktioonErmitlungRepräsentiirte, funktioonVerglaic, aktioonFürBeraitsVorhandeneRepräsentatioon == null ? (Action<ZiilTyp, KweleTyp>) null : (Action<ZiilTyp, KweleTyp>) ((ziil, kwele) => aktioonFürBeraitsVorhandeneRepräsentatioon(ziil)));
    }

    public static SictPropagiireListeRepräsentatioonProfile PropagiireListeRepräsentatioon<KweleTyp, ZiilTyp>(
      this IList<KweleTyp> kweleListe,
      IList ziilListe,
      Func<KweleTyp, ZiilTyp> funktioonRepräsentatioonKonstruktioon,
      Func<ZiilTyp, KweleTyp> funktioonErmitlungRepräsentiirte,
      Func<KweleTyp, KweleTyp, bool> funktioonVerglaic = null,
      Action<ZiilTyp, KweleTyp> aktioonFürBeraitsVorhandeneRepräsentatioon = null)
      where ZiilTyp : class
    {
      if (ziilListe == null)
        return (SictPropagiireListeRepräsentatioonProfile) null;
      SictPropagiireListeRepräsentatioonProfile repräsentatioonProfile = new SictPropagiireListeRepräsentatioonProfile();
      if (kweleListe == null)
      {
        repräsentatioonProfile.EntferntAnzaal = ziilListe.Count;
        ziilListe.Clear();
        return repräsentatioonProfile;
      }
      if (funktioonRepräsentatioonKonstruktioon == null)
        throw new ArgumentNullException("FunktioonRepräsentatioonKonstruktioon");
      if (funktioonErmitlungRepräsentiirte == null)
        throw new ArgumentNullException("FunktioonErmitlungRepräsentiirte");
      if (funktioonVerglaic == null)
        funktioonVerglaic = (Func<KweleTyp, KweleTyp, bool>) ((obj0, obj1) => object.Equals((object) obj0, (object) obj1));
      List<KeyValuePair<ZiilTyp, KweleTyp>> keyValuePairList = new List<KeyValuePair<ZiilTyp, KweleTyp>>();
      foreach (object key in (IEnumerable) ziilListe)
        keyValuePairList.Add(new KeyValuePair<ZiilTyp, KweleTyp>(key as ZiilTyp, funktioonErmitlungRepräsentiirte(key as ZiilTyp)));
      List<ZiilTyp> second = new List<ZiilTyp>();
      List<KweleTyp> MengeObjektBeraitsRepräsentiirt = new List<KweleTyp>();
      foreach (object obj in (IEnumerable) ziilListe)
      {
        if (obj is ZiilTyp ziilTyp)
        {
          KweleTyp VorherRepräsentiirte = funktioonErmitlungRepräsentiirte(ziilTyp);
          KweleTyp[] array = kweleListe.Where<KweleTyp>((Func<KweleTyp, bool>) (kandidaat => funktioonVerglaic(VorherRepräsentiirte, kandidaat))).Take<KweleTyp>(1).ToArray<KweleTyp>();
          if (array.Length >= 1)
          {
            KweleTyp kweleTyp = ((IEnumerable<KweleTyp>) array).FirstOrDefault<KweleTyp>();
            if ((object) kweleTyp != null)
            {
              second.Add(ziilTyp);
              if (aktioonFürBeraitsVorhandeneRepräsentatioon != null)
                aktioonFürBeraitsVorhandeneRepräsentatioon(ziilTyp, kweleTyp);
              MengeObjektBeraitsRepräsentiirt.Add(kweleTyp);
            }
          }
        }
      }
      foreach (object obj in ziilListe.OfType<object>().Except<object>((IEnumerable<object>) second).ToArray<object>())
      {
        ziilListe.Remove(obj);
        ++repräsentatioonProfile.EntferntAnzaal;
      }
      foreach (KweleTyp kweleTyp in kweleListe.Where<KweleTyp>((Func<KweleTyp, bool>) (kandidaatReprZuErsctele => MengeObjektBeraitsRepräsentiirt.All<KweleTyp>((Func<KweleTyp, bool>) (kandidaatBeraitsRepräsentiirt => !funktioonVerglaic(kandidaatReprZuErsctele, kandidaatBeraitsRepräsentiirt))))).ToArray<KweleTyp>())
      {
        ZiilTyp ziilTyp = funktioonRepräsentatioonKonstruktioon(kweleTyp);
        ziilListe.Add((object) ziilTyp);
        ++repräsentatioonProfile.KonstruiirtAnzaal;
      }
      return repräsentatioonProfile;
    }

    public static SictPropagiireListeRepräsentatioonProfile PropagiireListeRepräsentatioon<KweleTyp, ZiilTyp>(
      this IEnumerable<KweleTyp> kweleListe,
      IList<ZiilTyp> ziilListe,
      Func<KweleTyp, ZiilTyp> funktioonRepräsentatioonKonstruktioon,
      Func<ZiilTyp, KweleTyp, bool> funktioonEntscaidungObRepräsentatioonPasendZuKweleObj,
      Action<ZiilTyp, KweleTyp> aktioonFürRepräsentatioon = null,
      bool repräsentatioonEntferneNict = false)
      where ZiilTyp : class
    {
      return kweleListe.PropagiireListeRepräsentatioon<KweleTyp, ZiilTyp>(ziilListe as IList, funktioonRepräsentatioonKonstruktioon, funktioonEntscaidungObRepräsentatioonPasendZuKweleObj, aktioonFürRepräsentatioon, repräsentatioonEntferneNict);
    }

    public static SictPropagiireListeRepräsentatioonProfile PropagiireMengeRepräsentatioonMitBedingungScatescpaicerSequenceEqualNit<KweleTyp, ZiilTyp>(
      this IEnumerable<KweleTyp> kweleListe,
      ref KweleTyp[] scatescpaicer,
      ICollection<ZiilTyp> ziilMenge,
      Func<KweleTyp, ZiilTyp> funktioonRepräsentatioonKonstruktioon,
      Func<ZiilTyp, KweleTyp, bool> funktioonEntscaidungObRepräsentatioonPasendZuKweleObj,
      Action<ZiilTyp, KweleTyp> aktioonFürRepräsentatioon = null,
      bool repräsentatioonEntferneNict = false)
    {
      KweleTyp[] array = kweleListe != null ? kweleListe.ToArray<KweleTyp>() : (KweleTyp[]) null;
      if (Glob.SequenceEqual<ZiilTyp, KweleTyp>((IEnumerable<ZiilTyp>) ziilMenge, (IEnumerable<KweleTyp>) array, funktioonEntscaidungObRepräsentatioonPasendZuKweleObj))
      {
        if (ziilMenge != null && array != null)
        {
          for (int index = 0; index < array.Length; ++index)
            aktioonFürRepräsentatioon(ziilMenge.ElementAtOrDefault<ZiilTyp>(index), ((IEnumerable<KweleTyp>) array).ElementAtOrDefault<KweleTyp>(index));
        }
        return (SictPropagiireListeRepräsentatioonProfile) null;
      }
      scatescpaicer = array != null ? ((IEnumerable<KweleTyp>) array).ToArray<KweleTyp>() : (KweleTyp[]) null;
      return ((IEnumerable<KweleTyp>) array).PropagiireMengeRepräsentatioon<KweleTyp, ZiilTyp>(ziilMenge, funktioonRepräsentatioonKonstruktioon, funktioonEntscaidungObRepräsentatioonPasendZuKweleObj, aktioonFürRepräsentatioon, repräsentatioonEntferneNict);
    }

    public static SictPropagiireListeRepräsentatioonProfile PropagiireMengeRepräsentatioonMitBedingungScatescpaicerSequenceEqualNit<KweleTyp, ZiilTyp>(
      this IEnumerable<KweleTyp> kweleListe,
      ref KweleTyp[] scatescpaicer,
      ICollection<ZiilTyp> ziilMenge,
      Func<KweleTyp, ZiilTyp> funktioonRepräsentatioonKonstruktioon,
      Func<ZiilTyp, KweleTyp, bool> funktioonEntscaidungObRepräsentatioonPasendZuKweleObj,
      Action<ZiilTyp> aktioonFürRepräsentatioon = null,
      bool repräsentatioonEntferneNict = false)
    {
      return kweleListe.PropagiireMengeRepräsentatioonMitBedingungScatescpaicerSequenceEqualNit<KweleTyp, ZiilTyp>(ref scatescpaicer, ziilMenge, funktioonRepräsentatioonKonstruktioon, funktioonEntscaidungObRepräsentatioonPasendZuKweleObj, (Action<ZiilTyp, KweleTyp>) ((ziilElement, kweleElement) => aktioonFürRepräsentatioon(ziilElement)), repräsentatioonEntferneNict);
    }

    public static SictPropagiireListeRepräsentatioonProfile PropagiireMengeRepräsentatioon<KweleTyp, ZiilTyp>(
      this IEnumerable<KweleTyp> kweleListe,
      ICollection<ZiilTyp> ziilMenge,
      Func<KweleTyp, ZiilTyp> funktioonRepräsentatioonKonstruktioon,
      Func<ZiilTyp, KweleTyp, bool> funktioonEntscaidungObRepräsentatioonPasendZuKweleObj,
      Action<ZiilTyp, KweleTyp> aktioonFürRepräsentatioon = null,
      bool repräsentatioonEntferneNict = false)
    {
      if (ziilMenge == null)
        return (SictPropagiireListeRepräsentatioonProfile) null;
      SictPropagiireListeRepräsentatioonProfile repräsentatioonProfile = new SictPropagiireListeRepräsentatioonProfile();
      if (kweleListe == null)
      {
        if (!repräsentatioonEntferneNict)
        {
          repräsentatioonProfile.EntferntAnzaal = ziilMenge.Count;
          ziilMenge.Clear();
        }
        return repräsentatioonProfile;
      }
      if (funktioonRepräsentatioonKonstruktioon == null)
        throw new ArgumentNullException("FunktioonRepräsentatioonKonstruktioon");
      if (funktioonEntscaidungObRepräsentatioonPasendZuKweleObj == null)
        throw new ArgumentNullException("FunktioonEntscaidungObRepräsentatioonPasendZuKweleObj");
      List<ZiilTyp> second = new List<ZiilTyp>();
      List<KweleTyp> kweleTypList = new List<KweleTyp>();
      foreach (KweleTyp kweleTyp in kweleListe)
      {
        ZiilTyp ziilTyp1 = default (ZiilTyp);
        bool flag = false;
        foreach (ZiilTyp ziilTyp2 in (IEnumerable<ZiilTyp>) ziilMenge)
        {
          if (funktioonEntscaidungObRepräsentatioonPasendZuKweleObj(ziilTyp2, kweleTyp))
          {
            ZiilTyp ziilTyp3 = ziilTyp2;
            second.Add(ziilTyp3);
            if (aktioonFürRepräsentatioon != null)
              aktioonFürRepräsentatioon(ziilTyp3, kweleTyp);
            flag = true;
          }
        }
        if (!flag)
          kweleTypList.Add(kweleTyp);
      }
      if (repräsentatioonEntferneNict)
      {
        foreach (ZiilTyp ziilTyp4 in (IEnumerable<ZiilTyp>) ziilMenge)
        {
          if ((object) ziilTyp4 != null && !second.Contains(ziilTyp4))
          {
            ZiilTyp ziilTyp5 = ziilTyp4;
            if (aktioonFürRepräsentatioon != null)
              aktioonFürRepräsentatioon(ziilTyp5, default (KweleTyp));
          }
        }
      }
      else
      {
        foreach (ZiilTyp ziilTyp in ziilMenge.OfType<ZiilTyp>().Except<ZiilTyp>((IEnumerable<ZiilTyp>) second).ToArray<ZiilTyp>())
        {
          ziilMenge.Remove(ziilTyp);
          ++repräsentatioonProfile.EntferntAnzaal;
        }
      }
      foreach (KweleTyp kweleTyp in kweleTypList)
      {
        ZiilTyp ziilTyp = funktioonRepräsentatioonKonstruktioon(kweleTyp);
        ziilMenge.Add(ziilTyp);
        ++repräsentatioonProfile.KonstruiirtAnzaal;
        if (aktioonFürRepräsentatioon != null)
          aktioonFürRepräsentatioon(ziilTyp, kweleTyp);
      }
      return repräsentatioonProfile;
    }

    public static SictPropagiireListeRepräsentatioonProfile PropagiireListeRepräsentatioonMitBedingungScatescpaicerSequenceEqualNit<KweleTyp, ZiilTyp>(
      this IEnumerable<KweleTyp> kweleListe,
      ref KweleTyp[] scatescpaicer,
      IList ziilListe,
      Func<KweleTyp, ZiilTyp> funktioonRepräsentatioonKonstruktioon,
      Func<ZiilTyp, KweleTyp, bool> funktioonEntscaidungObRepräsentatioonPasendZuKweleObj,
      Action<ZiilTyp> aktioonFürRepräsentatioon = null,
      bool repräsentatioonEntferneNict = false)
      where ZiilTyp : class
    {
      KweleTyp[] array = kweleListe != null ? kweleListe.ToArray<KweleTyp>() : (KweleTyp[]) null;
      if (Glob.SequenceEqualPerObjectEquals((IEnumerable) scatescpaicer, (IEnumerable) array))
      {
        if (ziilListe != null)
        {
          foreach (object obj in (IEnumerable) ziilListe)
            aktioonFürRepräsentatioon(obj as ZiilTyp);
        }
        return (SictPropagiireListeRepräsentatioonProfile) null;
      }
      scatescpaicer = array != null ? ((IEnumerable<KweleTyp>) array).ToArray<KweleTyp>() : (KweleTyp[]) null;
      return ((IEnumerable<KweleTyp>) array).PropagiireListeRepräsentatioon<KweleTyp, ZiilTyp>(ziilListe, funktioonRepräsentatioonKonstruktioon, funktioonEntscaidungObRepräsentatioonPasendZuKweleObj, (Action<ZiilTyp, KweleTyp>) ((ziilElement, kweleElement) => aktioonFürRepräsentatioon(ziilElement)), repräsentatioonEntferneNict);
    }

    public static SictPropagiireListeRepräsentatioonProfile PropagiireListeRepräsentatioon<KweleTyp, ZiilTyp>(
      this IEnumerable<KweleTyp> kweleListe,
      IList ziilListe,
      Func<KweleTyp, ZiilTyp> funktioonRepräsentatioonKonstruktioon,
      Func<ZiilTyp, KweleTyp, bool> funktioonEntscaidungObRepräsentatioonPasendZuKweleObj,
      Action<ZiilTyp, KweleTyp> aktioonFürRepräsentatioon = null,
      bool repräsentatioonEntferneNict = false)
      where ZiilTyp : class
    {
      if (ziilListe == null)
        return (SictPropagiireListeRepräsentatioonProfile) null;
      SictPropagiireListeRepräsentatioonProfile repräsentatioonProfile = new SictPropagiireListeRepräsentatioonProfile();
      if (kweleListe == null)
      {
        if (!repräsentatioonEntferneNict)
        {
          repräsentatioonProfile.EntferntAnzaal = ziilListe.Count;
          ziilListe.Clear();
        }
        return repräsentatioonProfile;
      }
      if (funktioonRepräsentatioonKonstruktioon == null)
        throw new ArgumentNullException("FunktioonRepräsentatioonKonstruktioon");
      if (funktioonEntscaidungObRepräsentatioonPasendZuKweleObj == null)
        throw new ArgumentNullException("FunktioonEntscaidungObRepräsentatioonPasendZuKweleObj");
      List<ZiilTyp> ziilTypList = new List<ZiilTyp>();
      List<KweleTyp> kweleTypList = new List<KweleTyp>();
      foreach (KweleTyp kweleTyp in kweleListe)
      {
        ZiilTyp ziilTyp1 = default (ZiilTyp);
        bool flag = false;
        foreach (object obj in (IEnumerable) ziilListe)
        {
          ZiilTyp ziilTyp2 = obj as ZiilTyp;
          if (funktioonEntscaidungObRepräsentatioonPasendZuKweleObj(ziilTyp2, kweleTyp))
          {
            ZiilTyp ziilTyp3 = ziilTyp2;
            ziilTypList.Add(ziilTyp3);
            if (aktioonFürRepräsentatioon != null)
              aktioonFürRepräsentatioon(ziilTyp3, kweleTyp);
            flag = true;
          }
        }
        if (!flag)
          kweleTypList.Add(kweleTyp);
      }
      if (repräsentatioonEntferneNict)
      {
        foreach (object obj in (IEnumerable) ziilListe)
        {
          if (obj != null && !((IEnumerable<object>) ziilTypList).Contains<object>(obj))
          {
            ZiilTyp ziilTyp = obj as ZiilTyp;
            if (aktioonFürRepräsentatioon != null)
              aktioonFürRepräsentatioon(ziilTyp, default (KweleTyp));
          }
        }
      }
      else
      {
        foreach (object obj in ziilListe.OfType<object>().Except<object>((IEnumerable<object>) ziilTypList).ToArray<object>())
        {
          ziilListe.Remove(obj);
          ++repräsentatioonProfile.EntferntAnzaal;
        }
      }
      foreach (KweleTyp kweleTyp in kweleTypList)
      {
        ZiilTyp ziilTyp = funktioonRepräsentatioonKonstruktioon(kweleTyp);
        ziilListe.Add((object) ziilTyp);
        ++repräsentatioonProfile.KonstruiirtAnzaal;
        if (aktioonFürRepräsentatioon != null)
          aktioonFürRepräsentatioon(ziilTyp, kweleTyp);
      }
      return repräsentatioonProfile;
    }

    public static SictPropagiireListeRepräsentatioonProfile PropagiireListeRepräsentatioonMitReprUndIdentPerClrReferenz<KweleUndZiilTyp>(
      this IEnumerable<KweleUndZiilTyp> kweleListe,
      IList ziilListe,
      Action<KweleUndZiilTyp, KweleUndZiilTyp> aktioonFürRepräsentatioon = null,
      bool repräsentatioonEntferneNict = false)
      where KweleUndZiilTyp : class
    {
      return kweleListe.PropagiireListeRepräsentatioon<KweleUndZiilTyp, KweleUndZiilTyp>(ziilListe, (Func<KweleUndZiilTyp, KweleUndZiilTyp>) (zuRepräsentiirende => zuRepräsentiirende), (Func<KweleUndZiilTyp, KweleUndZiilTyp, bool>) ((zuRepräsentiirende, repräsentatioon) => (object) zuRepräsentiirende == (object) repräsentatioon), aktioonFürRepräsentatioon, repräsentatioonEntferneNict);
    }

    public static SictPropagiireListeRepräsentatioonProfile PropagiireListeRepräsentatioonStringEquals(
      this IEnumerable<string> kweleListe,
      IList ziilListe,
      bool repräsentatioonEntferneNict = false)
    {
      return kweleListe.PropagiireListeRepräsentatioon<string, string>(ziilListe, (Func<string, string>) (zuRepräsentiirende => zuRepräsentiirende), (Func<string, string, bool>) ((zuRepräsentiirende, repräsentatioon) => string.Equals(zuRepräsentiirende, repräsentatioon)), repräsentatioonEntferneNict: repräsentatioonEntferneNict);
    }

    public static void HueSatValKonvertiirtNaacRGB(
      int hue,
      int sat,
      int val,
      int hueSatValWerteberaicMax,
      int rgbWerteberaicMax,
      out int r,
      out int g,
      out int b)
    {
      r = -1;
      g = -1;
      b = -1;
      hue = (hue % hueSatValWerteberaicMax + hueSatValWerteberaicMax) % hueSatValWerteberaicMax;
      sat = Math.Max(0, Math.Min(sat, hueSatValWerteberaicMax));
      val = Math.Max(0, Math.Min(val, hueSatValWerteberaicMax));
      int num1 = hue * 6 / hueSatValWerteberaicMax;
      int num2 = hue * 6 % hueSatValWerteberaicMax;
      long num3 = (long) val * (long) (hueSatValWerteberaicMax - sat);
      long num4 = (long) val * (long) (hueSatValWerteberaicMax * hueSatValWerteberaicMax - sat * num2);
      long num5 = (long) val * (long) (hueSatValWerteberaicMax * hueSatValWerteberaicMax - sat * (hueSatValWerteberaicMax - num2));
      long num6 = (long) rgbWerteberaicMax * num3 / (long) hueSatValWerteberaicMax / (long) hueSatValWerteberaicMax;
      long num7 = (long) rgbWerteberaicMax * num4 / (long) hueSatValWerteberaicMax / (long) hueSatValWerteberaicMax / (long) hueSatValWerteberaicMax;
      long num8 = (long) rgbWerteberaicMax * num5 / (long) hueSatValWerteberaicMax / (long) hueSatValWerteberaicMax / (long) hueSatValWerteberaicMax;
      long num9 = (long) val * (long) rgbWerteberaicMax / (long) hueSatValWerteberaicMax;
      switch (num1)
      {
        case 0:
          r = (int) num9;
          g = (int) num8;
          b = (int) num6;
          break;
        case 1:
          r = (int) num7;
          g = (int) num9;
          b = (int) num6;
          break;
        case 2:
          r = (int) num6;
          g = (int) num9;
          b = (int) num8;
          break;
        case 3:
          r = (int) num6;
          g = (int) num7;
          b = (int) num9;
          break;
        case 4:
          r = (int) num8;
          g = (int) num6;
          b = (int) num9;
          break;
        case 5:
          r = (int) num9;
          g = (int) num6;
          b = (int) num7;
          break;
      }
    }

    public static void SezeListeElement<T>(this IList<T> ziilListe, IEnumerable<T> kweleListe)
    {
      if (ziilListe == null)
        return;
      if (kweleListe == null)
      {
        ziilListe.Clear();
      }
      else
      {
        int index = 0;
        foreach (T obj in kweleListe)
        {
          if (ziilListe.Count <= index)
            ziilListe.Add(obj);
          else
            ziilListe[index] = obj;
          ++index;
        }
        if (index == 0)
          ziilListe.Clear();
        while (index < ziilListe.Count)
          ziilListe.RemoveAt(index);
      }
    }

    public static bool MengeEqual<T>(this IEnumerable<T> menge0, IEnumerable<T> menge1)
    {
      if (menge0 == menge1)
        return true;
      return menge0 != null && menge1 != null && menge0.Except<T>(menge1).Count<T>() == 0 && menge1.Except<T>(menge0).Count<T>() == 0;
    }

    public static bool MengeEqual<T>(
      this IEnumerable<T> menge0,
      IEnumerable<T> menge1,
      IEqualityComparer<T> equalityComparer)
    {
      if (menge0 == menge1)
        return true;
      return menge0 != null && menge1 != null && menge0.Except<T>(menge1, equalityComparer).Count<T>() == 0 && menge1.Except<T>(menge0, equalityComparer).Count<T>() == 0;
    }

    public static string DataiPfaadAlsKombinatioonAusSctandardPfaadUndAingaabePfaad(
      string sctandardPfaad,
      string aingaabePfaad)
    {
      if (aingaabePfaad == null)
        return sctandardPfaad;
      FileInfo fileInfo1 = new FileInfo(aingaabePfaad);
      if (fileInfo1.Exists)
        return fileInfo1.FullName;
      DirectoryInfo directoryInfo = new DirectoryInfo(aingaabePfaad);
      if (!directoryInfo.Exists)
        return (string) null;
      FileInfo fileInfo2 = new FileInfo(sctandardPfaad);
      return directoryInfo.FullName + Path.DirectorySeparatorChar.ToString() + fileInfo2.Name;
    }

    public static bool SequenceEqual<ElementT>(
      IEnumerable<ElementT> sequence0,
      IEnumerable<ElementT> sequence1)
    {
      return Glob.SequenceEqual<ElementT, ElementT>(sequence0, sequence1, (Func<ElementT, ElementT, bool>) ((element0, element1) => object.Equals((object) element0, (object) element1)));
    }

    public static bool SequenceEqual<ElementT0, ElementT1>(
      IEnumerable<ElementT0> sequence0,
      IEnumerable<ElementT1> sequence1,
      Func<ElementT0, ElementT1, bool> funkElementEqual)
    {
      if (sequence0 == sequence1)
        return true;
      if (sequence0 == null || sequence1 == null || funkElementEqual == null)
        return false;
      IEnumerator<ElementT0> enumerator1 = sequence0.GetEnumerator();
      IEnumerator<ElementT1> enumerator2 = sequence1.GetEnumerator();
      ElementT0 current1;
      ElementT1 current2;
      do
      {
        bool flag1 = enumerator1.MoveNext();
        bool flag2 = enumerator2.MoveNext();
        if (flag1 || flag2)
        {
          if (flag1 && flag2)
          {
            current1 = enumerator1.Current;
            current2 = enumerator2.Current;
          }
          else
            goto label_7;
        }
        else
          goto label_5;
      }
      while (funkElementEqual(current1, current2));
      goto label_9;
label_5:
      return true;
label_7:
      return false;
label_9:
      return false;
    }

    public static long Fakultäät(this long t)
    {
      long num = 1;
      for (int index = 1; (long) index <= t; ++index)
        num *= t;
      return num;
    }

    public static long Fakultäät(this int t) => ((long) t).Fakultäät();

    public static void InListeOrdnetFüügeAin<T>(
      IList<T> liste,
      Func<T, long> sictOrdnungPositioon,
      T ainzufüügeElement,
      bool vorhandeneAnPositionEntferne = false)
    {
      if (liste == null || sictOrdnungPositioon == null)
        return;
      long num1 = sictOrdnungPositioon(ainzufüügeElement);
      for (int index = 0; index < liste.Count; ++index)
      {
        T obj = liste[index];
        long num2 = sictOrdnungPositioon(obj);
        if (num1 == num2 && vorhandeneAnPositionEntferne)
        {
          liste[index] = ainzufüügeElement;
          return;
        }
        if (num1 < num2)
        {
          liste.Insert(index, ainzufüügeElement);
          return;
        }
      }
      liste.Add(ainzufüügeElement);
    }

    public static bool AstEnthalteInBaum<T>(
      this T suuceWurzel,
      T astIdent,
      Func<T, IEnumerable<T>> funcAusAstMengeReferenziirteAst,
      int? tiifeScrankeMax = null,
      int? tiifeScrankeMin = null)
    {
      return suuceWurzel.AstEnthalteInBaum<T>((Func<T, bool>) (kandidaatAst => object.Equals((object) kandidaatAst, (object) (T) astIdent)), funcAusAstMengeReferenziirteAst, tiifeScrankeMax, tiifeScrankeMin);
    }

    public static bool AstEnthalteInBaum<T>(
      this T suuceWurzel,
      Func<T, bool> prädikaat,
      Func<T, IEnumerable<T>> funcAusAstMengeReferenziirteAst,
      int? tiifeScrankeMax = null,
      int? tiifeScrankeMin = null)
    {
      return !suuceWurzel.SuuceFlacMengeAst<T>(prädikaat, funcAusAstMengeReferenziirteAst, new int?(1), tiifeScrankeMax, tiifeScrankeMin).IsNullOrEmpty();
    }

    public static T[] SuuceFlacMengeAst<T>(
      this IEnumerable<T> suuceMengeWurzel,
      Func<T, bool> prädikaat,
      Func<T, IEnumerable<T>> funcAusAstMengeReferenziirteAst,
      int? listeFundAnzaalScrankeMax = null,
      int? tiifeScrankeMax = null,
      int? tiifeScrankeMin = null,
      bool laseAusMengeChildUnterhalbTrefer = false)
    {
      return suuceMengeWurzel.SuuceFlacMengeAstAusMengeWurzel<T>(prädikaat, funcAusAstMengeReferenziirteAst, listeFundAnzaalScrankeMax, tiifeScrankeMax, tiifeScrankeMin, laseAusMengeChildUnterhalbTrefer);
    }

    public static T[] SuuceFlacMengeAstAusMengeWurzel<T>(
      this IEnumerable<T> suuceMengeWurzel,
      Func<T, bool> prädikaat,
      Func<T, IEnumerable<T>> funcAusAstMengeReferenziirteAst,
      int? listeFundAnzaalScrankeMax = null,
      int? tiifeScrankeMax = null,
      int? tiifeScrankeMin = null,
      bool laseAusMengeChildUnterhalbTrefer = false)
    {
      if (suuceMengeWurzel == null)
        return (T[]) null;
      List<T[]> listeFeld = new List<T[]>();
      foreach (T suuceWurzel in suuceMengeWurzel)
        listeFeld.Add(suuceWurzel.SuuceFlacMengeAst<T>(prädikaat, funcAusAstMengeReferenziirteAst, listeFundAnzaalScrankeMax, tiifeScrankeMax, tiifeScrankeMin, laseAusMengeChildUnterhalbTrefer));
      return Glob.ArrayAusListeFeldGeflact<T>((IEnumerable<T[]>) listeFeld);
    }

    public static T[] SuuceFlacMengeAst<T>(
      this T suuceWurzel,
      Func<T, bool> prädikaat,
      Func<T, IEnumerable<T>> funcAusAstMengeReferenziirteAst,
      int? listeFundAnzaalScrankeMax = null,
      int? tiifeScrankeMax = null,
      int? tiifeScrankeMin = null,
      bool laseAusMengeChildUnterhalbTrefer = false)
    {
      T[][] source = Glob.SuuceFlacMengeAstMitPfaad<T>(suuceWurzel, prädikaat, funcAusAstMengeReferenziirteAst, listeFundAnzaalScrankeMax, tiifeScrankeMax, tiifeScrankeMin, laseAusMengeChildUnterhalbTrefer);
      return source == null ? (T[]) null : ((IEnumerable<T[]>) source).Select<T[], T>((Func<T[], T>) (astPfaad => astPfaad != null ? ((IEnumerable<T>) astPfaad).LastOrDefault<T>() : default (T))).ToArray<T>();
    }

    public static T[][] SuuceFlacMengeAstMitPfaad<T>(
      T root,
      Func<T, bool> matchPredicate,
      Func<T, IEnumerable<T>> funcAusAstMengeReferenziirteAst,
      int? listeFundAnzaalScrankeMax = null,
      int? tiifeScrankeMax = null,
      int? tiifeScrankeMin = null,
      bool laseAusMengeChildUnterhalbTrefer = false)
    {
      if (matchPredicate == null)
        return (T[][]) null;
      int? nullable = tiifeScrankeMax;
      int num1 = 0;
      if (nullable.GetValueOrDefault() <= num1 && nullable.HasValue)
        return (T[][]) null;
      List<T[]> objArrayList1 = new List<T[]>();
      T[][] objArray1 = new T[1][]{ new T[1]{ root } };
      int num2 = 0;
      while (true)
      {
        nullable = tiifeScrankeMax;
        int num3 = num2;
        if ((nullable.GetValueOrDefault() < num3 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
        {
          nullable = listeFundAnzaalScrankeMax;
          int count1 = objArrayList1.Count;
          if ((nullable.GetValueOrDefault() > count1 || !nullable.HasValue) && objArray1 != null && objArray1.Length >= 1)
          {
            List<T[]> objArrayList2 = new List<T[]>();
            for (int index = 0; index < objArray1.Length; ++index)
            {
              nullable = listeFundAnzaalScrankeMax;
              int count2 = objArrayList1.Count;
              if (nullable.GetValueOrDefault() > count2 || !nullable.HasValue)
              {
                T[] objArray2 = objArray1[index];
                T obj1 = ((IEnumerable<T>) objArray2).Last<T>();
                if (matchPredicate(obj1))
                {
                  int num4 = num2;
                  nullable = tiifeScrankeMin;
                  int valueOrDefault = nullable.GetValueOrDefault();
                  if ((num4 < valueOrDefault ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                  {
                    objArrayList1.Add(objArray2);
                    if (laseAusMengeChildUnterhalbTrefer)
                      continue;
                  }
                }
                if (funcAusAstMengeReferenziirteAst != null)
                {
                  IEnumerable<T> objs = funcAusAstMengeReferenziirteAst(obj1);
                  if (objs != null)
                  {
                    foreach (T obj2 in objs)
                    {
                      if ((object) obj2 != null)
                        objArrayList2.Add(((IEnumerable<T>) objArray2).Concat<T>((IEnumerable<T>) new T[1]
                        {
                          obj2
                        }).ToArray<T>());
                    }
                  }
                }
              }
              else
                break;
            }
            objArray1 = objArrayList2.ToArray();
            ++num2;
          }
          else
            break;
        }
        else
          break;
      }
      return objArrayList1.ToArray();
    }

    public static DateTime ZaitpunktNul => new DateTime(2000, 1, 1);

    public static Decimal StopwatchZaitMikroSictDecimal() => (Decimal) Stopwatch.GetTimestamp() * 1000000M / (Decimal) Stopwatch.Frequency;

    public static long StopwatchZaitMikroSictInt() => Stopwatch.GetTimestamp() * 100000L / Stopwatch.Frequency * 10L;

    public static long StopwatchZaitMiliSictInt() => Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;

    public static DateTime SictDateTimeVonStopwatchZaitMikro(long stopwatchZaitMikro) => Glob.SictDateTimeVonStopwatchZaitMikro((Decimal) stopwatchZaitMikro);

    public static DateTime SictDateTimeVonStopwatchZaitMikro(Decimal stopwatchZaitMikro) => DateTime.Now - TimeSpan.FromSeconds((double) (((Decimal) Glob.StopwatchZaitMikroSictInt() - stopwatchZaitMikro) * 0.000001M));

    public static DateTime SictDateTimeVonStopwatchZaitMili(Decimal stopwatchZaitMili) => Glob.SictDateTimeVonStopwatchZaitMikro(stopwatchZaitMili * 1000M);

    public static Decimal StopwatchZaitVonDateTimeMikroSictDecimal(DateTime zaitpunktSictDateTime) => Glob.StopwatchZaitMikroSictDecimal() - (Decimal) (DateTime.Now - zaitpunktSictDateTime).TotalSeconds * 1000000M;

    public static long StopwatchZaitVonDateTimeMikroSictInt(DateTime zaitpunktSictDateTime) => (long) Glob.StopwatchZaitVonDateTimeMikroSictDecimal(zaitpunktSictDateTime);

    public static XObject[] SictwaiseIPAddress(object abbildObject, out IPAddress abbildIPAddress) => Glob.SictwaiseIPAddress(abbildObject, out abbildIPAddress, out Exception _);

    public static XObject[] SictwaiseIPAddress(
      object abbildObject,
      out IPAddress abbildIPAddress,
      out Exception argumentAusnaame)
    {
      abbildIPAddress = (IPAddress) null;
      argumentAusnaame = (Exception) null;
      List<XObject> xobjectList = new List<XObject>();
      try
      {
        xobjectList.Add((XObject) new XAttribute((XName) "AbbildObject.Existent", (object) (abbildObject != null)));
        if (abbildObject != null)
        {
          xobjectList.Add((XObject) new XAttribute((XName) "AbbildObject.Type.FullName", (object) abbildObject.GetType().FullName));
          abbildIPAddress = abbildObject as IPAddress;
          if (abbildIPAddress == null && abbildObject is string ipString)
            abbildIPAddress = IPAddress.Parse(ipString);
        }
      }
      catch (Exception ex)
      {
        argumentAusnaame = ex;
        xobjectList.Add((XObject) Glob.SictwaiseXElement(ex));
      }
      finally
      {
        xobjectList.Add((XObject) new XAttribute((XName) "AbbildIPAddress", abbildIPAddress == null ? (object) "null" : (object) abbildIPAddress.ToString()));
      }
      return xobjectList.ToArray();
    }

    public static XObject[] SictwaiseInt32(object sictObjectAbbild, out int? sictInt32Abbild) => Glob.SictwaiseInt32(sictObjectAbbild, out sictInt32Abbild, out Exception _);

    public static XObject[] SictwaiseInt32(
      object sictObjectAbbild,
      out int? sictInt32Abbild,
      out Exception argumentAusnaame)
    {
      sictInt32Abbild = new int?();
      argumentAusnaame = (Exception) null;
      List<XObject> xobjectList = new List<XObject>();
      try
      {
        xobjectList.Add((XObject) new XAttribute((XName) "AbbildObject.Existent", (object) (sictObjectAbbild != null)));
        xobjectList.Add((XObject) new XAttribute((XName) "AbbildObject.Type.FullName", (object) Glob.TypeFullNameSictString(sictObjectAbbild)));
        if (sictObjectAbbild != null)
        {
          if (!(sictObjectAbbild is string zuParsende))
          {
            if (typeof (int).IsAssignableFrom(sictObjectAbbild.GetType()))
            {
              try
              {
                sictInt32Abbild = new int?((int) sictObjectAbbild);
              }
              catch
              {
              }
            }
            else if (typeof (double).IsAssignableFrom(sictObjectAbbild.GetType()))
            {
              try
              {
                sictInt32Abbild = new int?((int) (double) sictObjectAbbild);
              }
              catch
              {
              }
            }
          }
          else
            sictInt32Abbild = zuParsende.TryParseInt(Glob.NumberFormat);
        }
      }
      catch (Exception ex)
      {
        argumentAusnaame = ex;
        xobjectList.Add((XObject) Glob.SictwaiseXElement(ex));
      }
      finally
      {
        xobjectList.Add((XObject) new XAttribute((XName) "AbbildInt32", !sictInt32Abbild.HasValue ? (object) "null" : (object) sictInt32Abbild.Value.ToString()));
      }
      return xobjectList.ToArray();
    }

    public static string TypeFullNameSictString(object objekt) => objekt == null ? "null" : objekt.GetType().FullName;

    public static double SictwaiseSekundeZaal(this DateTime zaitpunkt) => (zaitpunkt - Glob.ZaitpunktNul).TotalSeconds;

    public static long SictwaiseMikrosekundeZaal(this DateTime zaitpunkt) => (long) ((zaitpunkt - Glob.ZaitpunktNul).TotalSeconds * 1000000.0);

    public static string SictwaiseKalenderString(
      this DateTime zaitpunkt,
      string trenzaice,
      int sekundeNaackomasctele = 4)
    {
      string str = zaitpunkt.Millisecond.ToString("D3").Substring(0, Math.Max(0, Math.Min(3, sekundeNaackomasctele)));
      return zaitpunkt.Year.ToString("D4") + trenzaice + (zaitpunkt.Month - 1).ToString("D2") + trenzaice + (zaitpunkt.Day - 1).ToString("D2") + trenzaice + zaitpunkt.Hour.ToString("D2") + trenzaice + zaitpunkt.Minute.ToString("D2") + trenzaice + zaitpunkt.Second.ToString("D2") + (0 < str.Length ? trenzaice + str : "");
    }

    public static string SictwaiseMikrosekundeString(this DateTime zaitpunkt) => zaitpunkt.SictwaiseMikrosekundeZaal().ToString();

    public static long SictUmgebrocen(
      this int wertUmzubrece,
      long regioonUmbrucBeginInklusiv,
      long regioonUmbrucEndeExklusiv)
    {
      return ((long) wertUmzubrece).SictUmgebrocen(regioonUmbrucBeginInklusiv, regioonUmbrucEndeExklusiv);
    }

    public static long SictUmgebrocen(
      this long wertUmzubrece,
      long regioonUmbrucBeginInklusiv,
      long regioonUmbrucEndeExklusiv)
    {
      if (regioonUmbrucEndeExklusiv < regioonUmbrucBeginInklusiv)
      {
        long num = regioonUmbrucEndeExklusiv;
        regioonUmbrucEndeExklusiv = regioonUmbrucBeginInklusiv;
        regioonUmbrucBeginInklusiv = num;
      }
      long actualValue = regioonUmbrucEndeExklusiv - regioonUmbrucBeginInklusiv;
      if (actualValue < 1L)
        throw new ArgumentOutOfRangeException("RegioonUmbrucGrööse", (object) actualValue, "");
      return ((wertUmzubrece - regioonUmbrucBeginInklusiv) % actualValue + actualValue) % actualValue + regioonUmbrucBeginInklusiv;
    }

    public static double SictUmgebrocen(
      this double wertUmzubrece,
      double regioonUmbrucBeginInklusiv,
      double regioonUmbrucEndeExklusiv)
    {
      if (regioonUmbrucEndeExklusiv < regioonUmbrucBeginInklusiv)
      {
        double num = regioonUmbrucEndeExklusiv;
        regioonUmbrucEndeExklusiv = regioonUmbrucBeginInklusiv;
        regioonUmbrucBeginInklusiv = num;
      }
      double actualValue = regioonUmbrucEndeExklusiv - regioonUmbrucBeginInklusiv;
      if (actualValue < 1.0)
        throw new ArgumentOutOfRangeException("RegioonUmbrucGrööse", (object) actualValue, "");
      return ((wertUmzubrece - regioonUmbrucBeginInklusiv) % actualValue + actualValue) % actualValue + regioonUmbrucBeginInklusiv;
    }

    public static string SictZaalSictStringBaasis16(
      this IEnumerable<byte> listeOktet,
      string oktetTrenung = null,
      string tetraadeTrenung = null)
    {
      if (listeOktet == null)
        return (string) null;
      List<string> list = listeOktet.Select<byte, string>((Func<byte, string>) (oktet => string.Format("{0:X1}", (object) ((int) oktet >> 4)) + tetraadeTrenung + string.Format("{0:X1}", (object) ((int) oktet & 15)))).ToList<string>();
      return list.Count < 1 ? "" : list.First<string>() + list.Skip<string>(1).Aggregate<string, string>("", (Func<string, string, string>) ((a, b) => a + oktetTrenung + b)) + oktetTrenung;
    }

    public static void Tausce<T>(ref T a, ref T b)
    {
      T obj = a;
      a = b;
      b = obj;
    }

    public static T[] ArrayAusListeListeGeflact<T>(IEnumerable<IEnumerable<T>> listeListe) => Glob.ArrayAusListeFeldGeflact<T>(listeListe.Select<IEnumerable<T>, T[]>((Func<IEnumerable<T>, T[]>) (liste => liste.ToArray<T>())));

    public static T[] ArrayAusListeFeldGeflact<T>(IEnumerable<T[]> listeFeld)
    {
      T[][] array = listeFeld.ToArray<T[]>();
      int length = 0;
      for (int index = 0; index < array.Length; ++index)
        length += array[index].Length;
      T[] objArray1 = new T[length];
      int num = 0;
      foreach (T[] objArray2 in array)
      {
        foreach (T obj in objArray2)
          objArray1[num++] = obj;
      }
      return objArray1;
    }

    public static FileInfo[] MengeDataiAusVerzaicnis(
      DirectoryInfo verzaicnis,
      bool berüksictigeUnterverzaicnis)
    {
      if (verzaicnis == null)
        return (FileInfo[]) null;
      List<FileInfo[]> listeListe = new List<FileInfo[]>();
      listeListe.Add(verzaicnis.GetFiles());
      if (berüksictigeUnterverzaicnis)
      {
        foreach (DirectoryInfo directory in verzaicnis.GetDirectories())
          listeListe.Add(Glob.MengeDataiAusVerzaicnis(directory, berüksictigeUnterverzaicnis));
      }
      return Glob.ArrayAusListeListeGeflact<FileInfo>((IEnumerable<IEnumerable<FileInfo>>) listeListe);
    }

    public static IEnumerable<FileInfo> MengeDataiGefiltertNaacNaameRegex(
      IEnumerable<FileInfo> vorFilterMengeDatai,
      string dataiNaameRegexPattern,
      RegexOptions dataiNaameRegexOptions)
    {
      if (vorFilterMengeDatai == null)
        return (IEnumerable<FileInfo>) null;
      List<FileInfo> fileInfoList = new List<FileInfo>();
      if (dataiNaameRegexPattern == null)
        return (IEnumerable<FileInfo>) fileInfoList;
      foreach (FileInfo fileInfo in vorFilterMengeDatai)
      {
        if (fileInfo != null && Regex.Match(fileInfo.Name, dataiNaameRegexPattern, dataiNaameRegexOptions).Success)
          fileInfoList.Add(fileInfo);
      }
      return (IEnumerable<FileInfo>) fileInfoList;
    }

    public static void AusVerzaicnisEntferneMengeDataiSolangeÜüberKapazitäätScranke(
      string verzaicnisPfaad,
      long kapazitäätScranke,
      bool berüksictigeUnterverzaicnis,
      out bool erfolg,
      out Exception exception,
      string filterDataiNaameRegexPattern = null,
      RegexOptions filterDataiNaameRegexOptions = RegexOptions.None)
    {
      exception = (Exception) null;
      List<XObject> xobjectList = new List<XObject>();
      bool flag1;
      try
      {
        long num1 = Glob.StopwatchZaitMikroSictInt();
        FileInfo[] vorFilterMengeDatai = verzaicnisPfaad != null ? Glob.MengeDataiAusVerzaicnis(new DirectoryInfo(verzaicnisPfaad), berüksictigeUnterverzaicnis) : throw new ArgumentNullException("VerzaicnisPfaad");
        List<FileInfo> list = (filterDataiNaameRegexPattern == null ? (IEnumerable<FileInfo>) vorFilterMengeDatai : Glob.MengeDataiGefiltertNaacNaameRegex((IEnumerable<FileInfo>) vorFilterMengeDatai, filterDataiNaameRegexPattern, filterDataiNaameRegexOptions)).OrderByDescending<FileInfo, DateTime>((Func<FileInfo, DateTime>) (datai => datai.CreationTime)).ToList<FileInfo>();
        long DataiLängeBisher = 0;
        FileInfo[] array = list.SkipWhile<FileInfo>((Func<FileInfo, bool>) (datai => (DataiLängeBisher += datai.Length) < kapazitäätScranke)).ToArray<FileInfo>();
        bool flag2 = true;
        try
        {
          foreach (FileInfo fileInfo in array)
          {
            bool flag3 = false;
            try
            {
              fileInfo.Delete();
              flag3 = true;
            }
            catch
            {
              flag3 = false;
            }
            finally
            {
              if (!flag3)
                flag2 = false;
            }
          }
        }
        finally
        {
        }
        long num2 = Glob.StopwatchZaitMikroSictInt() - num1;
        flag1 = flag2;
      }
      catch (Exception ex)
      {
        exception = ex;
        flag1 = false;
      }
      erfolg = flag1;
    }

    public static KeyValuePair<long, bool> KopiireVonStreamNaacStream(
      Stream kweleStream,
      Stream ziilStream,
      long zuKopiirendeListeOktetAnzaalScrankeMaximum,
      int puferOktetAnzaal = 4096)
    {
      byte[] buffer = new byte[puferOktetAnzaal];
      long key = 0;
      int count1;
      int count2;
      do
      {
        count1 = (int) Math.Min(zuKopiirendeListeOktetAnzaalScrankeMaximum - key, (long) buffer.Length);
        if (count1 >= 1)
        {
          count2 = kweleStream.Read(buffer, 0, count1);
          ziilStream.Write(buffer, 0, count2);
          key += (long) count2;
        }
        else
          goto label_1;
      }
      while (count2 >= count1);
      goto label_3;
label_1:
      return new KeyValuePair<long, bool>(key, false);
label_3:
      return new KeyValuePair<long, bool>(key, true);
    }

    public static XElement SictwaiseXElement(Exception ausnaame) => new XElement((XName) "Ausnaame", (object) Glob.SictwaiseXml(ausnaame));

    public static IEnumerable<XObject> SictwaiseXml(Exception ausnaame, bool laseAusStackTrace = false)
    {
      if (ausnaame == null)
        throw new ArgumentNullException("Ausnaame");
      List<XObject> xobjectList = new List<XObject>();
      xobjectList.Add((XObject) new XAttribute((XName) "TypFullName", (object) ausnaame.GetType().FullName));
      string message = ausnaame.Message;
      if (message != null)
        xobjectList.Add((XObject) new XElement((XName) "Message", (object) message));
      if (!laseAusStackTrace)
      {
        string stackTrace = ausnaame.StackTrace;
        if (stackTrace != null)
        {
          string SymbolFrameSeperation = "\n";
          xobjectList.Add((XObject) new XElement((XName) "StackTrace", (object) ((IEnumerable<string>) stackTrace.Split(new string[1]
          {
            SymbolFrameSeperation
          }, StringSplitOptions.RemoveEmptyEntries)).Select(frame => new
          {
            frame = frame,
            prettierFrame = frame.Replace(SymbolFrameSeperation, "").Trim()
          }).Select(_param1 => new XElement((XName) "Frame", (object) _param1.prettierFrame))));
        }
      }
      IDictionary data = ausnaame.Data;
      if (data != null && data.Count > 0)
        xobjectList.Add((XObject) new XElement((XName) "Data", (object) data.Cast<DictionaryEntry>().Select(entry => new
        {
          entry = entry,
          key = entry.Key.ToString()
        }).Select(_param1 => new
        {
          __TransparentIdentifier0 = _param1,
          value = _param1.entry.Value == null ? "null" : _param1.entry.Value.ToString()
        }).Select(_param1 => new XElement((XName) _param1.__TransparentIdentifier0.key, (object) _param1.value))));
      Exception innerException = ausnaame.InnerException;
      if (innerException != null)
        xobjectList.Add((XObject) new XElement((XName) "InnerException", (object) Glob.SictwaiseXml(innerException, laseAusStackTrace)));
      return (IEnumerable<XObject>) xobjectList;
    }

    public static byte[] InhaltAusDataiMitPfaad(string dataiPfaad)
    {
      FileStream fileStream = new FileInfo(dataiPfaad).OpenRead();
      try
      {
        byte[] buffer = new byte[fileStream.Length];
        fileStream.Read(buffer, 0, buffer.Length);
        return buffer;
      }
      finally
      {
        fileStream.Close();
      }
    }

    public static XElement SezeNaame(XElement xElement, string naame)
    {
      xElement.Name = (XName) naame;
      return xElement;
    }

    private static string EscapeAnfüürungszaicen(string @string) => @string.Replace("\\", "\\\\").Replace("\"", "\\\"");

    public static string SictString(this Exception exception, bool stackTraceLaseAus = false)
    {
      if (exception == null)
        return (string) null;
      ReflectionTypeLoadException typeLoadException = exception as ReflectionTypeLoadException;
      string str1 = Glob.EscapeAnfüürungszaicen(Glob.TypeFullNameSictString((object) exception));
      string str2 = "null";
      if (exception.InnerException != null)
        str2 = exception.InnerException.SictString(stackTraceLaseAus);
      string str3 = "null";
      if (exception.TargetSite != (MethodBase) null)
        str3 = Glob.EscapeAnfüürungszaicen(exception.TargetSite.Name);
      string str4 = exception.StackTrace == null ? "null" : Glob.EscapeAnfüürungszaicen(exception.StackTrace);
      string str5 = exception.Message == null ? "null" : Glob.EscapeAnfüürungszaicen(exception.Message);
      List<KeyValuePair<string, string>> keyValuePairList = new List<KeyValuePair<string, string>>();
      keyValuePairList.Add(new KeyValuePair<string, string>("Type", "\"" + str1 + "\""));
      keyValuePairList.Add(new KeyValuePair<string, string>("Message", "\"" + str5 + "\""));
      keyValuePairList.Add(new KeyValuePair<string, string>("InnerException", str2));
      if (typeLoadException != null)
      {
        Exception[] loaderExceptions = typeLoadException.LoaderExceptions;
        IEnumerable<string> source = loaderExceptions != null ? ((IEnumerable<Exception>) loaderExceptions).Select<Exception, string>((Func<Exception, string>) (loaderException => loaderException.SictString(stackTraceLaseAus))) : (IEnumerable<string>) null;
        string str6 = source != null ? source.Aggregate<string, string>("", (Func<string, string, string>) ((a, b) => a + "," + b)) : (string) null;
        keyValuePairList.Add(new KeyValuePair<string, string>("LoaderExceptions", str6));
      }
      if (!stackTraceLaseAus)
        keyValuePairList.Add(new KeyValuePair<string, string>("StackTrace", "\"" + str4 + "\""));
      keyValuePairList.Add(new KeyValuePair<string, string>("TargetSite", "\"" + str3 + "\""));
      string str7 = "{";
      for (int index = 0; index < keyValuePairList.Count; ++index)
      {
        KeyValuePair<string, string> keyValuePair = keyValuePairList[index];
        if (0 < index)
          str7 += ",";
        str7 = str7 + keyValuePair.Key + ":" + keyValuePair.Value;
      }
      return str7 + "}";
    }

    [Obsolete("Synchronization.LockExtension.InvokeWhenNotLocked")]
    public static void KapseleFunktioonsaufruufInTryEnter(
      Action funktioonsaufruuf,
      object zuLockendeObjekt,
      double timeout = 0.0)
    {
      funktioonsaufruuf.InvokeWhenNotLocked(zuLockendeObjekt, (int) (timeout * 1000.0));
    }

    public static NumberFormatInfo NumberFormat => new NumberFormatInfo()
    {
      NegativeSign = "-",
      PositiveSign = "+",
      NumberDecimalSeparator = "."
    };

    public static void KopiireVonStreamNaacStreamAbStreamBegin(Stream source, Stream dest)
    {
      source.Seek(0L, SeekOrigin.Begin);
      byte[] buffer = new byte[44444];
      while (true)
      {
        int count = source.Read(buffer, 0, buffer.Length);
        if (count > 0)
          dest.Write(buffer, 0, count);
        else
          break;
      }
    }

    public static Stream ErscteleKopiiVonStream(Stream source)
    {
      source.Seek(0L, SeekOrigin.Begin);
      MemoryStream dest = new MemoryStream();
      Glob.KopiireVonStreamNaacStreamAbStreamBegin(source, (Stream) dest);
      dest.Seek(0L, SeekOrigin.Begin);
      return (Stream) dest;
    }

    public static T[] SezeTailInArray<T>(
      T[] array,
      IEnumerable<T> zuSezendeTailListeElement,
      int zuSezendeTailBeginIndex,
      bool baiÜberlaufErwaitereArray)
    {
      if (zuSezendeTailListeElement == null)
        return array;
      if (zuSezendeTailBeginIndex < 0)
        return Glob.SezeTailInArray<T>(array, zuSezendeTailListeElement.Skip<T>(-zuSezendeTailBeginIndex), 0, baiÜberlaufErwaitereArray);
      T[] array1 = zuSezendeTailListeElement.ToArray<T>();
      for (int index1 = 0; index1 < array1.Length; ++index1)
      {
        int index2 = index1 + zuSezendeTailBeginIndex;
        if (array.Length > index2)
          array[index2] = array1[index1];
        else
          break;
      }
      int count = Math.Min(array1.Length, array.Length) - zuSezendeTailBeginIndex;
      return 0 < array1.Length - count && baiÜberlaufErwaitereArray ? ((IEnumerable<T>) array).Concat<T>(((IEnumerable<T>) array1).Skip<T>(count)).ToArray<T>() : array;
    }

    public static DateTime SictDateTimeVonMikrosekunde(long mikrosekunde) => Glob.ZaitpunktNul + TimeSpan.FromSeconds((double) mikrosekunde * 1E-06);

    public static DateTime SictDateTimeVonSekunde(long sekunde) => Glob.SictDateTimeVonMikrosekunde((long) ((double) sekunde * 1000000.0));

    public static long InFolgeFrüühesteVorkomeLaage<FolgeElementTyp>(
      FolgeElementTyp[] folgeZuDurcsuuce,
      FolgeElementTyp[] folgeZuFinde,
      long suuceBeginElementIndex = 0)
    {
      if (folgeZuDurcsuuce == null || folgeZuFinde == null || folgeZuFinde.Length < 1)
        return -1;
      long num = -1;
      for (long index1 = suuceBeginElementIndex; index1 < (long) folgeZuDurcsuuce.Length; ++index1)
      {
        FolgeElementTyp folgeElementTyp = folgeZuDurcsuuce[index1];
        if (0L <= num)
        {
          long index2 = index1 - num;
          if (folgeElementTyp.Equals((object) folgeZuFinde[index2]))
          {
            if ((long) (folgeZuFinde.Length - 1) <= index2)
              return num;
          }
          else
            num = -1L;
        }
        else if (folgeElementTyp.Equals((object) folgeZuFinde[0]))
          num = index1;
      }
      return -1;
    }

    public static void ScraibeInhaltNaacDataiPfaad(string dataiPfaad, byte[] dataiInhaltSol) => Glob.ScraibeInhaltNaacDataiPfaad(dataiPfaad, dataiInhaltSol, out bool _, out Exception _);

    public static void ScraibeInhaltNaacDataiPfaad(
      string dataiPfaad,
      byte[] dataiInhaltSol,
      out bool erfolg,
      out Exception ergeebnisAusnaame)
    {
      Glob.ScraibeInhaltNaacDataiPfaad(dataiPfaad, dataiInhaltSol, out erfolg, out ergeebnisAusnaame, out long _);
    }

    public static void ScraibeInhaltNaacDataiPfaad(
      string dataiPfaad,
      byte[] dataiInhaltSol,
      out bool erfolg,
      out Exception ergeebnisAusnaame,
      out long dauerMikro)
    {
      erfolg = false;
      ergeebnisAusnaame = (Exception) null;
      dauerMikro = -1L;
      long num1 = Glob.StopwatchZaitMikroSictInt();
      try
      {
        if (dataiInhaltSol == null)
          throw new ArgumentNullException("DataiInhaltSol");
        FileStream fileStream = new FileInfo(dataiPfaad).Create();
        try
        {
          fileStream.Write(dataiInhaltSol, 0, dataiInhaltSol.Length);
        }
        finally
        {
          fileStream.Close();
        }
        erfolg = true;
      }
      catch (Exception ex)
      {
        ergeebnisAusnaame = ex;
      }
      long num2 = Glob.StopwatchZaitMikroSictInt();
      dauerMikro = num2 - num1;
    }

    public static void LaadeInhaltAusDataiPfaad(string dataiPfaad, out byte[] dataiInhalt) => Glob.LaadeInhaltAusDataiPfaad(dataiPfaad, out dataiInhalt, out byte[] _);

    public static void LaadeInhaltAusDataiPfaad(
      string dataiPfaad,
      out byte[] dataiInhalt,
      out byte[] dataiInhaltHashSHA1)
    {
      Glob.LaadeInhaltAusDataiPfaad(dataiPfaad, out dataiInhalt, out dataiInhaltHashSHA1, out long _, out Exception _);
    }

    public static void LaadeInhaltAusDataiPfaad(
      string dataiPfaad,
      out byte[] dataiInhalt,
      out byte[] dataiInhaltHashSHA1,
      out long dauerMikro,
      out Exception ergeebnisException)
    {
      dataiInhalt = (byte[]) null;
      dataiInhaltHashSHA1 = (byte[]) null;
      ergeebnisException = (Exception) null;
      long num1 = Glob.StopwatchZaitMikroSictInt();
      List<XObject> xobjectList = new List<XObject>();
      try
      {
        FileStream fileStream = dataiPfaad != null ? new FileInfo(dataiPfaad).OpenRead() : throw new ArgumentNullException("DataiPfaad");
        try
        {
          byte[] buffer = new byte[fileStream.Length];
          fileStream.Read(buffer, 0, buffer.Length);
          SHA1Managed shA1Managed = new SHA1Managed();
          dataiInhaltHashSHA1 = shA1Managed.ComputeHash((Stream) new MemoryStream(buffer));
          dataiInhalt = buffer;
        }
        finally
        {
          fileStream.Close();
        }
      }
      catch (Exception ex)
      {
        ergeebnisException = ex;
      }
      long num2 = Glob.StopwatchZaitMikroSictInt();
      dauerMikro = num2 - num1;
    }
  }
}
