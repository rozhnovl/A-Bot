// Decompiled with JetBrains decompiler
// Type: Bib3.SictVersazInDaatesctrukturAtribut
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bib3
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
  public class SictVersazInDaatesctrukturAtribut : Attribute
  {
    private readonly int SctrukturBezaicner;
    private readonly int Versaz;
    private readonly int? LängeScrankeMaximum;

    public SictVersazInDaatesctrukturAtribut(int versaz)
      : this(versaz, 0)
    {
    }

    public SictVersazInDaatesctrukturAtribut(int versaz, int sctrukturBezaicner)
      : this(versaz, sctrukturBezaicner, new int?())
    {
    }

    public SictVersazInDaatesctrukturAtribut(
      int versaz,
      int sctrukturBezaicner,
      int längeScrankeMaximum)
      : this(versaz, sctrukturBezaicner, new int?(längeScrankeMaximum))
    {
    }

    public SictVersazInDaatesctrukturAtribut(
      int versaz,
      int sctrukturBezaicner,
      int? längeScrankeMaximum)
    {
      this.SctrukturBezaicner = sctrukturBezaicner;
      this.Versaz = versaz;
      this.LängeScrankeMaximum = längeScrankeMaximum;
    }

    public static byte[] ListeBitInListeOktetVersezt(
      byte[] listeOktet,
      int versazBitAnzaal,
      bool bitWertSctandard)
    {
      if (listeOktet == null)
        return (byte[]) null;
      byte maxValue = bitWertSctandard ? byte.MaxValue : (byte) 0;
      int num1 = (int) Math.Floor((double) -versazBitAnzaal / 8.0);
      int length = -num1;
      int num2 = (versazBitAnzaal % 8 + 8) % 8;
      if (num2 == 0)
      {
        if (0 >= length)
          return ((IEnumerable<byte>) listeOktet).Skip<byte>(-length).ToArray<byte>();
        byte[] first = new byte[length];
        if (bitWertSctandard)
        {
          for (int index = 0; index < first.Length; ++index)
            first[index] = byte.MaxValue;
        }
        return ((IEnumerable<byte>) first).Concat<byte>((IEnumerable<byte>) listeOktet).ToArray<byte>();
      }
      List<byte> byteList = new List<byte>();
      int num3 = 0;
      while (true)
      {
        int count1 = num3 + num1;
        int count2 = count1 + 1;
        if (listeOktet.Length > count1)
        {
          byte num4 = count1 < 0 ? maxValue : ((IEnumerable<byte>) listeOktet).Skip<byte>(count1).DefaultIfEmpty<byte>(maxValue).First<byte>();
          byte num5 = count2 < 0 ? maxValue : ((IEnumerable<byte>) listeOktet).Skip<byte>(count2).DefaultIfEmpty<byte>(maxValue).First<byte>();
          byte num6 = (byte) ((int) num4 >> (8 - num2) % 8 | (int) num5 << (num2 + 7) % 8 + 1);
          byteList.Add(num6);
          ++num3;
        }
        else
          break;
      }
      return byteList.ToArray();
    }

    public static void GibMaskeFürObjektSictListeOktet(
      object objekt,
      bool littleEndian,
      out bool typUntersctüzt,
      out byte[] maskeKonjunkt,
      out byte[] maskeDisjunkt)
    {
      typUntersctüzt = false;
      maskeKonjunkt = (byte[]) null;
      maskeDisjunkt = (byte[]) null;
      if (objekt == null)
        return;
      objekt.GetType();
      bool? nullable1 = objekt as bool?;
      if (nullable1.HasValue)
      {
        maskeKonjunkt = new byte[1]{ (byte) 254 };
        maskeDisjunkt = new byte[1]
        {
          nullable1.Value ? (byte) 1 : (byte) 0
        };
      }
      byte? nullable2 = objekt as byte?;
      if (nullable2.HasValue)
      {
        maskeKonjunkt = new byte[1]{ nullable2.Value };
        maskeDisjunkt = maskeKonjunkt;
      }
      sbyte? nullable3 = objekt as sbyte?;
      if (nullable3.HasValue)
      {
        maskeKonjunkt = new byte[1]
        {
          (byte) nullable3.Value
        };
        maskeDisjunkt = maskeKonjunkt;
      }
      if (objekt is byte[] numArray)
      {
        maskeKonjunkt = numArray;
        maskeDisjunkt = maskeKonjunkt;
      }
      ulong? nullable4 = objekt as ulong?;
      if (nullable4.HasValue)
      {
        maskeKonjunkt = BitConverter.GetBytes(nullable4.Value);
        if (BitConverter.IsLittleEndian != littleEndian)
          maskeKonjunkt = ((IEnumerable<byte>) maskeKonjunkt).Reverse<byte>().ToArray<byte>();
        maskeDisjunkt = maskeKonjunkt;
      }
      long? nullable5 = objekt as long?;
      if (nullable5.HasValue)
      {
        maskeKonjunkt = BitConverter.GetBytes(nullable5.Value);
        if (BitConverter.IsLittleEndian != littleEndian)
          maskeKonjunkt = ((IEnumerable<byte>) maskeKonjunkt).Reverse<byte>().ToArray<byte>();
        maskeDisjunkt = maskeKonjunkt;
      }
      uint? nullable6 = objekt as uint?;
      if (nullable6.HasValue)
      {
        maskeKonjunkt = BitConverter.GetBytes(nullable6.Value);
        if (BitConverter.IsLittleEndian != littleEndian)
          maskeKonjunkt = ((IEnumerable<byte>) maskeKonjunkt).Reverse<byte>().ToArray<byte>();
        maskeDisjunkt = maskeKonjunkt;
      }
      int? nullable7 = objekt as int?;
      if (nullable7.HasValue)
      {
        maskeKonjunkt = BitConverter.GetBytes(nullable7.Value);
        if (BitConverter.IsLittleEndian != littleEndian)
          maskeKonjunkt = ((IEnumerable<byte>) maskeKonjunkt).Reverse<byte>().ToArray<byte>();
        maskeDisjunkt = maskeKonjunkt;
      }
      ushort? nullable8 = objekt as ushort?;
      if (nullable8.HasValue)
      {
        maskeKonjunkt = BitConverter.GetBytes(nullable8.Value);
        if (BitConverter.IsLittleEndian != littleEndian)
          maskeKonjunkt = ((IEnumerable<byte>) maskeKonjunkt).Reverse<byte>().ToArray<byte>();
        maskeDisjunkt = maskeKonjunkt;
      }
      short? nullable9 = objekt as short?;
      if (nullable9.HasValue)
      {
        maskeKonjunkt = BitConverter.GetBytes(nullable9.Value);
        if (BitConverter.IsLittleEndian != littleEndian)
          maskeKonjunkt = ((IEnumerable<byte>) maskeKonjunkt).Reverse<byte>().ToArray<byte>();
        maskeDisjunkt = maskeKonjunkt;
      }
      typUntersctüzt = maskeKonjunkt != null && maskeDisjunkt != null;
    }

    public static object GibObjektAusSictListeOktetAbbild(
      byte[] sictListeOktetAbbild,
      Type objektTyp,
      bool littleEndian,
      int? objektLängeScrankeMaximum)
    {
      if (sictListeOktetAbbild == null || objektTyp == (Type) null)
        return (object) null;
      if (objektTyp == typeof (byte[]))
      {
        if (!objektLängeScrankeMaximum.HasValue)
          throw new ArgumentNullException("ObjektLängeScrankeMaximum");
        int count = (objektLängeScrankeMaximum.Value + 7) / 8;
        byte[] array = ((IEnumerable<byte>) sictListeOktetAbbild).Take<byte>(count).ToArray<byte>();
        int num1 = (objektLängeScrankeMaximum.Value - 1) % 8 + 1;
        if (array.Length != 0)
        {
          byte num2 = SictVersazInDaatesctrukturAtribut.ListeBitInListeOktetVersezt(new byte[1]
          {
            byte.MaxValue
          }, num1 - 8, false)[0];
          byte num3 = (byte) ((uint) array[array.Length - 1] & (uint) num2);
          array[array.Length - 1] = num3;
        }
        return (object) array;
      }
      byte[][] array1 = Enumerable.Range(0, 9).Select<int, byte[]>((Func<int, byte[]>) (untermengeListeOktetAnzaal =>
      {
        byte[] array2 = ((IEnumerable<byte>) sictListeOktetAbbild).Take<byte>(objektLängeScrankeMaximum ?? 999999).Concat<byte>((IEnumerable<byte>) new byte[untermengeListeOktetAnzaal]).Take<byte>(untermengeListeOktetAnzaal).ToArray<byte>();
        return BitConverter.IsLittleEndian == littleEndian ? array2 : ((IEnumerable<byte>) array2).Reverse<byte>().ToArray<byte>();
      })).ToArray<byte[]>();
      byte num = array1[1][0];
      if (objektTyp == typeof (bool))
        return (object) (((int) num & 1) == 1);
      if (objektTyp == typeof (byte))
        return (object) num;
      if (objektTyp == typeof (ushort))
        return (object) BitConverter.ToUInt16(array1[2], 0);
      if (objektTyp == typeof (short))
        return (object) BitConverter.ToInt16(array1[2], 0);
      if (objektTyp == typeof (uint))
        return (object) BitConverter.ToUInt32(array1[4], 0);
      if (objektTyp == typeof (int))
        return (object) BitConverter.ToInt32(array1[4], 0);
      if (objektTyp == typeof (ulong))
        return (object) BitConverter.ToUInt64(array1[8], 0);
      if (objektTyp == typeof (long))
        return (object) BitConverter.ToInt64(array1[8], 0);
      throw new ArgumentException("Nict untersctüzte Typ \"" + objektTyp.Name + "\"");
    }

    public static void ScraibeNaacDaatesctruktuur(
      object daatesctruktuur,
      int sctrukturBezaicner,
      byte[] wertZuScraibeSictListeOktet,
      bool littleEndian)
    {
      if (daatesctruktuur == null)
        return;
      Type type = daatesctruktuur.GetType();
      long num1 = 0;
      long num2 = 0;
      long num3 = 0;
      long num4 = 0;
      foreach (FieldInfo field in type.GetFields())
      {
        try
        {
          SictMesungZaitraumAusStopwatch zaitraumAusStopwatch1 = new SictMesungZaitraumAusStopwatch(true);
          SictVersazInDaatesctrukturAtribut daatesctrukturAtribut = ((IEnumerable<SictVersazInDaatesctrukturAtribut>) field.GetCustomAttributes(typeof (SictVersazInDaatesctrukturAtribut), true).OfType<SictVersazInDaatesctrukturAtribut>().ToArray<SictVersazInDaatesctrukturAtribut>()).FirstOrDefault<SictVersazInDaatesctrukturAtribut>((Func<SictVersazInDaatesctrukturAtribut, bool>) (kandidaat => kandidaat.SctrukturBezaicner == sctrukturBezaicner));
          zaitraumAusStopwatch1.EndeSezeJezt();
          long num5 = num1;
          long? dauerMikro = zaitraumAusStopwatch1.DauerMikro;
          long num6 = dauerMikro ?? 0L;
          num1 = num5 + num6;
          if (daatesctrukturAtribut != null)
          {
            SictMesungZaitraumAusStopwatch zaitraumAusStopwatch2 = new SictMesungZaitraumAusStopwatch(true);
            int versaz = daatesctrukturAtribut.Versaz;
            byte[] sictListeOktetAbbild = SictVersazInDaatesctrukturAtribut.ListeBitInListeOktetVersezt(wertZuScraibeSictListeOktet, -versaz, false);
            zaitraumAusStopwatch2.EndeSezeJezt();
            SictMesungZaitraumAusStopwatch zaitraumAusStopwatch3 = new SictMesungZaitraumAusStopwatch(true);
            object obj = SictVersazInDaatesctrukturAtribut.GibObjektAusSictListeOktetAbbild(sictListeOktetAbbild, field.FieldType, littleEndian, daatesctrukturAtribut.LängeScrankeMaximum);
            zaitraumAusStopwatch3.EndeSezeJezt();
            SictMesungZaitraumAusStopwatch zaitraumAusStopwatch4 = new SictMesungZaitraumAusStopwatch(true);
            field.SetValue(daatesctruktuur, obj);
            zaitraumAusStopwatch4.EndeSezeJezt();
            long num7 = num2;
            dauerMikro = zaitraumAusStopwatch2.DauerMikro;
            long num8 = dauerMikro ?? 0L;
            num2 = num7 + num8;
            long num9 = num3;
            dauerMikro = zaitraumAusStopwatch3.DauerMikro;
            long num10 = dauerMikro ?? 0L;
            num3 = num9 + num10;
            long num11 = num4;
            dauerMikro = zaitraumAusStopwatch4.DauerMikro;
            long num12 = dauerMikro ?? 0L;
            num4 = num11 + num12;
          }
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Field[" + field.Name + "]", ex);
        }
      }
    }

    public static void ScraibeNaacDaatesctruktuur(
      object daatesctruktuur,
      int sctrukturBezaicner,
      uint wertZuScraibe,
      bool littleEndian)
    {
      byte[] numArray = BitConverter.GetBytes(wertZuScraibe);
      if (BitConverter.IsLittleEndian != littleEndian)
        numArray = ((IEnumerable<byte>) numArray).Reverse<byte>().ToArray<byte>();
      SictVersazInDaatesctrukturAtribut.ScraibeNaacDaatesctruktuur(daatesctruktuur, sctrukturBezaicner, numArray, littleEndian);
    }

    public static byte[] DaatesctruktuurSictListeOktet(
      object daatesctruktuur,
      int sctrukturBezaicner,
      bool littleEndian)
    {
      if (daatesctruktuur == null)
        return (byte[]) null;
      FieldInfo[] fields = daatesctruktuur.GetType().GetFields();
      List<KeyValuePair<byte[], byte[]>> keyValuePairList = new List<KeyValuePair<byte[], byte[]>>();
      foreach (FieldInfo fieldInfo in fields)
      {
        try
        {
          SictVersazInDaatesctrukturAtribut daatesctrukturAtribut = ((IEnumerable<SictVersazInDaatesctrukturAtribut>) fieldInfo.GetCustomAttributes(typeof (SictVersazInDaatesctrukturAtribut), true).OfType<SictVersazInDaatesctrukturAtribut>().ToArray<SictVersazInDaatesctrukturAtribut>()).FirstOrDefault<SictVersazInDaatesctrukturAtribut>((Func<SictVersazInDaatesctrukturAtribut, bool>) (kandidaat => kandidaat.SctrukturBezaicner == sctrukturBezaicner));
          if (daatesctrukturAtribut != null)
          {
            int versaz = daatesctrukturAtribut.Versaz;
            object objekt = fieldInfo.GetValue(daatesctruktuur);
            if (objekt != null)
            {
              bool typUntersctüzt;
              byte[] maskeKonjunkt;
              byte[] maskeDisjunkt;
              SictVersazInDaatesctrukturAtribut.GibMaskeFürObjektSictListeOktet(objekt, littleEndian, out typUntersctüzt, out maskeKonjunkt, out maskeDisjunkt);
              if (!typUntersctüzt)
                throw new ArgumentException("!TypUntersctüzt (" + Glob.TypeFullNameSictString(objekt) + ")");
              if (maskeKonjunkt == null)
                throw new ArgumentNullException("FieldWertSictListeBitMaskeKonjunkt");
              if (maskeDisjunkt == null)
                throw new ArgumentNullException("FieldWertSictListeBitMaskeDisjunkt");
              byte[] key = SictVersazInDaatesctrukturAtribut.ListeBitInListeOktetVersezt(maskeKonjunkt, versaz, true);
              byte[] numArray = SictVersazInDaatesctrukturAtribut.ListeBitInListeOktetVersezt(maskeDisjunkt, versaz, false);
              keyValuePairList.Add(new KeyValuePair<byte[], byte[]>(key, numArray));
            }
          }
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Field[" + fieldInfo.Name + "]", ex);
        }
      }
      List<byte> byteList = new List<byte>();
      foreach (KeyValuePair<byte[], byte[]> keyValuePair in keyValuePairList)
      {
        byte[] key = keyValuePair.Key;
        byte[] numArray = keyValuePair.Value;
        for (int index = 0; index < key.Length; ++index)
        {
          if (byteList.Count <= index)
            byteList.Add((byte) 0);
          else
            byteList[index] &= key[index];
        }
        for (int index = 0; index < numArray.Length; ++index)
        {
          if (byteList.Count <= index)
            byteList.Add(numArray[index]);
          else
            byteList[index] |= numArray[index];
        }
      }
      return byteList.ToArray();
    }

    public static uint DaatesctruktuurSictUInt32(
      object daatesctruktuur,
      int sctrukturBezaicner,
      bool littleEndian)
    {
      byte[] numArray = SictVersazInDaatesctrukturAtribut.DaatesctruktuurSictListeOktet(daatesctruktuur, sctrukturBezaicner, littleEndian);
      if (BitConverter.IsLittleEndian != littleEndian)
        numArray = ((IEnumerable<byte>) numArray).Reverse<byte>().ToArray<byte>();
      int length = 4 - numArray.Length;
      if (0 < length)
        numArray = ((IEnumerable<byte>) numArray).Concat<byte>((IEnumerable<byte>) new byte[length]).ToArray<byte>();
      return BitConverter.ToUInt32(numArray, 0);
    }
  }
}
