// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictZuObjektDiferenzZuusctand
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Fasterflect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Bib3.RefNezDiferenz
{
  public class SictZuObjektDiferenzZuusctand
  {
    private static readonly Type TypeArray = typeof (Array);
    private static readonly Type TypeString = typeof (string);
    private static readonly Type TypeQueueGeneric = typeof (Queue<>);
    private static readonly Type InterfaceCollectionGeneric = typeof (ICollection<>);
    private static readonly Type InterfaceCollection = typeof (ICollection);
    private static readonly Type InterfaceList = typeof (IList);
    private static readonly SictScatenscpaicerDict<Type, object> ScatescpaicerFürTypeDefaultValue = new SictScatenscpaicerDict<Type, object>();
    public readonly SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer TypeBehandlungRictliinieMitScatescpaicer;
    public readonly object ObjektRefClr;
    public readonly long ObjektRefTransport;
    private readonly IDictionary<int, KeyValuePair<long, object>> DictZuMemberMeldungLezteScritIndexUndRefClr = (IDictionary<int, KeyValuePair<long, object>>) new Dictionary<int, KeyValuePair<long, object>>();
    private IDictionary<int, KeyValuePair<long, SictMeldungValueOderRef>> SictSumeNaaczuhooleMengeZuMemberMeldungLezteIdentUndScritIndexUndWert;
    private KeyValuePair<long, SictMeldungValueOderRef[]> NaaczuhooleCollectionListeElementWert;
    private KeyValuePair<long, SictMeldungValueOderRef> NaaczuhooleBoxedWert;
    public SictSumeZuTypeAgrInfo SictSumeZuTypeAgregatioonInfo;
    public int? TypeTransportIdent;
    private readonly IDictionary<long, KeyValuePair<long, object>> CollectionDictZuElementIndexMeldungLezte = (IDictionary<long, KeyValuePair<long, object>>) new Dictionary<long, KeyValuePair<long, object>>();
    private KeyValuePair<long, Sequenz> TempCollectionZuElementIndexMeldungLezte;
    private long? MeldungTypeLezteScritIndex;

    public object ObjektKopiiRefClr { private set; get; }

    public KeyValuePair<int, SictZuTypeBehandlung> TypeIdentUndBehandlung { private set; get; }

    public Type HerkunftType
    {
      get
      {
        KeyValuePair<int, SictZuTypeBehandlung> identUndBehandlung = this.TypeIdentUndBehandlung;
        return identUndBehandlung.Value == null ? (Type) null : identUndBehandlung.Value.HerkunftType;
      }
    }

    public Type ZiilType
    {
      get
      {
        KeyValuePair<int, SictZuTypeBehandlung> identUndBehandlung = this.TypeIdentUndBehandlung;
        return identUndBehandlung.Value == null ? (Type) null : identUndBehandlung.Value.ZiilType;
      }
    }

    public bool TypeGemeldet { private set; get; }

    public long? MeldungLezteScritIndex { private set; get; }

    public SictZuObjektDiferenzZuusctand(
      object objektRefClr,
      long objektRefTransport,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer typeBehandlungRictliinieMitScatescpaicer = null,
      SictSumeZuTypeAgrInfo sictSumeZuTypeAgregatioonInfo = null)
    {
      this.ObjektRefClr = objektRefClr;
      this.ObjektRefTransport = objektRefTransport;
      this.TypeBehandlungRictliinieMitScatescpaicer = typeBehandlungRictliinieMitScatescpaicer;
      this.SictSumeZuTypeAgregatioonInfo = sictSumeZuTypeAgregatioonInfo;
      if (objektRefClr == null)
        return;
      this.TypeIdentUndBehandlung = SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer.ZuTypeIdentUndBehandlung(objektRefClr.GetType(), typeBehandlungRictliinieMitScatescpaicer);
    }

    public static bool CollectionGlaicwertig(IEnumerable collection0, IEnumerable collection1) => Glob.SequenceEqualPerObjectEquals(collection0, collection1);

    public SictMeldungObjektDiferenz BerecneScritDif(
      long scritIndex,
      long? ältesteZuVerwendendeScritIndexScrankeMin,
      out IEnumerable<object> mengeReferenzClrVerwendet,
      out IEnumerable<KeyValuePair<string, object>> mengeMemberNameUndReferenzClr,
      out object[] collectionListeReferenzClr,
      out long verwendeteFrüühesteScritIndex)
    {
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer mitScatescpaicer = this.TypeBehandlungRictliinieMitScatescpaicer;
      mengeReferenzClrVerwendet = (IEnumerable<object>) null;
      mengeMemberNameUndReferenzClr = (IEnumerable<KeyValuePair<string, object>>) null;
      collectionListeReferenzClr = (object[]) null;
      verwendeteFrüühesteScritIndex = scritIndex;
      if (this.ObjektRefClr == null)
        return (SictMeldungObjektDiferenz) null;
      KeyValuePair<int, SictZuTypeBehandlung> identUndBehandlung = this.TypeIdentUndBehandlung;
      SictZuTypeBehandlung zuTypeBehandlung = identUndBehandlung.Value;
      if (zuTypeBehandlung == null)
        return (SictMeldungObjektDiferenz) null;
      Type herkunftType = zuTypeBehandlung.HerkunftType;
      Type ziilType = zuTypeBehandlung.ZiilType;
      if ((Type) null == herkunftType || (Type) null == ziilType)
        return (SictMeldungObjektDiferenz) null;
      SictMeldungValueOderRef boxedWert = (SictMeldungValueOderRef) null;
      List<KeyValuePair<int, SictMeldungValueOderRef>> enumerable = (List<KeyValuePair<int, SictMeldungValueOderRef>>) null;
      SictMeldungValueOderRef[] collectionListeElementWert = (SictMeldungValueOderRef[]) null;
      byte[] numArray = (byte[]) null;
      bool behandlungAlsReferenz1 = zuTypeBehandlung.BehandlungAlsReferenz;
      bool behandlungAlsCollection = zuTypeBehandlung.BehandlungAlsCollection;
      Type collectionElementType = zuTypeBehandlung.CollectionElementType;
      bool erfordertKopiiRekurs = zuTypeBehandlung.CollectionElementTypeErfordertKopiiRekurs;
      SictZuMemberBehandlung[] memberBehandlung1 = zuTypeBehandlung.MengeMemberBehandlung;
      bool alsListeByte = zuTypeBehandlung.AlsListeByte;
      long? nullable1;
      if (behandlungAlsReferenz1)
      {
        Array objektRefClr1 = this.ObjektRefClr as Array;
        Type genericTypeDefinition = ziilType.IsConstructedGenericType ? ziilType.GetGenericTypeDefinition() : (Type) null;
        bool flag1 = !((Type) null == genericTypeDefinition) && TypeExtensions.Implements(genericTypeDefinition, SictZuObjektDiferenzZuusctand.InterfaceCollectionGeneric);
        bool flag2 = !((Type) null == genericTypeDefinition) && SictZuObjektDiferenzZuusctand.TypeQueueGeneric.IsAssignableFrom(genericTypeDefinition);
        SictZuObjektDiferenzZuusctand.InterfaceCollection.IsAssignableFrom(ziilType);
        IEnumerable objektRefClr2 = this.ObjektRefClr as IEnumerable;
        IList objektRefClr3 = this.ObjektRefClr as IList;
        bool flag3 = !((Type) null == collectionElementType) && collectionElementType.IsValueType;
        if (behandlungAlsCollection)
        {
          bool behandlungAlsReferenz2 = SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer.ZuTypeBehandlung(collectionElementType, mitScatescpaicer).BehandlungAlsReferenz;
          KeyValuePair<long, Sequenz> indexMeldungLezte = this.TempCollectionZuElementIndexMeldungLezte;
          bool flag4 = false;
          bool flag5 = false;
          if (ältesteZuVerwendendeScritIndexScrankeMin.HasValue && indexMeldungLezte.Value != null)
          {
            long key = indexMeldungLezte.Key;
            nullable1 = ältesteZuVerwendendeScritIndexScrankeMin;
            long valueOrDefault = nullable1.GetValueOrDefault();
            if (key < valueOrDefault && nullable1.HasValue)
              flag4 = true;
          }
          Sequenz o1 = SequenzFunk.SequenzKopiire(objektRefClr2);
          Array listeElement = o1.ListeElement;
          if (!flag4 && indexMeldungLezte.Value != null && SequenzFunk.SequenzPrüüfeGlaicwertig(indexMeldungLezte.Value, o1))
            flag5 = true;
          if (!flag5)
          {
            this.TempCollectionZuElementIndexMeldungLezte = new KeyValuePair<long, Sequenz>(scritIndex, o1);
            if (alsListeByte)
            {
              Array src = listeElement;
              numArray = new byte[Marshal.SizeOf(collectionElementType) * o1.ListeElementAnzaal];
              Buffer.BlockCopy(src, 0, (Array) numArray, 0, numArray.Length);
            }
            else
            {
              collectionListeElementWert = new SictMeldungValueOderRef[o1.ListeElementAnzaal];
              for (int index = 0; index < collectionListeElementWert.Length; ++index)
              {
                object element = ArrayExtensions.GetElement((object) listeElement, (long) index);
                if (element != null)
                {
                  collectionListeElementWert[index] = SictMeldungValueOderRef.KonstruiireFürValueOderRef(mitScatescpaicer, element);
                  if (!behandlungAlsReferenz2)
                    ;
                }
              }
            }
          }
          else
            verwendeteFrüühesteScritIndex = Math.Min(verwendeteFrüühesteScritIndex, indexMeldungLezte.Key);
        }
        else
        {
          foreach (SictZuMemberBehandlung memberBehandlung2 in memberBehandlung1)
          {
            if (memberBehandlung2 != null)
            {
              Type herkunftMemberType = memberBehandlung2.HerkunftMemberType;
              string herkunftMemberName = memberBehandlung2.HerkunftMemberName;
              int transportMemberIdent = memberBehandlung2.SictDiferenzTransportMemberIdent;
              object obj = memberBehandlung2.HerkunftTypeMemberGetter.Invoke(this.ObjektRefClr);
              Type type = obj == null ? (Type) null : obj.GetType();
              bool flag6 = false;
              bool flag7 = true;
              KeyValuePair<long, object> keyValuePair;
              bool flag8 = this.DictZuMemberMeldungLezteScritIndexUndRefClr.TryGetValue(transportMemberIdent, out keyValuePair);
              if (flag8)
              {
                if (ältesteZuVerwendendeScritIndexScrankeMin.HasValue)
                {
                  long key = keyValuePair.Key;
                  nullable1 = ältesteZuVerwendendeScritIndexScrankeMin;
                  long valueOrDefault = nullable1.GetValueOrDefault();
                  if (key < valueOrDefault && nullable1.HasValue)
                    flag6 = true;
                }
                if (!flag6 && object.Equals(keyValuePair.Value, obj))
                  flag7 = false;
              }
              else if (obj == null)
                flag7 = false;
              else if ((type.IsValueType || type.IsEnum) && (Type) null == Nullable.GetUnderlyingType(herkunftMemberType) && object.Equals(obj, type.GetDefaultValue()))
                flag7 = false;
              if (flag7)
              {
                if (enumerable == null)
                  enumerable = new List<KeyValuePair<int, SictMeldungValueOderRef>>();
                enumerable.Add(new KeyValuePair<int, SictMeldungValueOderRef>(transportMemberIdent, SictMeldungValueOderRef.KonstruiireFürValueOderRef(mitScatescpaicer, obj)));
                this.DictZuMemberMeldungLezteScritIndexUndRefClr[transportMemberIdent] = new KeyValuePair<long, object>(scritIndex, obj);
              }
              else if (flag8)
                verwendeteFrüühesteScritIndex = Math.Min(verwendeteFrüühesteScritIndex, keyValuePair.Key);
            }
          }
        }
      }
      else
        boxedWert = SictMeldungValueOderRef.KonstruiireFürBoxedValue(mitScatescpaicer, this.ObjektRefClr);
      bool flag9 = true;
      bool flag10 = false;
      if (this.MeldungTypeLezteScritIndex.HasValue)
      {
        flag9 = false;
        if (ältesteZuVerwendendeScritIndexScrankeMin.HasValue)
        {
          nullable1 = this.MeldungTypeLezteScritIndex;
          long? nullable2 = ältesteZuVerwendendeScritIndexScrankeMin;
          if (nullable1.GetValueOrDefault() < nullable2.GetValueOrDefault() && nullable1.HasValue & nullable2.HasValue)
            flag10 = true;
        }
      }
      int? nullable3 = flag9 | flag10 ? new int?(identUndBehandlung.Key) : new int?();
      if (nullable3.HasValue)
        this.MeldungTypeLezteScritIndex = new long?(scritIndex);
      verwendeteFrüühesteScritIndex = Math.Min(verwendeteFrüühesteScritIndex, this.MeldungTypeLezteScritIndex.Value);
      return SictMeldungObjektDiferenz.SequenzInterpretiirt(nullable3 ?? 0, boxedWert, enumerable != null ? enumerable.ToArrayIfNotEmpty<KeyValuePair<int, SictMeldungValueOderRef>>() : (KeyValuePair<int, SictMeldungValueOderRef>[]) null, collectionListeElementWert, numArray);
    }

    public void BerecneScritSumeTailInstanz(
      long scritIndex,
      SictMeldungObjektDiferenz scritDiferenz,
      bool ziilMemberExistentNictIgnoriire = false)
    {
      SictSumeZuTypeAgrInfo typeAgregatioonInfo = this.SictSumeZuTypeAgregatioonInfo;
      SictMeldungValueOderRef meldungValueOderRef = (SictMeldungValueOderRef) null;
      SictMeldungValueOderRef[] meldungValueOderRefArray = (SictMeldungValueOderRef[]) null;
      KeyValuePair<int, SictMeldungValueOderRef>[] keyValuePairArray = (KeyValuePair<int, SictMeldungValueOderRef>[]) null;
      byte[] numArray = (byte[]) null;
      bool flag1 = false;
      if (scritDiferenz != null)
      {
        meldungValueOderRef = scritDiferenz.BoxedWert;
        meldungValueOderRefArray = scritDiferenz.CollectionListeElementWert;
        keyValuePairArray = scritDiferenz.MengeZuMemberIdentWert;
        keyValuePairArray = scritDiferenz.MengeZuMemberIdentWert;
        numArray = scritDiferenz.AbbildListeByte;
        flag1 = scritDiferenz.SequenzÄnderung;
      }
      if (meldungValueOderRefArray == null && this.NaaczuhooleCollectionListeElementWert.Value != null)
        meldungValueOderRefArray = this.NaaczuhooleCollectionListeElementWert.Value;
      if (meldungValueOderRef == null)
        meldungValueOderRef = this.NaaczuhooleBoxedWert.Value;
      object objektKopiiRefClr = this.ObjektKopiiRefClr;
      long? nullable = new long?();
      if (typeAgregatioonInfo != null)
        this.TypeIdentUndBehandlung = new KeyValuePair<int, SictZuTypeBehandlung>(typeAgregatioonInfo.TransportIdent, typeAgregatioonInfo.TypeBehandlung);
      SictZuTypeBehandlung zuTypeBehandlung = this.TypeIdentUndBehandlung.Value;
      if (zuTypeBehandlung == null)
        return;
      if (flag1)
        nullable = !zuTypeBehandlung.AlsListeByte ? new long?(meldungValueOderRefArray == null ? 0L : (long) meldungValueOderRefArray.Length) : new long?(numArray == null ? 0L : (long) numArray.Length / (long) Marshal.SizeOf(zuTypeBehandlung.CollectionElementType));
      if (meldungValueOderRefArray != null)
        nullable = new long?((long) meldungValueOderRefArray.Length);
      Type ziilType = zuTypeBehandlung.ZiilType;
      if (meldungValueOderRef == null && (Type) null != ziilType)
      {
        bool flag2 = false;
        bool isArray = ziilType.IsArray;
        Type elementType = isArray ? ziilType.GetElementType() : (Type) null;
        if (objektKopiiRefClr != null && object.Equals((object) objektKopiiRefClr.GetType(), (object) ziilType))
          flag2 = true;
        if (!flag2)
        {
          object instance;
          if (isArray)
          {
            if (!nullable.HasValue)
              throw new ArgumentNullException("CollectionListeElementAnzaal");
            instance = (object) Array.CreateInstance(elementType, nullable.Value);
          }
          else
          {
            try
            {
              instance = Activator.CreateInstance(ziilType, true);
            }
            catch (Exception ex)
            {
              throw new Exception("CreateInstance failed for type " + ziilType?.ToString(), ex);
            }
          }
          this.ObjektKopiiRefClr = instance;
        }
      }
      this.MeldungLezteScritIndex = Glob.Max(this.MeldungLezteScritIndex, new long?(scritIndex));
    }

    public void BerecneScritSumeTailMember(
      long scritIndex,
      SictMeldungObjektDiferenz scritDiferenz,
      Func<long, object> funkReferenzClrBerecneAusReferenzTransport,
      bool ziilMemberExistentNictIgnoriire = false)
    {
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer mitScatescpaicer = this.TypeBehandlungRictliinieMitScatescpaicer;
      IDictionary<int, SictZuMemberBehandlung> dictionary = (IDictionary<int, SictZuMemberBehandlung>) null;
      SictSumeZuTypeAgrInfo typeAgregatioonInfo = this.SictSumeZuTypeAgregatioonInfo;
      if (typeAgregatioonInfo != null)
        dictionary = typeAgregatioonInfo.DictZuMemberIdentBehandlung;
      SictMeldungValueOderRef meldungValueOderRef1 = (SictMeldungValueOderRef) null;
      SictMeldungValueOderRef[] meldungValueOderRefArray = (SictMeldungValueOderRef[]) null;
      KeyValuePair<int, SictMeldungValueOderRef>[] keyValuePairArray = (KeyValuePair<int, SictMeldungValueOderRef>[]) null;
      byte[] src = (byte[]) null;
      if (scritDiferenz != null)
      {
        meldungValueOderRef1 = scritDiferenz.BoxedWert;
        meldungValueOderRefArray = scritDiferenz.CollectionListeElementWert;
        keyValuePairArray = scritDiferenz.MengeZuMemberIdentWert;
        src = scritDiferenz.AbbildListeByte;
      }
      List<int> intList = new List<int>();
      bool flag1 = false;
      bool flag2 = false;
      if (meldungValueOderRefArray == null && this.NaaczuhooleCollectionListeElementWert.Value != null)
        meldungValueOderRefArray = this.NaaczuhooleCollectionListeElementWert.Value;
      if (meldungValueOderRef1 == null)
        meldungValueOderRef1 = this.NaaczuhooleBoxedWert.Value;
      object obj1 = this.ObjektKopiiRefClr;
      try
      {
        SictZuTypeBehandlung zuTypeBehandlung = this.TypeIdentUndBehandlung.Value;
        if (keyValuePairArray == null && meldungValueOderRefArray == null && meldungValueOderRef1 == null && this.SictSumeNaaczuhooleMengeZuMemberMeldungLezteIdentUndScritIndexUndWert.IsNullOrEmpty() && src == null || obj1 == null || zuTypeBehandlung == null)
          return;
        Type ziilType = zuTypeBehandlung.ZiilType;
        bool behandlungAlsCollection = zuTypeBehandlung.BehandlungAlsCollection;
        Type collectionElementType = zuTypeBehandlung.CollectionElementType;
        bool erfordertKopiiRekurs = zuTypeBehandlung.CollectionElementTypeErfordertKopiiRekurs;
        SictZuTypeBehandlung.CollectionClearDelegate collectionDelegateClear = zuTypeBehandlung.CollectionDelegateClear;
        SictZuTypeBehandlung.CollectionElementFüügeAinDelegate delegateElementFüügeAin = zuTypeBehandlung.CollectionDelegateElementFüügeAin;
        bool alsListeByte = zuTypeBehandlung.AlsListeByte;
        Array array = obj1 as Array;
        Type type = obj1.GetType();
        Type genericTypeDefinition = type.IsConstructedGenericType ? type.GetGenericTypeDefinition() : (Type) null;
        bool flag3 = !((Type) null == genericTypeDefinition) && TypeExtensions.Implements(genericTypeDefinition, SictZuObjektDiferenzZuusctand.InterfaceCollectionGeneric);
        bool flag4 = !((Type) null == genericTypeDefinition) && SictZuObjektDiferenzZuusctand.TypeQueueGeneric.IsAssignableFrom(genericTypeDefinition);
        SictZuObjektDiferenzZuusctand.InterfaceCollection.IsAssignableFrom(type);
        ICollection collection = obj1 as ICollection;
        IList list = obj1 as IList;
        if (meldungValueOderRef1 != null)
        {
          obj1 = meldungValueOderRef1.KonstruiireObjektReferenzClr(mitScatescpaicer, ziilType, funkReferenzClrBerecneAusReferenzTransport);
          this.NaaczuhooleBoxedWert = new KeyValuePair<long, SictMeldungValueOderRef>();
          flag2 = true;
        }
        if (behandlungAlsCollection)
        {
          Array dst = (Array) null;
          if (src != null)
          {
            int num = Marshal.SizeOf(collectionElementType);
            int length = (int) ((long) src.Length / (long) num);
            dst = Array.CreateInstance(collectionElementType, length);
            Buffer.BlockCopy((Array) src, 0, dst, 0, length * num);
          }
          if (meldungValueOderRefArray != null)
          {
            dst = Array.CreateInstance(collectionElementType, meldungValueOderRefArray.Length);
            if (collectionDelegateClear != null)
              collectionDelegateClear(obj1);
            if (array == null && list != null)
              list.Clear();
            for (int index = 0; (long) index < (long) meldungValueOderRefArray.Length; ++index)
            {
              SictMeldungValueOderRef meldungValueOderRef2 = meldungValueOderRefArray[index];
              object obj2 = (object) null;
              if (meldungValueOderRef2 != null)
                obj2 = meldungValueOderRef2.KonstruiireObjektReferenzClr(mitScatescpaicer, collectionElementType, funkReferenzClrBerecneAusReferenzTransport);
              dst.SetValue(obj2, index);
            }
            this.NaaczuhooleCollectionListeElementWert = new KeyValuePair<long, SictMeldungValueOderRef[]>();
          }
          if (dst == null)
            return;
          if (array != null)
          {
            dst.CopyTo(array, 0);
          }
          else
          {
            for (int index = 0; index < dst.Length; ++index)
            {
              object element = dst.GetValue(index);
              if (list == null)
              {
                if (delegateElementFüügeAin == null)
                  throw new ArgumentNullException("CollectionDelegateElementFüügeAin");
                delegateElementFüügeAin(obj1, element);
              }
              else
                list.Add(element);
            }
          }
          flag1 = true;
        }
        else
        {
          if (!this.SictSumeNaaczuhooleMengeZuMemberMeldungLezteIdentUndScritIndexUndWert.IsNullOrEmpty())
          {
            foreach (KeyValuePair<int, KeyValuePair<long, SictMeldungValueOderRef>> keyValuePair in this.SictSumeNaaczuhooleMengeZuMemberMeldungLezteIdentUndScritIndexUndWert.ToArray<KeyValuePair<int, KeyValuePair<long, SictMeldungValueOderRef>>>())
            {
              int key = keyValuePair.Key;
              SictZuMemberBehandlung memberBehandlung = (SictZuMemberBehandlung) null;
              dictionary?.TryGetValue(key, out memberBehandlung);
              if (memberBehandlung != null)
              {
                SictZuObjektDiferenzZuusctand.MemberSezeWert(key, keyValuePair.Value.Value, funkReferenzClrBerecneAusReferenzTransport, mitScatescpaicer, obj1, memberBehandlung);
                this.SictSumeNaaczuhooleMengeZuMemberMeldungLezteIdentUndScritIndexUndWert.Remove(key);
              }
            }
          }
          if (keyValuePairArray != null)
          {
            foreach (KeyValuePair<int, SictMeldungValueOderRef> keyValuePair in keyValuePairArray)
            {
              int key = keyValuePair.Key;
              SictZuMemberBehandlung memberBehandlung = (SictZuMemberBehandlung) null;
              dictionary?.TryGetValue(key, out memberBehandlung);
              if (memberBehandlung != null)
              {
                SictZuObjektDiferenzZuusctand.MemberSezeWert(key, keyValuePair.Value, funkReferenzClrBerecneAusReferenzTransport, mitScatescpaicer, obj1, memberBehandlung);
                intList.Add(key);
              }
            }
          }
        }
      }
      finally
      {
        if (meldungValueOderRef1 != null && !flag2)
          this.NaaczuhooleBoxedWert = new KeyValuePair<long, SictMeldungValueOderRef>(scritIndex, meldungValueOderRef1);
        if (meldungValueOderRefArray != null && !flag1)
          this.NaaczuhooleCollectionListeElementWert = new KeyValuePair<long, SictMeldungValueOderRef[]>(scritIndex, meldungValueOderRefArray);
        if (keyValuePairArray != null)
        {
          foreach (KeyValuePair<int, SictMeldungValueOderRef> keyValuePair in keyValuePairArray)
          {
            int key = keyValuePair.Key;
            if (!intList.Contains(key))
            {
              if (this.SictSumeNaaczuhooleMengeZuMemberMeldungLezteIdentUndScritIndexUndWert == null)
                this.SictSumeNaaczuhooleMengeZuMemberMeldungLezteIdentUndScritIndexUndWert = (IDictionary<int, KeyValuePair<long, SictMeldungValueOderRef>>) new Dictionary<int, KeyValuePair<long, SictMeldungValueOderRef>>();
              this.SictSumeNaaczuhooleMengeZuMemberMeldungLezteIdentUndScritIndexUndWert[key] = new KeyValuePair<long, SictMeldungValueOderRef>(scritIndex, keyValuePair.Value);
            }
          }
        }
        this.MeldungLezteScritIndex = Glob.Max(this.MeldungLezteScritIndex, new long?(scritIndex));
      }
    }

    private static void MemberSezeWert(
      int memberIdent,
      SictMeldungValueOderRef memberWertZuSeze,
      Func<long, object> funkReferenzClrBerecneAusReferenzTransport,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieScatescpaicer,
      object objektKopiiRefClr,
      SictZuMemberBehandlung memberBehandlung)
    {
      Type herkunftMemberType = memberBehandlung.HerkunftMemberType;
      if ((Type) null == herkunftMemberType)
        throw new ArgumentNullException("MemberType");
      Type vorgaabeType = SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer.ZuTypeBehandlung(herkunftMemberType, rictliinieScatescpaicer).BehandlungAlsReferenz ? (Type) null : herkunftMemberType;
      object obj = (object) null;
      if (memberWertZuSeze != null)
        obj = memberWertZuSeze.KonstruiireObjektReferenzClr(rictliinieScatescpaicer, vorgaabeType, funkReferenzClrBerecneAusReferenzTransport);
      memberBehandlung.ZiilTypeMemberSetter.Invoke(objektKopiiRefClr, obj);
    }
  }
}
