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

    public static long? Max(long? o0, long? o1) => Glob.Max((IEnumerable<long?>) new long?[2]
    {
      o0,
      o1
    });

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

    public static DateTime ZaitpunktNul => new DateTime(2000, 1, 1);

    public static long StopwatchZaitMikroSictInt() => Stopwatch.GetTimestamp() * 100000L / Stopwatch.Frequency * 10L;

    public static long StopwatchZaitMiliSictInt() => Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;

    public static DateTime SictDateTimeVonStopwatchZaitMikro(long stopwatchZaitMikro) => Glob.SictDateTimeVonStopwatchZaitMikro((Decimal) stopwatchZaitMikro);

    public static DateTime SictDateTimeVonStopwatchZaitMikro(Decimal stopwatchZaitMikro) => DateTime.Now - TimeSpan.FromSeconds((double) (((Decimal) Glob.StopwatchZaitMikroSictInt() - stopwatchZaitMikro) * 0.000001M));

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

    public static long SictwaiseMikrosekundeZaal(this DateTime zaitpunkt) => (long) ((zaitpunkt - Glob.ZaitpunktNul).TotalSeconds * 1000000.0);

    public static string SictwaiseKalenderString(
      this DateTime zaitpunkt,
      string trenzaice,
      int sekundeNaackomasctele = 4)
    {
      string str = zaitpunkt.Millisecond.ToString("D3").Substring(0, Math.Max(0, Math.Min(3, sekundeNaackomasctele)));
      return zaitpunkt.Year.ToString("D4") + trenzaice + (zaitpunkt.Month - 1).ToString("D2") + trenzaice + (zaitpunkt.Day - 1).ToString("D2") + trenzaice + zaitpunkt.Hour.ToString("D2") + trenzaice + zaitpunkt.Minute.ToString("D2") + trenzaice + zaitpunkt.Second.ToString("D2") + (0 < str.Length ? trenzaice + str : "");
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
    public static NumberFormatInfo NumberFormat => new NumberFormatInfo()
    {
      NegativeSign = "-",
      PositiveSign = "+",
      NumberDecimalSeparator = "."
    };


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
