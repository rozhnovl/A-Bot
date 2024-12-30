// Decompiled with JetBrains decompiler
// Type: Bib3.RefBaumKopii.RefBaumKopiiStatic
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Bib3.RefNezDiferenz;
using Fasterflect;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bib3.RefBaumKopii
{
  public class RefBaumKopiiStatic
  {
    public static Scatescpaicer ScatescpaicerAppDomain = new Scatescpaicer();
    private static readonly Type TypeArray = typeof (Array);
    private static readonly Type TypeString = typeof (string);
    private static readonly Type InterfaceCollectionGeneric = typeof (ICollection<>);
    private static readonly Type InterfaceCollection = typeof (ICollection);
    private static readonly Type InterfaceList = typeof (IList);
    private static string TempDebugTypeName = (string) null;
    private static string DebugMemberName = (string) null;
    private static readonly Type TempDebugTypeSictDictZuList = typeof (SictDictZuList<,>);

    public static T ObjektKopiiErsctele<T>(T zuKopiireObjekt, Param param = null, int? tiifeScrankeMax = null) where T : class => RefBaumKopiiStatic.ObjektKopiiErsctele((object) zuKopiireObjekt, param, RefBaumKopiiStatic.ScatescpaicerAppDomain, tiifeScrankeMax) as T;

    public static object ObjektKopiiErscteleUndErsazType<ZuErsezeT, ErsazT>(
      object zuKopiireObjekt,
      ref Profile profile,
      int? tiifeScrankeMax = null)
    {
      Type type1 = typeof (ZuErsezeT);
      Type type2 = typeof (ErsazT);
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer typeBehandlungRictliinie = SictRefNezKopii.ZuMengeHerkunftTypeZiilTypeRictlinieMitScatescpaicer(new Dictionary<Type, Type>()
      {
        [type1] = type2
      }.ToArray<KeyValuePair<Type, Type>>());
      Param obj = new Param(profile, typeBehandlungRictliinie);
      return RefBaumKopiiStatic.ObjektKopiiErsctele(zuKopiireObjekt, obj, RefBaumKopiiStatic.ScatescpaicerAppDomain, tiifeScrankeMax);
    }

    public static object ObjektKopiiErsctele(object zuKopiireObjekt, int? tiifeScrankeMax = null) => RefBaumKopiiStatic.ObjektKopiiErsctele(zuKopiireObjekt, RefBaumKopiiStatic.ScatescpaicerAppDomain, tiifeScrankeMax);

    public static MemberInfo[] MengeMemberGefiltertNewestInHierarchy(
      IEnumerable<MemberInfo> mengeMember)
    {
      return mengeMember == null ? (MemberInfo[]) null : ((IEnumerable<IGrouping<string, MemberInfo>>) mengeMember.GroupBy<MemberInfo, string>((Func<MemberInfo, string>) (member => member.Name)).ToArray<IGrouping<string, MemberInfo>>()).Select<IGrouping<string, MemberInfo>, MemberInfo>((Func<IGrouping<string, MemberInfo>, MemberInfo>) (grupe => RefBaumKopiiStatic.MengeMemberGefiltertNewestInHierarchyAnnaameGlaicnaamig((IEnumerable<MemberInfo>) grupe))).ToArray<MemberInfo>();
    }

    public static MemberInfo MengeMemberGefiltertNewestInHierarchyAnnaameGlaicnaamig(
      IEnumerable<MemberInfo> mengeMember)
    {
      return mengeMember == null ? (MemberInfo) null : ((IEnumerable<MemberInfo>) mengeMember.OrderBy<MemberInfo, Type>((Func<MemberInfo, Type>) (member => member.DeclaringType), (IComparer<Type>) new TypeHierarchyComparer()).ToArray<MemberInfo>()).FirstOrDefault<MemberInfo>();
    }

    public static MemberInfo[] FürTypeMengeMemberKandidaatFürSerialis(Type type)
    {
      if ((Type) null == type)
        return (MemberInfo[]) null;
      BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      Type baseType = type.BaseType;
      MemberInfo[] members = type.GetMembers(bindingAttr);
      IEnumerable<MemberInfo> source = ((IEnumerable<IEnumerable<MemberInfo>>) new IEnumerable<MemberInfo>[2]
      {
        (IEnumerable<MemberInfo>) ((Type) null == baseType ? (MemberInfo[]) null : RefBaumKopiiStatic.FürTypeMengeMemberKandidaatFürSerialis(baseType)),
        (IEnumerable<MemberInfo>) members
      }).ListeEnumerableAgregiirt<MemberInfo>();
      return source != null ? source.ToArray<MemberInfo>() : (MemberInfo[]) null;
    }

    public static bool ErfordertKopiiRekursBerecne(Type type)
    {
      if ((Type) null == type || type.IsPrimitive || type.IsEnum || typeof (string) == type)
        return false;
      if (!type.IsValueType)
        return true;
      ZuMemberMengeDelegate[] seq = RefBaumKopiiStatic.ScatescpaicerAppDomain.ZuHerkunftTypeUndZiilTypeMengeDelegate(type, type);
      if (seq.IsNullOrEmpty())
        return false;
      foreach (ZuMemberMengeDelegate memberMengeDelegate in seq)
      {
        if (memberMengeDelegate != null && memberMengeDelegate.ErfordertKopiiRekurs)
          return true;
      }
      return false;
    }

    public static ZuMemberMengeDelegate[] ZuHerkunftTypeUndZiilTypeMengeDelegateBerecne(
      Type herkunftType,
      Type ziilType)
    {
      if ((Type) null == herkunftType || (Type) null == ziilType)
        return (ZuMemberMengeDelegate[]) null;
      object[] customAttributes1 = herkunftType.GetCustomAttributes(true);
      ziilType.GetCustomAttributes(true);
      MemberSerialization memberSerialization = (MemberSerialization) 0;
      if (customAttributes1 != null)
      {
        foreach (object obj in customAttributes1)
        {
          if (obj != null && obj is JsonObjectAttribute jsonObjectAttribute)
            memberSerialization = jsonObjectAttribute.MemberSerialization;
        }
      }
      MemberInfo[] mengeMember1 = RefBaumKopiiStatic.FürTypeMengeMemberKandidaatFürSerialis(herkunftType);
      MemberInfo[] mengeMember2 = RefBaumKopiiStatic.FürTypeMengeMemberKandidaatFürSerialis(ziilType);
      MemberInfo[] memberInfoArray = RefBaumKopiiStatic.MengeMemberGefiltertNewestInHierarchy((IEnumerable<MemberInfo>) mengeMember1);
      MemberInfo[] source = RefBaumKopiiStatic.MengeMemberGefiltertNewestInHierarchy((IEnumerable<MemberInfo>) mengeMember2);
      List<ZuMemberMengeDelegate> memberMengeDelegateList = new List<ZuMemberMengeDelegate>();
      foreach (MemberInfo memberInfo1 in memberInfoArray)
      {
        MemberInfo HerkunftTypeMember = memberInfo1;
        string name = HerkunftTypeMember.Name;
        try
        {
          FieldInfo fieldInfo = HerkunftTypeMember as FieldInfo;
          PropertyInfo propertyInfo = HerkunftTypeMember as PropertyInfo;
          object[] customAttributes2 = HerkunftTypeMember.GetCustomAttributes(true);
          MemberInfo memberInfo2 = ((IEnumerable<MemberInfo>) source).FirstOrDefault<MemberInfo>((Func<MemberInfo, bool>) (kandidaat => string.Equals(kandidaat.Name, HerkunftTypeMember.Name)));
          if (!((MemberInfo) null == memberInfo2))
          {
            JsonPropertyAttribute propertyAttribute1 = (JsonPropertyAttribute) null;
            if (customAttributes2 != null)
            {
              foreach (object obj in customAttributes2)
              {
                if (obj is JsonPropertyAttribute propertyAttribute2)
                  propertyAttribute1 = propertyAttribute2;
              }
            }
            bool flag = false;
            if ((FieldInfo) null != fieldInfo)
            {
              if (memberSerialization == null || 2 == (int)memberSerialization)
                flag = true;
              if (1 == (int)memberSerialization && propertyAttribute1 != null)
                flag = true;
            }
            if ((PropertyInfo) null != propertyInfo)
            {
              MethodInfo setMethod = propertyInfo.SetMethod;
              MethodInfo getMethod = propertyInfo.GetMethod;
              if ((MethodInfo) null != setMethod && (MethodInfo) null != getMethod && memberSerialization == 0)
                flag = true;
              if (1 == (int)memberSerialization && propertyAttribute1 != null)
                flag = true;
            }
            if (flag)
            {
              Type declaringType1 = HerkunftTypeMember.DeclaringType;
              Type declaringType2 = memberInfo2.DeclaringType;
              MemberGetter getter = (MemberGetter) null;
              MemberSetter setter = (MemberSetter) null;
              Type type = (Type) null;
              if ((FieldInfo) null != fieldInfo)
              {
                type = fieldInfo.FieldType;
                getter = FieldExtensions.DelegateForGetFieldValue(declaringType1, name);
                setter = FieldExtensions.DelegateForSetFieldValue(declaringType2, name);
              }
              if ((PropertyInfo) null != propertyInfo)
              {
                type = propertyInfo.PropertyType;
                getter = PropertyExtensions.DelegateForGetPropertyValue(declaringType1, name);
                setter = PropertyExtensions.DelegateForSetPropertyValue(declaringType2, name);
              }
              if (setter != null && getter != null && !((Type) null == type))
              {
                bool erfordertKopiiRekurs = RefBaumKopiiStatic.ScatescpaicerAppDomain.TypeErfordertKopiiRekurs(type);
                ZuMemberMengeDelegate memberMengeDelegate = new ZuMemberMengeDelegate(declaringType1, name, type, erfordertKopiiRekurs, getter, setter);
                memberMengeDelegateList.Add(memberMengeDelegate);
              }
            }
          }
        }
        catch (Exception ex)
        {
          throw new ApplicationException("MemberName = " + name, ex);
        }
      }
      return memberMengeDelegateList.ToArray();
    }

    public static object ObjektKopiiErsctele(
      object zuKopiireObjekt,
      Scatescpaicer scatescpaicer,
      int? tiifeScrankeMax = null)
    {
      return RefBaumKopiiStatic.ObjektKopiiErsctele(zuKopiireObjekt, (Param) null, scatescpaicer, tiifeScrankeMax);
    }

    public static object ObjektKopiiErsctele(
      object zuKopiireObjekt,
      Param param,
      Scatescpaicer scatescpaicer,
      int? tiifeScrankeMax = null)
    {
      return RefBaumKopiiStatic.ObjektKopiiErsctele(zuKopiireObjekt, (Type) null, param, scatescpaicer, tiifeScrankeMax);
    }

    public static object ObjektKopiiErsctele(
      object zuKopiireObjekt,
      Type vorgaabeZiilObjektType,
      Param param,
      Scatescpaicer scatescpaicer,
      int? tiifeScrankeMax = null,
      Stack<object> sctaapelErkenungZyyklus = null)
    {
      return SictRefNezKopii.ObjektKopiiErsctele(zuKopiireObjekt, param, scatescpaicer, tiifeScrankeMax);
    }
  }
}
