// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictRefNezDiferenz
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib3.RefNezDiferenz
{
  public class SictRefNezDiferenz
  {
    private readonly SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer TypeBehandlungRictliinieMitScatescpaicer;
    private readonly SictIdentInt64Fabrik ObjektIdentFabrik = new SictIdentInt64Fabrik(1L);
    private readonly IDictionary<object, SictZuObjektDiferenzZuusctand> DictZuObjInfo = (IDictionary<object, SictZuObjektDiferenzZuusctand>) new Dictionary<object, SictZuObjektDiferenzZuusctand>();
    private readonly IDictionary<int, SictDifZuTypeMeldungInfo> MengeZuTypeMeldungLezte = (IDictionary<int, SictDifZuTypeMeldungInfo>) new Dictionary<int, SictDifZuTypeMeldungInfo>();
    private readonly SictDiferenzSictParam SictParam;
    private readonly object Lock = new object();

    public long ÄltesteNocVerwendeteScritIndex { private set; get; }

    public long ScritLezteIndex { private set; get; }

    public SictRefNezDiferenz()
    {
    }

    public SictRefNezDiferenz(
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer)
      : this(new SictDiferenzSictParam(rictliinieMitScatescpaicer))
    {
    }

    public SictRefNezDiferenz(SictDiferenzSictParam sictParam)
    {
      this.SictParam = sictParam;
      this.TypeBehandlungRictliinieMitScatescpaicer = (sictParam == null ? (SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer) null : sictParam.TypeBehandlungRictliinieMitScatescpaicer) ?? new SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer((SictMengeTypeBehandlungRictliinie) null);
    }

    public SictZuNezSictDiferenzScritAbbild BerecneScritDif(
      long? ältesteZuVerwendendeScritIndexScrankeMin,
      object[] listeWurzel,
      Action<object> callbackObjektNoi = null)
    {
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer mitScatescpaicer = this.TypeBehandlungRictliinieMitScatescpaicer;
      lock (this.Lock)
      {
        long scritIndex1 = this.ScritLezteIndex++;
        long val1 = scritIndex1;
        Dictionary<object, KeyValuePair<long, KeyValuePair<SictZuObjektDiferenzZuusctand, SictMeldungObjektDiferenz>>> source1 = new Dictionary<object, KeyValuePair<long, KeyValuePair<SictZuObjektDiferenzZuusctand, SictMeldungObjektDiferenz>>>();
        Dictionary<object, bool> dictionary1 = new Dictionary<object, bool>();
        Dictionary<long, SictMeldungObjektDiferenz> dictionary2 = new Dictionary<long, SictMeldungObjektDiferenz>();
        Queue<object> objectQueue = new Queue<object>();
        if (listeWurzel != null)
        {
          foreach (object obj in listeWurzel)
            objectQueue.Enqueue(obj);
        }
        Func<object, long> funkReferenzTransportBerecneAusReferenzClr = (Func<object, long>) (objektReferenzClr =>
        {
          if (this.DictZuObjInfo == null)
            throw new ArgumentNullException("DictZuObjInfo");
          return this.DictZuObjInfo[objektReferenzClr].ObjektRefTransport;
        });
        List<object> second = new List<object>();
        List<KeyValuePair<object, object>> ziilList = new List<KeyValuePair<object, object>>();
        while (0 < objectQueue.Count)
        {
          object obj = objectQueue.Dequeue();
          if (obj != null && !dictionary1.ContainsKey(obj))
          {
            Type type = obj.GetType();
            second.Add(obj);
            bool abbildFraigaabe = mitScatescpaicer.ZuTypeBehandlung(type).AbbildFraigaabe;
            SictZuObjektDiferenzZuusctand key = (SictZuObjektDiferenzZuusctand) null;
            this.DictZuObjInfo.TryGetValue(obj, out key);
            if (key == null)
            {
              if (callbackObjektNoi != null)
                callbackObjektNoi(obj);
              long objektRefTransport = abbildFraigaabe ? this.ObjektIdentFabrik.IdentBerecne() : 0L;
              key = new SictZuObjektDiferenzZuusctand(obj, objektRefTransport, mitScatescpaicer);
              this.DictZuObjInfo[obj] = key;
            }
            dictionary1[obj] = true;
            if (abbildFraigaabe)
            {
              long verwendeteFrüühesteScritIndex;
              SictMeldungObjektDiferenz meldungObjektDiferenz = key.BerecneScritDif(scritIndex1, ältesteZuVerwendendeScritIndexScrankeMin, out IEnumerable<object> _, out IEnumerable<KeyValuePair<string, object>> _, out object[] _, out verwendeteFrüühesteScritIndex);
              val1 = Math.Min(val1, verwendeteFrüühesteScritIndex);
              SictMeldungValueOderRef.AusReferenziirteMengeReferenzClrVerwendetFüügeAinNaacList(mitScatescpaicer, obj, (IList<KeyValuePair<object, object>>) ziilList);
              foreach (KeyValuePair<object, object> keyValuePair in ziilList)
              {
                second.Add(keyValuePair.Value);
                objectQueue.Enqueue(keyValuePair.Value);
              }
              ziilList.Clear();
              if (meldungObjektDiferenz != null && !meldungObjektDiferenz.BerictLeer())
                source1[obj] = new KeyValuePair<long, KeyValuePair<SictZuObjektDiferenzZuusctand, SictMeldungObjektDiferenz>>(key.ObjektRefTransport, new KeyValuePair<SictZuObjektDiferenzZuusctand, SictMeldungObjektDiferenz>(key, meldungObjektDiferenz));
            }
          }
        }
        long[] numArray1;
        if (listeWurzel == null)
        {
          numArray1 = (long[]) null;
        }
        else
        {
          IEnumerable<long> source2 = ((IEnumerable<object>) listeWurzel).Select<object, long>((Func<object, long>) (wurzel => wurzel != null ? this.DictZuObjInfo[wurzel].ObjektRefTransport : 0L));
          numArray1 = source2 != null ? source2.ToArray<long>() : (long[]) null;
        }
        long[] numArray2 = numArray1;
        foreach (KeyValuePair<object, KeyValuePair<long, KeyValuePair<SictZuObjektDiferenzZuusctand, SictMeldungObjektDiferenz>>> keyValuePair1 in source1)
        {
          KeyValuePair<long, KeyValuePair<SictZuObjektDiferenzZuusctand, SictMeldungObjektDiferenz>> keyValuePair2 = keyValuePair1.Value;
          KeyValuePair<SictZuObjektDiferenzZuusctand, SictMeldungObjektDiferenz> keyValuePair3 = keyValuePair2.Value;
          if (keyValuePair3.Value != null)
          {
            keyValuePair2 = keyValuePair1.Value;
            keyValuePair3 = keyValuePair2.Value;
            keyValuePair3.Value.BerecneTailReferenz(funkReferenzTransportBerecneAusReferenzClr);
          }
        }
        foreach (object key in this.DictZuObjInfo.Keys.Except<object>((IEnumerable<object>) second).ToArray<object>())
          this.DictZuObjInfo.Remove(key);
        foreach (KeyValuePair<object, KeyValuePair<long, KeyValuePair<SictZuObjektDiferenzZuusctand, SictMeldungObjektDiferenz>>> keyValuePair4 in source1)
        {
          KeyValuePair<int, SictMeldungValueOderRef>[] zuMemberIdentWert = keyValuePair4.Value.Value.Value.MengeZuMemberIdentWert;
          SictZuObjektDiferenzZuusctand key1 = keyValuePair4.Value.Value.Key;
          if (key1 != null)
          {
            SictDifZuTypeMeldungInfo zuTypeMeldungInfo = (SictDifZuTypeMeldungInfo) null;
            KeyValuePair<int, SictZuTypeBehandlung> identUndBehandlung = key1.TypeIdentUndBehandlung;
            this.MengeZuTypeMeldungLezte.TryGetValue(identUndBehandlung.Key, out zuTypeMeldungInfo);
            if (zuTypeMeldungInfo == null)
            {
              zuTypeMeldungInfo = new SictDifZuTypeMeldungInfo(identUndBehandlung.Value);
              this.MengeZuTypeMeldungLezte[identUndBehandlung.Key] = zuTypeMeldungInfo;
            }
            if (!zuMemberIdentWert.IsNullOrEmpty())
            {
              foreach (KeyValuePair<int, SictMeldungValueOderRef> keyValuePair5 in zuMemberIdentWert)
              {
                int key2 = keyValuePair5.Key;
                zuTypeMeldungInfo.MengeZuMemberIdentVerwendungLezteScritIndex[key2] = scritIndex1;
              }
            }
          }
        }
        List<KeyValuePair<int, SictMeldungType>> enumerable1 = new List<KeyValuePair<int, SictMeldungType>>();
        foreach (KeyValuePair<int, SictDifZuTypeMeldungInfo> keyValuePair6 in (IEnumerable<KeyValuePair<int, SictDifZuTypeMeldungInfo>>) this.MengeZuTypeMeldungLezte)
        {
          SictDifZuTypeMeldungInfo zuTypeMeldungInfo = keyValuePair6.Value;
          string clrName = (string) null;
          SictZuTypeBehandlung typeBhandlung = zuTypeMeldungInfo.TypeBhandlung;
          long? nameLezteScritIndex = zuTypeMeldungInfo.MeldungClrNameLezteScritIndex;
          List<KeyValuePair<int, string>> keyValuePairList = new List<KeyValuePair<int, string>>();
          long? nullable1;
          long? nullable2;
          foreach (KeyValuePair<int, long> keyValuePair7 in (IEnumerable<KeyValuePair<int, long>>) zuTypeMeldungInfo.MengeZuMemberIdentVerwendungLezteScritIndex)
          {
            int MemberIdent = keyValuePair7.Key;
            IDictionary<int, long> meldungLezteScritIndex = zuTypeMeldungInfo.MengeZuMemberIdentMeldungLezteScritIndex;
            long? nullable3;
            if (meldungLezteScritIndex == null)
            {
              nullable1 = new long?();
              nullable3 = nullable1;
            }
            else
              nullable3 = new long?(meldungLezteScritIndex.FirstOrDefault<KeyValuePair<int, long>>((Func<KeyValuePair<int, long>, bool>) (kandidaat => kandidaat.Key == MemberIdent)).Value);
            long? nullable4 = nullable3;
            if (keyValuePair7.Value < scritIndex1)
            {
              if (ältesteZuVerwendendeScritIndexScrankeMin.HasValue)
              {
                long num = keyValuePair7.Value;
                nullable1 = ältesteZuVerwendendeScritIndexScrankeMin;
                long valueOrDefault = nullable1.GetValueOrDefault();
                if (num >= valueOrDefault || !nullable1.HasValue)
                {
                  nullable1 = ältesteZuVerwendendeScritIndexScrankeMin;
                  nullable2 = nullable4;
                  if (nullable1.GetValueOrDefault() < nullable2.GetValueOrDefault() && nullable1.HasValue & nullable2.HasValue)
                    continue;
                }
                else
                  continue;
              }
              else
                continue;
            }
            SictZuMemberBehandlung[] memberBehandlung1 = typeBhandlung.MengeMemberBehandlung;
            SictZuMemberBehandlung memberBehandlung2 = memberBehandlung1 != null ? ((IEnumerable<SictZuMemberBehandlung>) memberBehandlung1).FirstOrDefault<SictZuMemberBehandlung>((Func<SictZuMemberBehandlung, bool>) (kandidaat => MemberIdent == kandidaat.SictDiferenzTransportMemberIdent)) : (SictZuMemberBehandlung) null;
            if (memberBehandlung2 != null)
            {
              keyValuePairList.Add(new KeyValuePair<int, string>(MemberIdent, memberBehandlung2.HerkunftMemberName));
              zuTypeMeldungInfo.MengeZuMemberIdentMeldungLezteScritIndex[MemberIdent] = scritIndex1;
            }
          }
          int num1;
          if (nameLezteScritIndex.HasValue)
          {
            nullable2 = nameLezteScritIndex;
            nullable1 = ältesteZuVerwendendeScritIndexScrankeMin;
            num1 = nullable2.GetValueOrDefault() < nullable1.GetValueOrDefault() ? (nullable2.HasValue & nullable1.HasValue ? 1 : 0) : 0;
          }
          else
            num1 = 1;
          if (num1 != 0)
          {
            clrName = SictSumeZuTypeAgrInfo.TypeClrAssemblyQualifiedNameBerecne(typeBhandlung.ZiilType);
            zuTypeMeldungInfo.MeldungClrNameLezteScritIndex = new long?(scritIndex1);
          }
          if (clrName != null || !keyValuePairList.IsNullOrEmpty())
            enumerable1.Add(new KeyValuePair<int, SictMeldungType>(keyValuePair6.Key, new SictMeldungType(clrName, keyValuePairList != null ? keyValuePairList.ToArrayIfNotEmpty<KeyValuePair<int, string>>() : (KeyValuePair<int, string>[]) null)));
        }
        KeyValuePair<int, SictMeldungType>[] arrayIfNotEmpty = enumerable1 != null ? enumerable1.ToArrayIfNotEmpty<KeyValuePair<int, SictMeldungType>>() : (KeyValuePair<int, SictMeldungType>[]) null;
        long[] listeWurzelReferenz = numArray2;
        KeyValuePair<long, SictMeldungObjektDiferenzSeriel>[] mengeZuObjektDiferenz;
        if (source1 == null)
        {
          mengeZuObjektDiferenz = (KeyValuePair<long, SictMeldungObjektDiferenzSeriel>[]) null;
        }
        else
        {
          IEnumerable<KeyValuePair<long, SictMeldungObjektDiferenzSeriel>> enumerable2 = source1.Select<KeyValuePair<object, KeyValuePair<long, KeyValuePair<SictZuObjektDiferenzZuusctand, SictMeldungObjektDiferenz>>>, KeyValuePair<long, SictMeldungObjektDiferenzSeriel>>((Func<KeyValuePair<object, KeyValuePair<long, KeyValuePair<SictZuObjektDiferenzZuusctand, SictMeldungObjektDiferenz>>>, KeyValuePair<long, SictMeldungObjektDiferenzSeriel>>) (t =>
          {
            KeyValuePair<long, KeyValuePair<SictZuObjektDiferenzZuusctand, SictMeldungObjektDiferenz>> keyValuePair = t.Value;
            long key = keyValuePair.Key;
            keyValuePair = t.Value;
            SictMeldungObjektDiferenzSeriel objektDiferenzSeriel = keyValuePair.Value.Value.SictSeriel();
            return new KeyValuePair<long, SictMeldungObjektDiferenzSeriel>(key, objektDiferenzSeriel);
          }));
          mengeZuObjektDiferenz = enumerable2 != null ? enumerable2.ToArrayIfNotEmpty<KeyValuePair<long, SictMeldungObjektDiferenzSeriel>>() : (KeyValuePair<long, SictMeldungObjektDiferenzSeriel>[]) null;
        }
        long num2 = scritIndex1;
        long num3 = num2 + 1L;
        long? scritIndex2 = new long?(num2);
        long? verwendeteFrüühesteScritIndex1 = new long?(val1);
        return new SictZuNezSictDiferenzScritAbbild(arrayIfNotEmpty, listeWurzelReferenz, mengeZuObjektDiferenz, scritIndex2, verwendeteFrüühesteScritIndex1);
      }
    }

    public static IEnumerable<object> ZuRefNezMengeClrTypeInstanceReferenziirt(
      object[] mengeWurzelRefClr,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer mengeTypeBehandlungRictliinieMitScatescpaicer)
    {
      List<object> Menge = new List<object>();
      Action<object> callback = (Action<object>) (instanceRefClr => Menge.Add(instanceRefClr));
      SictRefNezDiferenz.ZuRefNezMengeClrTypeInstanceCallback(mengeWurzelRefClr, mengeTypeBehandlungRictliinieMitScatescpaicer, callback);
      return (IEnumerable<object>) Menge;
    }

    public static IEnumerable<KeyValuePair<Type, int>> ZuRefNezMengeClrTypeInstanceAnzaalBerecne(
      object[] mengeWurzelRefClr,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer mengeTypeBehandlungRictliinieMitScatescpaicer)
    {
      if (mengeWurzelRefClr == null || mengeTypeBehandlungRictliinieMitScatescpaicer == null)
        return (IEnumerable<KeyValuePair<Type, int>>) null;
      Dictionary<object, bool> dictionary = new Dictionary<object, bool>();
      Dictionary<Type, int> DictZuTypeMengeInstanceAnzaal = new Dictionary<Type, int>();
      Action<object> callback = (Action<object>) (instanceRefClr =>
      {
        Type type = instanceRefClr.GetType();
        int num;
        if (!DictZuTypeMengeInstanceAnzaal.TryGetValue(type, out num))
          num = 0;
        DictZuTypeMengeInstanceAnzaal[type] = 1 + num;
      });
      SictRefNezDiferenz.ZuRefNezMengeClrTypeInstanceCallback(mengeWurzelRefClr, mengeTypeBehandlungRictliinieMitScatescpaicer, callback);
      return (IEnumerable<KeyValuePair<Type, int>>) DictZuTypeMengeInstanceAnzaal;
    }

    public static void ZuRefNezMengeClrTypeInstanceCallback(
      object[] mengeWurzelRefClr,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer mengeTypeBehandlungRictliinieMitScatescpaicer,
      Action<object> callback)
    {
      IEnumerable<object> objects = mengeWurzelRefClr.EnumMengeRefAusNezAusMengeWurzel(mengeTypeBehandlungRictliinieMitScatescpaicer);
      if (objects == null)
        return;
      foreach (object obj in objects)
        callback(obj);
    }

    public static IEnumerable<KeyValuePair<object, object>[]> ZuTypeMengeInstanzKürzestePfaadVonWurzelBerecne(
      Type pfaadEndeType,
      object[] mengeWurzelRefClr,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer mengeTypeBehandlungRictliinieMitScatescpaicer)
    {
      if (mengeWurzelRefClr == null || mengeTypeBehandlungRictliinieMitScatescpaicer == null)
        return (IEnumerable<KeyValuePair<object, object>[]>) null;
      Queue<KeyValuePair<object, object>[]> source = new Queue<KeyValuePair<object, object>[]>();
      List<KeyValuePair<object, object>[]> list = source.ToList<KeyValuePair<object, object>[]>();
      foreach (object obj in mengeWurzelRefClr)
      {
        if (obj != null)
          source.Enqueue(new KeyValuePair<object, object>[1]
          {
            new KeyValuePair<object, object>((object) null, obj)
          });
      }
      Dictionary<object, bool> dictionary = new Dictionary<object, bool>();
      List<KeyValuePair<object, object>> ziilList = new List<KeyValuePair<object, object>>();
      while (0 < source.Count)
      {
        KeyValuePair<object, object>[] keyValuePairArray = source.Dequeue();
        object obj = ((IEnumerable<KeyValuePair<object, object>>) keyValuePairArray).Last<KeyValuePair<object, object>>().Value;
        if (!dictionary.ContainsKey(obj))
        {
          dictionary[obj] = true;
          if (obj.GetType() == pfaadEndeType)
            list.Add(keyValuePairArray);
          SictMeldungValueOderRef.AusReferenziirteMengeReferenzClrVerwendetFüügeAinNaacList(mengeTypeBehandlungRictliinieMitScatescpaicer, obj, (IList<KeyValuePair<object, object>>) ziilList);
          foreach (KeyValuePair<object, object> keyValuePair in ziilList)
          {
            if (keyValuePair.Value != null)
            {
              KeyValuePair<object, object>[] destinationArray = new KeyValuePair<object, object>[keyValuePairArray.Length + 1];
              Array.Copy((Array) keyValuePairArray, 0, (Array) destinationArray, 0, keyValuePairArray.Length);
              destinationArray[keyValuePairArray.Length] = keyValuePair;
              source.Enqueue(destinationArray);
            }
          }
          ziilList.Clear();
        }
      }
      return (IEnumerable<KeyValuePair<object, object>[]>) list;
    }
  }
}
