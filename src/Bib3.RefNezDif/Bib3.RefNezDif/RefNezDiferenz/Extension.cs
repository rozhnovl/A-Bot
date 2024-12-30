// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.Extension
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Bib3.RefBaumKopii;
using Fasterflect;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib3.RefNezDiferenz
{
  public static class Extension
  {
    private static readonly JsonSerializerSettings NewtonsoftJsonSerializerSettings = new JsonSerializerSettings()
    {
      DefaultValueHandling = (DefaultValueHandling) 3
    };

    public static SictZuNezSictDiferenzScritAbbild ListeWurzelSerialisiire(
      this IEnumerable<object> listeWurzel,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null)
    {
      return new SictRefNezDiferenz(new SictDiferenzSictParam(rictliinieMitScatescpaicer ?? SictRefNezKopii.SctandardRictlinieMitScatescpaicer)).BerecneScritDif(new long?(), listeWurzel != null ? listeWurzel.ToArray<object>() : (object[]) null);
    }

    public static SictZuNezSictDiferenzScritAbbild WurzelSerialisiire(
      this object wurzel,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null)
    {
      return ((IEnumerable<object>) new object[1]{ wurzel }).ListeWurzelSerialisiire(rictliinieMitScatescpaicer);
    }

    public static string ListeWurzelSerialisiireZuJson(
      this IEnumerable<object> listeWurzel,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null)
    {
      return JsonConvert.SerializeObject((object) listeWurzel.ListeWurzelSerialisiire(rictliinieMitScatescpaicer), Extension.NewtonsoftJsonSerializerSettings);
    }

    public static string WurzelSerialisiireZuJson(
      this object wurzel,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null)
    {
      return ((IEnumerable<object>) new object[1]{ wurzel }).ListeWurzelSerialisiireZuJson(rictliinieMitScatescpaicer);
    }

    public static object[] ListeWurzelDeserialisiire(
      this SictZuNezSictDiferenzScritAbbild seriel,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null)
    {
      if (seriel == null)
        return (object[]) null;
      SictRefNezScritSumeErgeebnis scritSumeErgeebnis = new SictRefNezSume(new SictDiferenzSictParam(rictliinieMitScatescpaicer ?? SictRefNezKopii.SctandardRictlinieMitScatescpaicer)).BerecneScritSumeListeWurzelRefClrUndErfolg(seriel);
      return scritSumeErgeebnis == null || !scritSumeErgeebnis.Volsctändig ? (object[]) null : scritSumeErgeebnis.ListeWurzelRefClr;
    }

    public static object[] ListeWurzelDeserialisiireVonJson(
      this string json,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null)
    {
      if (json == null)
        return (object[]) null;
      SictZuNezSictDiferenzScritAbbild seriel = JsonConvert.DeserializeObject<SictZuNezSictDiferenzScritAbbild>(json);
      return seriel == null ? (object[]) null : seriel.ListeWurzelDeserialisiire(rictliinieMitScatescpaicer);
    }

    public static IEnumerable<object> EnumRefClrVonObjekt(
      this object objekt,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieScatescpaicer)
    {
      List<KeyValuePair<object, object>> keyValuePairList = new List<KeyValuePair<object, object>>();
      SictMeldungValueOderRef.AusReferenziirteMengeReferenzClrVerwendetFüügeAinNaacList(rictliinieScatescpaicer, objekt, (IList<KeyValuePair<object, object>>) keyValuePairList);
      return keyValuePairList.Select<KeyValuePair<object, object>, object>((Func<KeyValuePair<object, object>, object>) (t => t.Value));
    }

    public static IEnumerable<object> EnumerateReferenced(
      this object root,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer mengeTypeBehandlungRictliinieMitScatescpaicer)
    {
      List<KeyValuePair<object, object>> keyValuePairList = new List<KeyValuePair<object, object>>();
      SictMeldungValueOderRef.AusReferenziirteMengeReferenzClrVerwendetFüügeAinNaacList(mengeTypeBehandlungRictliinieMitScatescpaicer, root, (IList<KeyValuePair<object, object>>) keyValuePairList);
      return keyValuePairList.Values<object, object>();
    }

    public static IEnumerable<object> EnumMengeRefAusNezAusMengeWurzel(
      this object[] mengeWurzelRefClr,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer mengeTypeBehandlungRictliinieMitScatescpaicer)
    {
      if (mengeWurzelRefClr != null && mengeTypeBehandlungRictliinieMitScatescpaicer != null)
      {
        Dictionary<object, bool> DictObjektBeraitsBesuuct = new Dictionary<object, bool>();
        Queue<object> SclangeInstance = new Queue<object>();
        object[] objArray = mengeWurzelRefClr;
        for (int index = 0; index < objArray.Length; ++index)
        {
          object WurzelRefClr = objArray[index];
          if (WurzelRefClr != null)
          {
            SclangeInstance.Enqueue(WurzelRefClr);
            WurzelRefClr = (object) null;
          }
        }
        objArray = (object[]) null;
        List<KeyValuePair<object, object>> MengeObjektVerwendetReferenzClrInAst = new List<KeyValuePair<object, object>>();
        while (0 < SclangeInstance.Count)
        {
          object InstanceRefClr = SclangeInstance.Dequeue();
          if (!DictObjektBeraitsBesuuct.ContainsKey(InstanceRefClr))
          {
            DictObjektBeraitsBesuuct[InstanceRefClr] = true;
            yield return InstanceRefClr;
            SictMeldungValueOderRef.AusReferenziirteMengeReferenzClrVerwendetFüügeAinNaacList(mengeTypeBehandlungRictliinieMitScatescpaicer, InstanceRefClr, (IList<KeyValuePair<object, object>>) MengeObjektVerwendetReferenzClrInAst);
            foreach (KeyValuePair<object, object> keyValuePair in MengeObjektVerwendetReferenzClrInAst)
            {
              KeyValuePair<object, object> ObjektVerwendetReferenzClrInAst = keyValuePair;
              if (ObjektVerwendetReferenzClrInAst.Value != null)
              {
                SclangeInstance.Enqueue(ObjektVerwendetReferenzClrInAst.Value);
                ObjektVerwendetReferenzClrInAst = new KeyValuePair<object, object>();
              }
            }
            MengeObjektVerwendetReferenzClrInAst.Clear();
            InstanceRefClr = (object) null;
          }
        }
      }
    }

    public static IEnumerable<object> EnumMengeRefAusNezAusWurzel(
      this object wurzelRefClr,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer mengeTypeBehandlungRictliinieMitScatescpaicer)
    {
      if (wurzelRefClr == null)
        return (IEnumerable<object>) null;
      return new object[1]{ wurzelRefClr }.EnumMengeRefAusNezAusMengeWurzel(mengeTypeBehandlungRictliinieMitScatescpaicer);
    }

    public static Type GetCommonBaseClass(this Type[] types)
    {
      if (types == null || types.Length == 0)
        return (Type) null;
      if (types.Length == 1)
        return types[0];
      Type[] array = ((IEnumerable<Type>) types).ToArray<Type>();
      bool flag = false;
      Type commonBaseClass = (Type) null;
      while (!flag)
      {
        commonBaseClass = array[0];
        flag = true;
        for (int index1 = 1; index1 < array.Length; ++index1)
        {
          if (!commonBaseClass.Equals(array[index1]))
          {
            if (commonBaseClass.Equals(array[index1].BaseType))
            {
              array[index1] = array[index1].BaseType;
            }
            else
            {
              if (commonBaseClass.BaseType.Equals(array[index1]))
              {
                for (int index2 = 0; index2 <= index1 - 1; ++index2)
                  array[index2] = array[index2].BaseType;
                flag = false;
                break;
              }
              for (int index3 = 0; index3 <= index1; ++index3)
                array[index3] = array[index3].BaseType;
              flag = false;
              break;
            }
          }
        }
      }
      return commonBaseClass;
    }

    public static void InRefNezApliziireErsazWoUnglaicDefault(
      this object refNezZuErsezende,
      object refNezErsaz,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null,
      int? tiifeScrankeMax = null)
    {
      if (refNezZuErsezende == null || refNezErsaz == null)
        return;
      rictliinieMitScatescpaicer = rictliinieMitScatescpaicer ?? SictRefNezKopii.SctandardRictlinieMitScatescpaicer;
      int? nullable = tiifeScrankeMax;
      int num = 0;
      if (nullable.GetValueOrDefault() < num && nullable.HasValue)
        return;
      Type commonBaseClass = new Type[2]
      {
        refNezErsaz.GetType(),
        refNezZuErsezende.GetType()
      }.GetCommonBaseClass();
      SictZuTypeBehandlung zuTypeBehandlung = rictliinieMitScatescpaicer.ZuTypeBehandlung(commonBaseClass);
      if (zuTypeBehandlung == null)
        return;
      SictZuMemberBehandlung[] memberBehandlung1 = zuTypeBehandlung.MengeMemberBehandlung;
      if (memberBehandlung1 == null)
        return;
      foreach (SictZuMemberBehandlung memberBehandlung2 in memberBehandlung1)
      {
        Type herkunftMemberType = memberBehandlung2.HerkunftMemberType;
        MemberGetter typeMemberGetter = memberBehandlung2.HerkunftTypeMemberGetter;
        MemberSetter typeMemberSetter = memberBehandlung2.ZiilTypeMemberSetter;
        if (typeMemberGetter != null && typeMemberSetter != null)
        {
          object objA1 = typeMemberGetter.Invoke(refNezErsaz);
          object defaultValue = herkunftMemberType.GetDefaultValue();
          if (!object.Equals(objA1, defaultValue))
          {
            object objA2 = typeMemberGetter.Invoke(refNezZuErsezende);
            if (object.Equals(objA2, defaultValue) || !herkunftMemberType.IsClass)
            {
              typeMemberSetter.Invoke(refNezZuErsezende, objA1);
            }
            else
            {
              object refNezZuErsezende1 = objA2;
              object refNezErsaz1 = objA1;
              SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer1 = rictliinieMitScatescpaicer;
              nullable = tiifeScrankeMax;
              int? tiifeScrankeMax1 = nullable.HasValue ? new int?(nullable.GetValueOrDefault() - 1) : new int?();
              refNezZuErsezende1.InRefNezApliziireErsazWoUnglaicDefault(refNezErsaz1, rictliinieMitScatescpaicer1, tiifeScrankeMax1);
            }
          }
        }
      }
    }

    public static object ObjektKopiiKonstruktWeakTyped(
      this object zuKopiireObjekt,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null,
      int? tiifeScrankeMax = null)
    {
      Param obj = new Param((Profile) null, rictliinieMitScatescpaicer ?? SictRefNezKopii.SctandardRictlinieMitScatescpaicer);
      return SictRefNezKopii.ObjektKopiiErsctele(zuKopiireObjekt, obj, (Scatescpaicer) null, tiifeScrankeMax);
    }

    public static T ObjektKopiiKonstrukt<T>(
      this T zuKopiireObjekt,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null,
      int? tiifeScrankeMax = null)
    {
      Param obj = new Param((Profile) null, rictliinieMitScatescpaicer ?? SictRefNezKopii.SctandardRictlinieMitScatescpaicer);
      return SictRefNezKopii.ObjektKopiiErsctele<T>(zuKopiireObjekt, obj, tiifeScrankeMax);
    }

    public static T ObjektKopiiKonstruktÜberSerialisString<T>(
      this T zuKopiireObjekt,
      out string sictSeriel,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null)
    {
      rictliinieMitScatescpaicer = rictliinieMitScatescpaicer ?? SictRefNezKopii.SctandardRictlinieMitScatescpaicer;
      SictZuNezSictDiferenzScritAbbild diferenzScritAbbild = ((object) zuKopiireObjekt).WurzelSerialisiire(rictliinieMitScatescpaicer);
      sictSeriel = JsonConvert.SerializeObject((object) diferenzScritAbbild);
      object[] source = JsonConvert.DeserializeObject<SictZuNezSictDiferenzScritAbbild>(sictSeriel).ListeWurzelDeserialisiire(rictliinieMitScatescpaicer);
      return (T) (source != null ? ((IEnumerable<object>) source).FirstOrDefault<object>() : (object) null);
    }

    public static object GetDefaultValue(this Type type) => (Type) null == type || (Type) null != Nullable.GetUnderlyingType(type) || !type.IsValueType ? (object) null : Activator.CreateInstance(type, true);

    public static string SictSerielJsonConvert(
      object zuSerialisiirende,
      JsonSerializerSettings settings = null)
    {
      return zuSerialisiirende == null ? (string) null : JsonConvert.SerializeObject(zuSerialisiirende, settings);
    }

    public static T JsonConvertKopii<T>(T zuKopiirende, JsonSerializerSettings settings = null)
    {
      if ((object) zuKopiirende == null)
        return default (T);
      string str = Extension.SictSerielJsonConvert((object) zuKopiirende, settings);
      try
      {
        return JsonConvert.DeserializeObject<T>(str, settings);
      }
      catch
      {
        throw;
      }
    }

    public static bool EqualPerNewtonsoftJsonSerializer(
      object o0,
      object o1,
      out string o0SictSeriel,
      out string o1SictSeriel,
      JsonSerializerSettings serializerSettings = null)
    {
      o0SictSeriel = (string) null;
      o1SictSeriel = (string) null;
      if (o0 == o1)
        return true;
      if (o0 == null || o1 == null)
        return false;
      o0SictSeriel = JsonConvert.SerializeObject(o0, serializerSettings);
      o1SictSeriel = JsonConvert.SerializeObject(o1, serializerSettings);
      return string.Equals(o0SictSeriel, o1SictSeriel);
    }

    public static bool InheritsOrImplementsOrEquals(this Type type, Type kandidaatBase) => !((Type) null == type) && !((Type) null == kandidaatBase) && (TypeExtensions.InheritsOrImplements(type, kandidaatBase) || type.Equals(kandidaatBase));

    public static bool InheritsOrImplementsOrEquals<T>(this Type type) => type.InheritsOrImplementsOrEquals(typeof (T));

    public static SictMeldungValueOderRefSictSeriel SictSeriel(
      this SictMeldungValueOderRef meldungValueOderRef)
    {
      return meldungValueOderRef == null ? (SictMeldungValueOderRefSictSeriel) null : new SictMeldungValueOderRefSictSeriel(meldungValueOderRef.ValueSictJsonAbbild, SictMeldungValueAstReferenzSictSeriel.Konstrukt(meldungValueOderRef.ValueBaumReferenz), meldungValueOderRef.ReferenzTransport);
    }

    public static SictMeldungValueOderRef KonstruktStructClr(
      this SictMeldungValueOderRefSictSeriel meldungValueOderRefSictJson)
    {
      return meldungValueOderRefSictJson == null ? (SictMeldungValueOderRef) null : new SictMeldungValueOderRef(meldungValueOderRefSictJson.ValueSictJsonAbbild, (object) null, SictMeldungValueAstReferenzSictSeriel.KonstruktZurük(meldungValueOderRefSictJson.ValueBaumReferenz), meldungValueOderRefSictJson.Referenz);
    }

    public static SictMeldungObjektDiferenzSeriel SictSeriel(
      this SictMeldungObjektDiferenz referenziirteDiferenz)
    {
      if (referenziirteDiferenz == null)
        return (SictMeldungObjektDiferenzSeriel) null;
      int type = referenziirteDiferenz.Type;
      SictMeldungValueOderRefSictSeriel boxedWert = referenziirteDiferenz.BoxedWert.SictSeriel();
      KeyValuePair<int, SictMeldungValueOderRef>[] zuMemberIdentWert1 = referenziirteDiferenz.MengeZuMemberIdentWert;
      KeyValuePair<int, SictMeldungValueOderRefSictSeriel>[] mengeZuMemberIdentWert;
      if (zuMemberIdentWert1 == null)
      {
        mengeZuMemberIdentWert = (KeyValuePair<int, SictMeldungValueOderRefSictSeriel>[]) null;
      }
      else
      {
        IEnumerable<KeyValuePair<int, SictMeldungValueOderRefSictSeriel>> source = ((IEnumerable<KeyValuePair<int, SictMeldungValueOderRef>>) zuMemberIdentWert1).Select<KeyValuePair<int, SictMeldungValueOderRef>, KeyValuePair<int, SictMeldungValueOderRefSictSeriel>>((Func<KeyValuePair<int, SictMeldungValueOderRef>, KeyValuePair<int, SictMeldungValueOderRefSictSeriel>>) (zuMemberIdentWert => new KeyValuePair<int, SictMeldungValueOderRefSictSeriel>(zuMemberIdentWert.Key, zuMemberIdentWert.Value.SictSeriel())));
        mengeZuMemberIdentWert = source != null ? source.ToArray<KeyValuePair<int, SictMeldungValueOderRefSictSeriel>>() : (KeyValuePair<int, SictMeldungValueOderRefSictSeriel>[]) null;
      }
      SictMeldungValueOderRef[] listeElementWert = referenziirteDiferenz.CollectionListeElementWert;
      SictMeldungValueOderRefSictSeriel[] collectionListeElementWert;
      if (listeElementWert == null)
      {
        collectionListeElementWert = (SictMeldungValueOderRefSictSeriel[]) null;
      }
      else
      {
        IEnumerable<SictMeldungValueOderRefSictSeriel> source = ((IEnumerable<SictMeldungValueOderRef>) listeElementWert).Select<SictMeldungValueOderRef, SictMeldungValueOderRefSictSeriel>((Func<SictMeldungValueOderRef, SictMeldungValueOderRefSictSeriel>) (collectionElementWert => collectionElementWert.SictSeriel()));
        collectionListeElementWert = source != null ? source.ToArray<SictMeldungValueOderRefSictSeriel>() : (SictMeldungValueOderRefSictSeriel[]) null;
      }
      byte[] abbildListeByte = referenziirteDiferenz.AbbildListeByte;
      int num = referenziirteDiferenz.SequenzÄnderung ? 1 : 0;
      return new SictMeldungObjektDiferenzSeriel(type, boxedWert, mengeZuMemberIdentWert, collectionListeElementWert, abbildListeByte, num != 0);
    }

    public static SictMeldungObjektDiferenz KonstruktStructClr(
      this SictMeldungObjektDiferenzSeriel referenziirteDiferenzSictSeriel)
    {
      if (referenziirteDiferenzSictSeriel == null)
        return (SictMeldungObjektDiferenz) null;
      int type = referenziirteDiferenzSictSeriel.Type;
      SictMeldungValueOderRef boxedWert = referenziirteDiferenzSictSeriel.BoxedWert.KonstruktStructClr();
      KeyValuePair<int, SictMeldungValueOderRefSictSeriel>[] zuMemberIdentWert = referenziirteDiferenzSictSeriel.MengeZuMemberIdentWert;
      KeyValuePair<int, SictMeldungValueOderRef>[] mengeZuMemberIdentWert;
      if (zuMemberIdentWert == null)
      {
        mengeZuMemberIdentWert = (KeyValuePair<int, SictMeldungValueOderRef>[]) null;
      }
      else
      {
        IEnumerable<KeyValuePair<int, SictMeldungValueOderRef>> source = ((IEnumerable<KeyValuePair<int, SictMeldungValueOderRefSictSeriel>>) zuMemberIdentWert).Select<KeyValuePair<int, SictMeldungValueOderRefSictSeriel>, KeyValuePair<int, SictMeldungValueOderRef>>((Func<KeyValuePair<int, SictMeldungValueOderRefSictSeriel>, KeyValuePair<int, SictMeldungValueOderRef>>) (zuMemberNameWert => new KeyValuePair<int, SictMeldungValueOderRef>(zuMemberNameWert.Key, zuMemberNameWert.Value.KonstruktStructClr())));
        mengeZuMemberIdentWert = source != null ? source.ToArray<KeyValuePair<int, SictMeldungValueOderRef>>() : (KeyValuePair<int, SictMeldungValueOderRef>[]) null;
      }
      SictMeldungValueOderRefSictSeriel[] listeElementWert = referenziirteDiferenzSictSeriel.CollectionListeElementWert;
      SictMeldungValueOderRef[] collectionListeElementWert;
      if (listeElementWert == null)
      {
        collectionListeElementWert = (SictMeldungValueOderRef[]) null;
      }
      else
      {
        IEnumerable<SictMeldungValueOderRef> source = ((IEnumerable<SictMeldungValueOderRefSictSeriel>) listeElementWert).Select<SictMeldungValueOderRefSictSeriel, SictMeldungValueOderRef>((Func<SictMeldungValueOderRefSictSeriel, SictMeldungValueOderRef>) (collectionElementWert => collectionElementWert.KonstruktStructClr()));
        collectionListeElementWert = source != null ? source.ToArray<SictMeldungValueOderRef>() : (SictMeldungValueOderRef[]) null;
      }
      byte[] abbildListeByte = referenziirteDiferenzSictSeriel.AbbildListeByte;
      int num = referenziirteDiferenzSictSeriel.SequenzÄnderung ? 1 : 0;
      return new SictMeldungObjektDiferenz(type, boxedWert, mengeZuMemberIdentWert, collectionListeElementWert, abbildListeByte, num != 0);
    }
  }
}
