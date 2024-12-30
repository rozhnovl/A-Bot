// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictMeldungValueOderRef
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Bib3.RefBaumKopii;
using Fasterflect;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bib3.RefNezDiferenz
{
  public class SictMeldungValueOderRef
  {
    public readonly string ValueSictJsonAbbild;
    public readonly SictWertAst ValueBaumReferenz;
    private static IDictionary<Type, Func<string, object>> DictZuMemberTypeFunkWertBerecneAusSictJson = (IDictionary<Type, Func<string, object>>) ((IEnumerable<KeyValuePair<Type, Func<string, object>>>) new KeyValuePair<Type, Func<string, object>>[0]).ToDictionary<KeyValuePair<Type, Func<string, object>>, Type, Func<string, object>>((Func<KeyValuePair<Type, Func<string, object>>, Type>) (t => t.Key), (Func<KeyValuePair<Type, Func<string, object>>, Func<string, object>>) (t => t.Value));
    private static readonly Type TypeArray = typeof (Array);
    private static readonly Type TypeString = typeof (string);
    private static readonly Type TypeQueueGeneric = typeof (Queue<>);
    private static readonly Type InterfaceCollectionGeneric = typeof (ICollection<>);
    private static readonly Type InterfaceCollection = typeof (ICollection);
    private static readonly Type InterfaceList = typeof (IList);
    private static readonly Type InterfaceListGeneric = typeof (IList<>);
    private static readonly Type InterfaceDictionaryGeneric = typeof (IDictionary<,>);

    public object ReferenzClr { private set; get; }

    public long? ReferenzTransport { private set; get; }

    public SictMeldungValueOderRef(
      string valueSictJsonAbbild,
      object referenzClr,
      SictWertAst valueBaumReferenz,
      long? referenzTransport = null)
    {
      this.ValueSictJsonAbbild = valueSictJsonAbbild;
      this.ReferenzClr = referenzClr;
      this.ValueBaumReferenz = valueBaumReferenz;
      this.ReferenzTransport = referenzTransport;
    }

    public void BerecneTailReferenz(
      Func<object, long> funkReferenzTransportBerecneAusReferenzClr)
    {
      if (this.ReferenzClr != null)
        this.ReferenzTransport = new long?(funkReferenzTransportBerecneAusReferenzClr(this.ReferenzClr));
      this.ValueBaumReferenz?.BerecneTailReferenz(funkReferenzTransportBerecneAusReferenzClr);
    }

    public void MengeReferenzClrVerwendetFüügeAinNaacList(List<object> list)
    {
      if (list == null)
        return;
      object referenzClr = this.ReferenzClr;
      if (referenzClr != null)
        list.Add(referenzClr);
      this.ValueBaumReferenz?.MengeReferenzClrVerwendetFüügeAinNaacList(list);
    }

    public object KonstruiireObjektReferenzClr(
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieScatescpaicer,
      Type vorgaabeType,
      Func<long, object> funkReferenzClrBerecneAusReferenzTransport)
    {
      object ziilObjektReferenzClr = this.ReferenzClr;
      string valueSictJsonAbbild = this.ValueSictJsonAbbild;
      if (valueSictJsonAbbild != null)
        this.ReferenzClr = ziilObjektReferenzClr = SictMeldungValueOderRef.BoxedValueBerecneAusBoxedValueSictJson(valueSictJsonAbbild, vorgaabeType);
      long? referenzTransport = this.ReferenzTransport;
      SictWertAst valueBaumReferenz = this.ValueBaumReferenz;
      if (referenzTransport.HasValue)
        return this.ReferenzClr = funkReferenzClrBerecneAusReferenzTransport(referenzTransport.Value);
      if (valueBaumReferenz == null)
        return ziilObjektReferenzClr;
      if (ziilObjektReferenzClr == null)
        throw new ArgumentNullException("ReferenzClr");
      valueBaumReferenz.Appliziire(rictliinieScatescpaicer, ref ziilObjektReferenzClr, funkReferenzClrBerecneAusReferenzTransport);
      this.ReferenzClr = ziilObjektReferenzClr;
      return ziilObjektReferenzClr;
    }

    public static object BoxedValueBerecneAusBoxedValueSictJson(
      string memberWertSictJsonAbbild,
      Type memberType)
    {
      if (memberWertSictJsonAbbild == null)
        return (object) null;
      Func<string, object> func;
      SictMeldungValueOderRef.DictZuMemberTypeFunkWertBerecneAusSictJson.TryGetValue(memberType, out func);
      return func != null ? func(memberWertSictJsonAbbild) : JsonConvert.DeserializeObject(memberWertSictJsonAbbild, memberType);
    }

    public static string BoxedValueSictJson(object memberWert) => memberWert == null ? (string) null : JsonConvert.SerializeObject(memberWert);

    public static object BoxedValueKopiireAbzüüglicReferenz(
      object boxedValue,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieScatescpaicer)
    {
      return SictRefNezKopii.ObjektKopiiErsctele(boxedValue, new Param((Profile) null, rictliinieScatescpaicer, true), RefBaumKopiiStatic.ScatescpaicerAppDomain);
    }

    public static bool TypeBehandlungAlsCollection(Type type) => SictMeldungValueOderRef.TypeBehandlungAlsCollection(type, out Type _, out SictZuTypeBehandlung.CollectionClearDelegate _, out SictZuTypeBehandlung.CollectionElementFüügeAinDelegate _);

    public static bool TypeBehandlungAlsCollection(
      Type type,
      out Type collectionElementType,
      out SictZuTypeBehandlung.CollectionClearDelegate collectionDelegateClear,
      out SictZuTypeBehandlung.CollectionElementFüügeAinDelegate collectionDelegateElementFüügeAin)
    {
      collectionElementType = (Type) null;
      collectionDelegateClear = (SictZuTypeBehandlung.CollectionClearDelegate) null;
      collectionDelegateElementFüügeAin = (SictZuTypeBehandlung.CollectionElementFüügeAinDelegate) null;
      if ((Type) null == type)
        return false;
      Type genericTypeDefinition = type.IsConstructedGenericType ? type.GetGenericTypeDefinition() : (Type) null;
      bool flag1 = genericTypeDefinition.InheritsOrImplementsOrEquals(SictMeldungValueOderRef.InterfaceListGeneric);
      bool flag2 = genericTypeDefinition.InheritsOrImplementsOrEquals(SictMeldungValueOderRef.InterfaceCollectionGeneric);
      bool flag3 = !((Type) null == genericTypeDefinition) && SictMeldungValueOderRef.TypeQueueGeneric.IsAssignableFrom(genericTypeDefinition);
      Type type1 = type.GenericTypeZuBaseOderInterfaceGenericTypeDefinition(typeof (HashSet<>));
      bool flag4 = genericTypeDefinition.InheritsOrImplementsOrEquals(SictMeldungValueOderRef.InterfaceDictionaryGeneric);
      bool flag5 = false;
      if (type.IsArray)
      {
        flag5 = true;
        collectionElementType = type.GetElementType();
      }
      if (TypeExtensions.Implements(type, SictMeldungValueOderRef.InterfaceList))
        flag5 = true;
      if (flag1)
      {
        flag5 = true;
        collectionElementType = ((IEnumerable<Type>) type.GetGenericArguments()).FirstOrDefault<Type>();
      }
      if (flag3)
      {
        flag5 = true;
        collectionElementType = ((IEnumerable<Type>) type.GetGenericArguments()).FirstOrDefault<Type>();
        Type KopiiCollectionElementType = collectionElementType;
        collectionDelegateClear = (SictZuTypeBehandlung.CollectionClearDelegate) (collection => MethodExtensions.CallMethod(collection, "Clear", new object[0]));
        collectionDelegateElementFüügeAin = (SictZuTypeBehandlung.CollectionElementFüügeAinDelegate) ((collection, element) => MethodExtensions.CallMethod(collection, "Enqueue", new Type[1]
        {
          KopiiCollectionElementType
        }, new object[1]{ element }));
      }
      if ((Type) null != type1)
      {
        flag5 = true;
        collectionElementType = type1;
        Type KopiiCollectionElementType = collectionElementType;
        collectionDelegateClear = (SictZuTypeBehandlung.CollectionClearDelegate) (collection => MethodExtensions.CallMethod(collection, "Clear", new object[0]));
        collectionDelegateElementFüügeAin = (SictZuTypeBehandlung.CollectionElementFüügeAinDelegate) ((collection, element) => MethodExtensions.CallMethod(collection, "Add", new Type[1]
        {
          KopiiCollectionElementType
        }, new object[1]{ element }));
      }
      if (flag4)
        flag5 = true;
      if (flag5 && flag2)
      {
        type.GenericTypeZuBaseOderInterfaceGenericTypeDefinition(SictMeldungValueOderRef.InterfaceCollectionGeneric);
        ref Type local = ref collectionElementType;
        Type[] source = type.ListeTypeArgumentZuBaseOderInterface(SictMeldungValueOderRef.InterfaceCollectionGeneric);
        Type type2 = source != null ? ((IEnumerable<Type>) source).FirstOrDefault<Type>() : (Type) null;
        local = type2;
        Type KopiiCollectionElementType = collectionElementType;
        if (flag4)
        {
          Type[] typeArray = KopiiCollectionElementType.ListeTypeArgumentZuBaseOderInterface(typeof (KeyValuePair<,>));
          Type KeyType = typeArray[0];
          Type ValueType = typeArray[1];
          collectionDelegateClear = (SictZuTypeBehandlung.CollectionClearDelegate) (collection => MethodExtensions.CallMethod(collection, "Clear", new object[0]));
          collectionDelegateElementFüügeAin = (SictZuTypeBehandlung.CollectionElementFüügeAinDelegate) ((collection, element) =>
          {
            object obj = ValueTypeExtensions.WrapIfValueType(element);
            object propertyValue1 = PropertyExtensions.GetPropertyValue(obj, "Key");
            object propertyValue2 = PropertyExtensions.GetPropertyValue(obj, "Value");
            if (propertyValue1 == null)
              return;
            if ((bool) MethodExtensions.CallMethod(collection, "ContainsKey", new object[1]
            {
              propertyValue1
            }))
              return;
            MethodExtensions.CallMethod(collection, "Add", new Type[2]
            {
              KeyType,
              ValueType
            }, new object[2]
            {
              propertyValue1,
              propertyValue2
            });
          });
        }
        else
          collectionDelegateElementFüügeAin = (SictZuTypeBehandlung.CollectionElementFüügeAinDelegate) ((collection, element) => MethodExtensions.CallMethod(collection, "Add", new Type[1]
          {
            KopiiCollectionElementType
          }, new object[1]{ element }));
      }
      return flag5;
    }

    public static void AusReferenziirteMengeReferenzClrVerwendetFüügeAinNaacList(
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieScatescpaicer,
      object referenz,
      IList<KeyValuePair<object, object>> ziilList)
    {
      if (referenz == null || ziilList == null)
        return;
      SictZuTypeBehandlung zuTypeBehandlung = SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer.ZuTypeBehandlung(referenz.GetType(), rictliinieScatescpaicer);
      if (zuTypeBehandlung == null)
        return;
      if (!zuTypeBehandlung.BehandlungAlsReferenz)
        SictMeldungValueOderRef.AusBoxedValueMengeReferenzClrVerwendetFüügeAinNaacList(rictliinieScatescpaicer, referenz, ziilList, (object) "UnBox");
      else if (zuTypeBehandlung.BehandlungAlsCollection)
      {
        if (!zuTypeBehandlung.CollectionElementTypeErfordertKopiiRekurs)
          return;
        IEnumerable enumerable = referenz as IEnumerable;
        long memberOderIndexIdent = 0;
        foreach (object boxedValue in enumerable)
        {
          SictMeldungValueOderRef.AusBoxedValueMengeReferenzClrVerwendetFüügeAinNaacList(rictliinieScatescpaicer, boxedValue, ziilList, (object) memberOderIndexIdent);
          ++memberOderIndexIdent;
        }
      }
      else
      {
        foreach (SictZuMemberBehandlung memberBehandlung in zuTypeBehandlung.MengeMemberBehandlung)
        {
          object boxedValue = memberBehandlung.HerkunftTypeMemberGetter.Invoke(referenz);
          if (boxedValue != null)
            SictMeldungValueOderRef.AusBoxedValueMengeReferenzClrVerwendetFüügeAinNaacList(rictliinieScatescpaicer, boxedValue, ziilList, (object) memberBehandlung.HerkunftMemberName);
        }
      }
    }

    public static void AusBoxedValueMengeReferenzClrVerwendetFüügeAinNaacList(
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieScatescpaicer,
      object boxedValue,
      IList<KeyValuePair<object, object>> ziilList,
      object memberOderIndexIdent)
    {
      if (boxedValue == null || ziilList == null)
        return;
      Type type = boxedValue.GetType();
      SictZuTypeBehandlung zuTypeBehandlung = rictliinieScatescpaicer != null ? rictliinieScatescpaicer.ZuTypeBehandlung(type) : throw new ArgumentNullException("RictliinieScatescpaicer");
      if (zuTypeBehandlung == null)
        return;
      if (zuTypeBehandlung.BehandlungAlsReferenz)
      {
        ziilList.Add(new KeyValuePair<object, object>(memberOderIndexIdent, boxedValue));
      }
      else
      {
        if (!zuTypeBehandlung.ErfordertKopiiRekurs)
          return;
        foreach (SictZuMemberBehandlung memberBehandlung in zuTypeBehandlung.MengeMemberBehandlung)
        {
          object boxedValue1 = memberBehandlung.HerkunftTypeMemberGetter.Invoke(ValueTypeExtensions.WrapIfValueType(boxedValue));
          if (boxedValue1 != null)
            SictMeldungValueOderRef.AusBoxedValueMengeReferenzClrVerwendetFüügeAinNaacList(rictliinieScatescpaicer, boxedValue1, ziilList, memberOderIndexIdent);
        }
      }
    }

    public static SictMeldungValueOderRef KonstruiireFürValueOderRef(
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieScatescpaicer,
      object valueOderRef)
    {
      if (valueOderRef == null)
        return new SictMeldungValueOderRef((string) null, (object) null, (SictWertAst) null);
      SictZuTypeBehandlung zuTypeBehandlung = SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer.ZuTypeBehandlung(valueOderRef.GetType(), rictliinieScatescpaicer);
      if (zuTypeBehandlung == null)
        return (SictMeldungValueOderRef) null;
      return zuTypeBehandlung.BehandlungAlsReferenz ? new SictMeldungValueOderRef((string) null, valueOderRef, (SictWertAst) null) : SictMeldungValueOderRef.KonstruiireFürBoxedValue(rictliinieScatescpaicer, valueOderRef);
    }

    public static SictMeldungValueOderRef KonstruiireFürBoxedValue(
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieScatescpaicer,
      object boxedValue)
    {
      Type type = boxedValue != null ? boxedValue.GetType() : throw new ArgumentNullException("BoxedValue");
      bool erfordertKopiiRekurs = ((rictliinieScatescpaicer != null ? rictliinieScatescpaicer.ZuTypeBehandlung(type) : throw new ArgumentNullException("RictliinieScatescpaicer")) ?? throw new ArgumentNullException("TypeBehandlung")).ErfordertKopiiRekurs;
      object memberWert = boxedValue;
      if (erfordertKopiiRekurs)
        memberWert = SictMeldungValueOderRef.BoxedValueKopiireAbzüüglicReferenz(boxedValue, rictliinieScatescpaicer);
      return new SictMeldungValueOderRef(SictMeldungValueOderRef.BoxedValueSictJson(memberWert), (object) null, erfordertKopiiRekurs ? SictWertAst.KonstruktFürStruct(rictliinieScatescpaicer, boxedValue) : (SictWertAst) null);
    }
  }
}
