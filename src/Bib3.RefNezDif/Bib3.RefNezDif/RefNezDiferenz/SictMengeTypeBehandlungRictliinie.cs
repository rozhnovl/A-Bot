// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictMengeTypeBehandlungRictliinie
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Bib3.RefBaumKopii;
using Fasterflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bib3.RefNezDiferenz
{
  public class SictMengeTypeBehandlungRictliinie
  {
    public readonly IZuTypeEntscaidungBinäär TypeAbbildFraigaabeRictliinie;
    public readonly IZuMemberEntscaidungBinäär MemberBehandlungRictliinie;
    public readonly SictZuTypeBehandlungRictliinie[] MengeZuTypeBehandlungRictliinie;

    public static SictZuTypeBehandlung ZuTypeBehandlungBerecne(
      Type herkunftType,
      SictMengeTypeBehandlungRictliinie rictliinie,
      Func<Type, SictZuTypeBehandlung> callbackZuTypeBehandlung,
      Action<Type, SictZuTypeBehandlung> callbackZuTypeBehandlungErgeebnis)
    {
      return rictliinie?.ZuTypeBehandlungBerecne(herkunftType, callbackZuTypeBehandlung, callbackZuTypeBehandlungErgeebnis);
    }

    public static MemberInfo[] MengeMemberGefiltertNewestInHierarchy(
      IEnumerable<MemberInfo> mengeMember)
    {
      return mengeMember == null ? (MemberInfo[]) null : ((IEnumerable<IGrouping<string, MemberInfo>>) mengeMember.GroupBy<MemberInfo, string>((Func<MemberInfo, string>) (member => member.Name)).ToArray<IGrouping<string, MemberInfo>>()).Select<IGrouping<string, MemberInfo>, MemberInfo>((Func<IGrouping<string, MemberInfo>, MemberInfo>) (grupe => SictMengeTypeBehandlungRictliinie.MengeMemberGefiltertNewestInHierarchyAnnaameGlaicnaamig((IEnumerable<MemberInfo>) grupe))).ToArray<MemberInfo>();
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
        (IEnumerable<MemberInfo>) ((Type) null == baseType ? (MemberInfo[]) null : SictMengeTypeBehandlungRictliinie.FürTypeMengeMemberKandidaatFürSerialis(baseType)),
        (IEnumerable<MemberInfo>) members
      }).ListeEnumerableAgregiirt<MemberInfo>();
      return source != null ? source.ToArray<MemberInfo>() : (MemberInfo[]) null;
    }

    public bool AbbildFraigaabeBerecne(Type type)
    {
      if ((Type) null == type)
        return false;
      IZuTypeEntscaidungBinäär fraigaabeRictliinie = this.TypeAbbildFraigaabeRictliinie;
      return fraigaabeRictliinie == null || fraigaabeRictliinie.TypeBehandlung(type);
    }

    public bool ErfordertKopiiRekursBerecne(
      Type type,
      Func<Type, SictZuTypeBehandlung> callbackZuTypeBehandlung)
    {
      if ((Type) null == type || type.IsPrimitive || type.IsEnum || typeof (string) == type)
        return false;
      if (!type.IsValueType)
        return true;
      SictZuTypeBehandlung zuTypeBehandlung = callbackZuTypeBehandlung(type);
      return zuTypeBehandlung != null && zuTypeBehandlung.ErfordertKopiiRekurs;
    }

    public Type ZiilTypeBerecneAusHerkunftType(Type herkunftType)
    {
      if ((Type) null == herkunftType)
        return (Type) null;
      SictZuTypeBehandlungRictliinie[] behandlungRictliinie1 = this.MengeZuTypeBehandlungRictliinie;
      if (behandlungRictliinie1 == null)
        return herkunftType;
      SictZuTypeBehandlungRictliinie behandlungRictliinie2 = ((IEnumerable<SictZuTypeBehandlungRictliinie>) behandlungRictliinie1).FirstOrDefault<SictZuTypeBehandlungRictliinie>((Func<SictZuTypeBehandlungRictliinie, bool>) (kandidaat => kandidaat.HerkunftType == herkunftType));
      return behandlungRictliinie2 != null ? behandlungRictliinie2.ZiilType : herkunftType;
    }

    public SictZuMemberBehandlung[] ZuTypeBehandlungMengeMemberBerecne(
      Type herkunftType,
      Func<Type, SictZuTypeBehandlung> callbackZuTypeBehandlung = null,
      Action<Type, SictZuTypeBehandlung> callbackZuTypeBehandlungErgeebnis = null)
    {
      return this.ZuTypeBehandlungBerecne(herkunftType, callbackZuTypeBehandlung, callbackZuTypeBehandlungErgeebnis, true)?.MengeMemberBehandlung;
    }

    public SictZuTypeBehandlung ZuTypeBehandlungBerecne(
      Type herkunftType,
      Func<Type, SictZuTypeBehandlung> callbackZuTypeBehandlung = null,
      Action<Type, SictZuTypeBehandlung> callbackZuTypeBehandlungErgeebnis = null,
      bool erfordertKopiiRekursBerecnungLaseAus = false)
    {
      if ((Type) null == herkunftType)
        return (SictZuTypeBehandlung) null;
      try
      {
        SictZuTypeBehandlungRictliinie[] behandlungRictliinie1 = this.MengeZuTypeBehandlungRictliinie;
        IZuMemberEntscaidungBinäär entscaidungBinäär = this.MemberBehandlungRictliinie;
        bool behandlungAlsReferenz = false;
        bool behandlungAlsAtom = false;
        if (herkunftType.IsPrimitive)
          behandlungAlsAtom = true;
        if (herkunftType.IsEnum)
          behandlungAlsAtom = true;
        SictZuTypeBehandlungRictliinie behandlungRictliinie2 = (SictZuTypeBehandlungRictliinie) null;
        if (behandlungRictliinie1 != null)
          behandlungRictliinie2 = ((IEnumerable<SictZuTypeBehandlungRictliinie>) behandlungRictliinie1).FirstOrDefault<SictZuTypeBehandlungRictliinie>((Func<SictZuTypeBehandlungRictliinie, bool>) (kandidaat => kandidaat.HerkunftType == herkunftType));
        Type type1 = (Type) null;
        if (behandlungRictliinie2 != null)
        {
          type1 = behandlungRictliinie2.ZiilType;
          entscaidungBinäär = behandlungRictliinie2.MemberBehandlungRictliinie ?? entscaidungBinäär;
        }
        Type type2 = !((Type) null == type1) ? type1 : this.ZiilTypeBerecneAusHerkunftType(herkunftType);
        bool abbildFraigaabe = this.AbbildFraigaabeBerecne(type2);
        SictZuMemberBehandlung[] mengeMemberBehandlung = (SictZuMemberBehandlung[]) null;
        herkunftType.GetCustomAttributes(true);
        type2.GetCustomAttributes(true);
        Type HerkunftCollectionElementType = (Type) null;
        SictZuTypeBehandlung.CollectionClearDelegate collectionDelegateClear = (SictZuTypeBehandlung.CollectionClearDelegate) null;
        SictZuTypeBehandlung.CollectionElementFüügeAinDelegate collectionDelegateElementFüügeAin = (SictZuTypeBehandlung.CollectionElementFüügeAinDelegate) null;
        if (herkunftType.IsClass)
          behandlungAlsReferenz = true;
        if (typeof (string) == herkunftType)
          behandlungAlsReferenz = false;
        bool erfordertKopiiRekurs = behandlungAlsReferenz;
        bool behandlungAlsCollection = SictMeldungValueOderRef.TypeBehandlungAlsCollection(herkunftType, out HerkunftCollectionElementType, out collectionDelegateClear, out collectionDelegateElementFüügeAin);
        Type type3 = HerkunftCollectionElementType;
        if ((Type) null != HerkunftCollectionElementType && behandlungRictliinie1 != null)
        {
          SictZuTypeBehandlungRictliinie behandlungRictliinie3 = ((IEnumerable<SictZuTypeBehandlungRictliinie>) behandlungRictliinie1).FirstOrDefault<SictZuTypeBehandlungRictliinie>((Func<SictZuTypeBehandlungRictliinie, bool>) (kandidaat => kandidaat.HerkunftType == HerkunftCollectionElementType));
          if (behandlungRictliinie3 != null)
          {
            Type type4 = behandlungRictliinie3.ZiilType;
            if ((object) type4 == null)
              type4 = HerkunftCollectionElementType;
            type3 = type4;
          }
        }
        if (!behandlungAlsCollection || HerkunftCollectionElementType == type3 || !((Type) null == type1))
          ;
        bool collectionElementTypeErfordertKopiiRekurs = this.ErfordertKopiiRekursBerecne(HerkunftCollectionElementType, callbackZuTypeBehandlung);
        if (!behandlungAlsCollection && !behandlungAlsAtom)
        {
          MemberInfo[] mengeMember1 = SictMengeTypeBehandlungRictliinie.FürTypeMengeMemberKandidaatFürSerialis(herkunftType);
          MemberInfo[] mengeMember2 = SictMengeTypeBehandlungRictliinie.FürTypeMengeMemberKandidaatFürSerialis(type2);
          MemberInfo[] memberInfoArray = SictMengeTypeBehandlungRictliinie.MengeMemberGefiltertNewestInHierarchy((IEnumerable<MemberInfo>) mengeMember1);
          MemberInfo[] source = SictMengeTypeBehandlungRictliinie.MengeMemberGefiltertNewestInHierarchy((IEnumerable<MemberInfo>) mengeMember2);
          List<SictZuMemberBehandlung> memberBehandlungList = new List<SictZuMemberBehandlung>();
          int num = 0;
          foreach (MemberInfo memberInfo1 in memberInfoArray)
          {
            MemberInfo HerkunftTypeMember = memberInfo1;
            string name = HerkunftTypeMember.Name;
            try
            {
              if (entscaidungBinäär != null)
              {
                FieldInfo fieldInfo = HerkunftTypeMember as FieldInfo;
                PropertyInfo propertyInfo = HerkunftTypeMember as PropertyInfo;
                HerkunftTypeMember.GetCustomAttributes(true);
                MemberInfo memberInfo2 = ((IEnumerable<MemberInfo>) source).FirstOrDefault<MemberInfo>((Func<MemberInfo, bool>) (kandidaat => string.Equals(kandidaat.Name, HerkunftTypeMember.Name)));
                if (!((MemberInfo) null == memberInfo2))
                {
                  if (entscaidungBinäär.MemberBehandlung(HerkunftTypeMember))
                  {
                    Type declaringType1 = HerkunftTypeMember.DeclaringType;
                    Type declaringType2 = memberInfo2.DeclaringType;
                    MemberGetter herkunftTypeMemberGetter = (MemberGetter) null;
                    MemberSetter ziilTypeMemberSetter = (MemberSetter) null;
                    Type type5 = (Type) null;
                    if ((FieldInfo) null != fieldInfo)
                    {
                      type5 = fieldInfo.FieldType;
                      herkunftTypeMemberGetter = FieldExtensions.DelegateForGetFieldValue(declaringType1, name);
                      ziilTypeMemberSetter = FieldExtensions.DelegateForSetFieldValue(declaringType2, name);
                    }
                    if ((PropertyInfo) null != propertyInfo)
                    {
                      type5 = propertyInfo.PropertyType;
                      herkunftTypeMemberGetter = PropertyExtensions.DelegateForGetPropertyValue(declaringType1, name);
                      try
                      {
                        ziilTypeMemberSetter = PropertyExtensions.DelegateForSetPropertyValue(declaringType2, name);
                      }
                      catch (MissingFieldException ex)
                      {
                      }
                    }
                    if (herkunftTypeMemberGetter != null && !((Type) null == type5))
                    {
                      if (this.AbbildFraigaabeBerecne(type5))
                      {
                        if (this.ErfordertKopiiRekursBerecne(type5, callbackZuTypeBehandlung))
                          erfordertKopiiRekurs = true;
                        SictZuMemberBehandlung memberBehandlung = new SictZuMemberBehandlung(declaringType1, name, type5, herkunftTypeMemberGetter, ziilTypeMemberSetter, ++num);
                        memberBehandlungList.Add(memberBehandlung);
                      }
                    }
                  }
                }
              }
            }
            catch (Exception ex)
            {
              throw new ApplicationException("MemberName = " + name, ex);
            }
          }
          mengeMemberBehandlung = memberBehandlungList.ToArray();
        }
        bool alsListeByte = false;
        if ((Type) null != HerkunftCollectionElementType & behandlungAlsCollection && HerkunftCollectionElementType == type3 && HerkunftCollectionElementType.IsPrimitive)
          alsListeByte = true;
        SictZuTypeBehandlung zuTypeBehandlung = new SictZuTypeBehandlung(herkunftType, type2, abbildFraigaabe, behandlungAlsAtom, behandlungAlsReferenz, erfordertKopiiRekurs, behandlungAlsCollection, HerkunftCollectionElementType, collectionElementTypeErfordertKopiiRekurs, collectionDelegateClear, collectionDelegateElementFüügeAin, mengeMemberBehandlung, alsListeByte);
        if (!erfordertKopiiRekursBerecnungLaseAus)
          callbackZuTypeBehandlungErgeebnis(herkunftType, zuTypeBehandlung);
        return zuTypeBehandlung;
      }
      catch (Exception ex)
      {
        throw new ApplicationException("HerkunftType=" + herkunftType.FullName, ex);
      }
    }

    public SictMengeTypeBehandlungRictliinie()
    {
    }

    public SictMengeTypeBehandlungRictliinie(
      IZuMemberEntscaidungBinäär memberBehandlungRictliinie,
      SictZuTypeBehandlungRictliinie[] mengeZuTypeBehandlungRictliinie = null,
      IZuTypeEntscaidungBinäär typeAbbildFraigaabeRictliinie = null)
    {
      this.MemberBehandlungRictliinie = memberBehandlungRictliinie;
      this.MengeZuTypeBehandlungRictliinie = mengeZuTypeBehandlungRictliinie;
      this.TypeAbbildFraigaabeRictliinie = typeAbbildFraigaabeRictliinie;
    }
  }
}
