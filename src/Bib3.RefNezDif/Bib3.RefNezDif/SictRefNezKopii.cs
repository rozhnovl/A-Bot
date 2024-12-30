// Decompiled with JetBrains decompiler
// Type: Bib3.SictRefNezKopii
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Bib3.RefBaumKopii;
using Bib3.RefNezDiferenz;
using Bib3.RefNezDiferenz.NewtonsoftJson;
using Fasterflect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bib3
{
  public static class SictRefNezKopii
  {
    private static readonly Type TypeArray = typeof (Array);
    private static readonly Type TypeString = typeof (string);
    private static readonly Type InterfaceCollectionGeneric = typeof (ICollection<>);
    private static readonly Type InterfaceCollection = typeof (ICollection);
    private static readonly Type InterfaceList = typeof (IList);
    private static string TempDebugTypeName = (string) null;
    private static string DebugMemberName = (string) null;
    private static readonly Type TempDebugTypeSictDictZuList = typeof (SictDictZuList<,>);
    private static SictScatenscpaicerDict<KeyValuePair<Type, Type>[], SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer> ScatescpaicerZuMengeZuHerkunftTypeZiilTypeRictliinie = new SictScatenscpaicerDict<KeyValuePair<Type, Type>[], SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer>();
    public static SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer SctandardRictlinieMitScatescpaicer = SictRefNezKopii.ZuMengeHerkunftTypeZiilTypeRictlinieMitScatescpaicerKonstrukt();

    private static SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer ZuMengeHerkunftTypeZiilTypeRictlinieMitScatescpaicerKonstrukt(
      KeyValuePair<Type, Type>[] mengeHerkunftTypeZiilType = null)
    {
      return new SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer(SictMengeTypeBehandlungRictliinieNewtonsoftJson.KonstruktMengeTypeBehandlungRictliinie(mengeHerkunftTypeZiilType));
    }

    public static SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer ZuMengeHerkunftTypeZiilTypeRictlinieMitScatescpaicer(
      KeyValuePair<Type, Type>[] mengeHerkunftTypeZiilType = null)
    {
      return mengeHerkunftTypeZiilType.IsNullOrEmpty() ? SictRefNezKopii.SctandardRictlinieMitScatescpaicer : SictRefNezKopii.ScatescpaicerZuMengeZuHerkunftTypeZiilTypeRictliinie.ValueFürKey(mengeHerkunftTypeZiilType, new Func<KeyValuePair<Type, Type>[], SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer>(SictRefNezKopii.ZuMengeHerkunftTypeZiilTypeRictlinieMitScatescpaicerKonstrukt));
    }

    public static T ObjektKopiiErsctele<T>(T zuKopiireObjekt, Param param = null, int? tiifeScrankeMax = null) => SictRefNezKopii.ObjektKopiiErsctele<T>(zuKopiireObjekt, param, RefBaumKopiiStatic.ScatescpaicerAppDomain, tiifeScrankeMax);

    public static T ObjektKopiiErsctele<T>(
      T zuKopiireObjekt,
      Param param = null,
      Scatescpaicer scatescpaicer = null,
      int? tiifeScrankeMax = null)
    {
      return (T) SictRefNezKopii.ObjektKopiiErsctele((object) zuKopiireObjekt, typeof (T), param, scatescpaicer, tiifeScrankeMax);
    }

    public static object ObjektKopiiErsctele(
      object zuKopiireObjekt,
      Param param,
      Scatescpaicer scatescpaicer,
      int? tiifeScrankeMax = null)
    {
      return SictRefNezKopii.ObjektKopiiErsctele(zuKopiireObjekt, (Type) null, param, scatescpaicer, tiifeScrankeMax);
    }

    public static object ObjektKopiiErsctele(
      object zuKopiireObjekt,
      Type vorgaabeZiilObjektType,
      Param param,
      Scatescpaicer scatescpaicer,
      int? tiifeScrankeMax = null,
      IDictionary<object, object> dictObjektKopii = null)
    {
      if (zuKopiireObjekt == null)
        return (object) null;
      if (scatescpaicer == null)
        scatescpaicer = new Scatescpaicer();
      Type type1 = zuKopiireObjekt.GetType();
      if (param == null)
        param = new Param((Profile) null, SictRefNezKopii.ZuMengeHerkunftTypeZiilTypeRictlinieMitScatescpaicer());
      if (param != null && param.ReferenzLaseAus && type1.IsClass && !(type1 == typeof (string)))
        return (object) null;
      SictZuTypeBehandlung zuTypeBehandlung = param.ZuTypeBehandlung(type1);
      Profile profile = param == null ? (Profile) null : param.Profile;
      if (profile == null)
      {
        profile = new Profile();
        if (param != null)
          param.Profile = profile;
      }
      ++profile.ObjektAnzaal;
      string tempDebugTypeName = SictRefNezKopii.TempDebugTypeName;
      if (tempDebugTypeName == null || !string.Equals(tempDebugTypeName, type1.Name))
        ;
      if (dictObjektKopii == null)
        dictObjektKopii = (IDictionary<object, object>) new Dictionary<object, object>();
      if (type1.IsClass)
      {
        object obj = (object) null;
        dictObjektKopii.TryGetValue(zuKopiireObjekt, out obj);
        if (obj != null)
          return obj;
      }
      try
      {
        bool erfordertKopiiRekurs1 = zuTypeBehandlung.ErfordertKopiiRekurs;
        bool behandlungAlsReferenz = zuTypeBehandlung.BehandlungAlsReferenz;
        bool behandlungAlsCollection = zuTypeBehandlung.BehandlungAlsCollection;
        SictZuTypeBehandlung.CollectionElementFüügeAinDelegate delegateElementFüügeAin = zuTypeBehandlung.CollectionDelegateElementFüügeAin;
        bool erfordertKopiiRekurs2 = zuTypeBehandlung.CollectionElementTypeErfordertKopiiRekurs;
        Type ziilType = zuTypeBehandlung.ZiilType;
        SictZuMemberBehandlung[] memberBehandlung1 = zuTypeBehandlung.MengeMemberBehandlung;
        if (!behandlungAlsReferenz && !erfordertKopiiRekurs1)
          return zuKopiireObjekt;
        Type type2 = vorgaabeZiilObjektType;
        if ((object) type2 == null)
          type2 = ziilType;
        Type type3 = type2;
        IEnumerable enumerable = zuKopiireObjekt as IEnumerable;
        Type baseType = type1.BaseType;
        Array array1 = zuKopiireObjekt as Array;
        object instance;
        if (array1 != null)
          instance = Activator.CreateInstance(type3, (object) array1.Length);
        else
          instance = Activator.CreateInstance(type3, true);
        dictObjektKopii[zuKopiireObjekt] = instance;
        Array array2 = instance as Array;
        IList list = instance as IList;
        if (behandlungAlsCollection)
        {
          List<object> objectList = new List<object>();
          foreach (object zuKopiireObjekt1 in enumerable)
          {
            Param obj1 = param;
            Scatescpaicer scatescpaicer1 = scatescpaicer;
            int? nullable = tiifeScrankeMax;
            int? tiifeScrankeMax1 = nullable.HasValue ? new int?(nullable.GetValueOrDefault() - 1) : new int?();
            IDictionary<object, object> dictObjektKopii1 = dictObjektKopii;
            object obj2 = SictRefNezKopii.ObjektKopiiErsctele(zuKopiireObjekt1, (Type) null, obj1, scatescpaicer1, tiifeScrankeMax1, dictObjektKopii1);
            objectList.Add(obj2);
          }
          for (int index = 0; index < objectList.Count; ++index)
          {
            object element = objectList[index];
            if (array1 != null)
              ArrayExtensions.SetElement((object) array2, (long) index, element);
            else if (list != null)
              list.Add(element);
            else
              delegateElementFüügeAin(instance, element);
          }
          return instance;
        }
        object obj3 = ValueTypeExtensions.WrapIfValueType(zuKopiireObjekt);
        object obj4 = ValueTypeExtensions.WrapIfValueType(instance);
        foreach (SictZuMemberBehandlung memberBehandlung2 in memberBehandlung1)
        {
          MemberGetter typeMemberGetter = memberBehandlung2.HerkunftTypeMemberGetter;
          MemberSetter typeMemberSetter = memberBehandlung2.ZiilTypeMemberSetter;
          object obj5 = typeMemberGetter.Invoke(obj3);
          if (obj5 != null)
          {
            object zuKopiireObjekt2 = obj5;
            Param obj6 = param;
            Scatescpaicer scatescpaicer2 = scatescpaicer;
            int? nullable = tiifeScrankeMax;
            int? tiifeScrankeMax2 = nullable.HasValue ? new int?(nullable.GetValueOrDefault() - 1) : new int?();
            IDictionary<object, object> dictObjektKopii2 = dictObjektKopii;
            object obj7 = SictRefNezKopii.ObjektKopiiErsctele(zuKopiireObjekt2, (Type) null, obj6, scatescpaicer2, tiifeScrankeMax2, dictObjektKopii2);
            typeMemberSetter.Invoke(obj4, obj7);
          }
        }
        return ValueTypeExtensions.UnwrapIfWrapped(obj4);
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Type = " + type1.FullName, ex);
      }
      finally
      {
      }
    }

    public static void KopiireFlacVon(
      this object ziilObjekt,
      object herkunftObjekt,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieScatescpaicer)
    {
      if (ziilObjekt == null || herkunftObjekt == null || rictliinieScatescpaicer == null)
        return;
      SictZuTypeBehandlung zuTypeBehandlung1 = rictliinieScatescpaicer.ZuTypeBehandlung(ziilObjekt.GetType());
      SictZuMemberBehandlung[] source = (SictZuMemberBehandlung[]) null;
      SictZuTypeBehandlung zuTypeBehandlung2 = (SictZuTypeBehandlung) null;
      if (herkunftObjekt != null)
        zuTypeBehandlung2 = rictliinieScatescpaicer.ZuTypeBehandlung(herkunftObjekt.GetType());
      if (zuTypeBehandlung2 != null)
        source = zuTypeBehandlung2.MengeMemberBehandlung;
      SictZuMemberBehandlung[] memberBehandlung1 = zuTypeBehandlung1.MengeMemberBehandlung;
      if (memberBehandlung1 == null)
        return;
      foreach (SictZuMemberBehandlung memberBehandlung2 in memberBehandlung1)
      {
        if (memberBehandlung2 != null)
        {
          string ZiilMemberName = memberBehandlung2.HerkunftMemberName;
          MemberSetter typeMemberSetter = memberBehandlung2.ZiilTypeMemberSetter;
          Type herkunftMemberType = memberBehandlung2.HerkunftMemberType;
          Type type = Nullable.GetUnderlyingType(herkunftMemberType);
          if ((object) type == null)
            type = herkunftMemberType;
          Type kandidaatBase = type;
          if (ZiilMemberName != null && typeMemberSetter != null && !((Type) null == kandidaatBase))
          {
            SictZuMemberBehandlung memberBehandlung3 = source != null ? ((IEnumerable<SictZuMemberBehandlung>) source).FirstOrDefault<SictZuMemberBehandlung>((Func<SictZuMemberBehandlung, bool>) (kandidaat => string.Equals(kandidaat.HerkunftMemberName, ZiilMemberName))) : (SictZuMemberBehandlung) null;
            object obj1 = herkunftMemberType.GetDefaultValue();
            try
            {
              if (memberBehandlung3 != null)
              {
                MemberGetter typeMemberGetter = memberBehandlung3.HerkunftTypeMemberGetter;
                if (typeMemberGetter != null)
                {
                  object obj2 = typeMemberGetter.Invoke(herkunftObjekt);
                  if (obj2 != null)
                  {
                    if (obj2.GetType().InheritsOrImplementsOrEquals(kandidaatBase))
                      obj1 = obj2;
                  }
                }
              }
            }
            finally
            {
              typeMemberSetter.Invoke(ziilObjekt, obj1);
            }
          }
        }
      }
    }
  }
}
