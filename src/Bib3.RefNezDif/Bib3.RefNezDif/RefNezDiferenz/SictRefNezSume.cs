// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictRefNezSume
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib3.RefNezDiferenz
{
  public class SictRefNezSume
  {
    private readonly SictDiferenzSictParam SictParam;
    private readonly SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer TypeBehandlungRictliinieMitScatescpaicer;
    private readonly Dictionary<long, SictZuObjektDiferenzZuusctand> DictVonRefTransportZuObjektInfo = new Dictionary<long, SictZuObjektDiferenzZuusctand>();
    private readonly SictScatenscpaicerDict<Type, SictRefNezSumeProfileZuType> DictProfileZuType = new SictScatenscpaicerDict<Type, SictRefNezSumeProfileZuType>();
    private readonly IDictionary<int, SictSumeZuTypeAgrInfo> DictZuTypeTransportIdentAgregatioonInfo = (IDictionary<int, SictSumeZuTypeAgrInfo>) new Dictionary<int, SictSumeZuTypeAgrInfo>();

    public IReadOnlyDictionary<Type, SictRefNezSumeProfileZuType> DictProfileZuTypeBerecne() => (IReadOnlyDictionary<Type, SictRefNezSumeProfileZuType>) this.DictProfileZuType;

    public KeyValuePair<long?, SictRefNezScritSumeErgeebnis> ScritLezteErgeebnis { private set; get; }

    public long? SelbstScritLezteIndex { private set; get; }

    public long MengeObjektAnzaalAngeleegt { private set; get; }

    public long? VonEmitentMeldungScritLezteIndex => this.ScritLezteErgeebnis.Key;

    public long? VonEmitentMeldungScritFrüühesteIndex { private set; get; }

    public SictRefNezScritSumeErgeebnis BerecneScritSumeListeWurzelRefClrUndErfolg(
      SictZuNezSictDiferenzScritAbbild nezDiferenzScrit,
      bool ziilMemberExistentNictIgnoriire = false)
    {
      if (nezDiferenzScrit == null)
        return (SictRefNezScritSumeErgeebnis) null;
      long? selbstScritLezteIndex = this.SelbstScritLezteIndex;
      long num1 = 1;
      long? nullable = selbstScritLezteIndex.HasValue ? new long?(selbstScritLezteIndex.GetValueOrDefault() + num1) : new long?();
      long scritIndex1 = nullable ?? 0L;
      long? scritIndex2 = nezDiferenzScrit.ScritIndex;
      int num2;
      if (this.ScritLezteErgeebnis.Value == null)
      {
        nullable = this.VonEmitentMeldungScritFrüühesteIndex;
        num2 = !nullable.HasValue ? 1 : 0;
      }
      else
        num2 = 0;
      if (num2 != 0)
        this.VonEmitentMeldungScritFrüühesteIndex = scritIndex2;
      SictRefNezScritSumeErgeebnis scritSumeErgeebnis = (SictRefNezScritSumeErgeebnis) null;
      List<long> longList = new List<long>();
      try
      {
        KeyValuePair<int, SictMeldungType>[] mengeZuTypeMeldung = nezDiferenzScrit.MengeZuTypeMeldung;
        long[] listeWurzelReferenz = nezDiferenzScrit.ListeWurzelReferenz;
        KeyValuePair<long, SictMeldungObjektDiferenzSeriel>[] referenzDiferenz = nezDiferenzScrit.MengeZuReferenzDiferenz;
        Func<string, SictZuTypeBehandlung> callbackZuTypeClrNameTypeBehandlung = (Func<string, SictZuTypeBehandlung>) (typeClrName => SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer.ZuTypeBehandlung(SictSumeZuTypeAgrInfo.TypeAusTypeClrAssemblyQualifiedName(typeClrName), this.TypeBehandlungRictliinieMitScatescpaicer));
        if (mengeZuTypeMeldung != null)
        {
          foreach (KeyValuePair<int, SictMeldungType> keyValuePair in mengeZuTypeMeldung)
          {
            int key = keyValuePair.Key;
            SictSumeZuTypeAgrInfo sumeZuTypeAgrInfo = (SictSumeZuTypeAgrInfo) null;
            this.DictZuTypeTransportIdentAgregatioonInfo.TryGetValue(key, out sumeZuTypeAgrInfo);
            if (sumeZuTypeAgrInfo == null)
            {
              sumeZuTypeAgrInfo = new SictSumeZuTypeAgrInfo(key);
              this.DictZuTypeTransportIdentAgregatioonInfo[key] = sumeZuTypeAgrInfo;
            }
            sumeZuTypeAgrInfo.AingangAusScritInfo(keyValuePair.Value, callbackZuTypeClrNameTypeBehandlung);
          }
        }
        if (listeWurzelReferenz == null)
          return (SictRefNezScritSumeErgeebnis) null;
        bool VolsctändigTailReferenzNict = false;
        bool hasValue = scritIndex2.HasValue;
        Func<long, object> funkReferenzClrBerecneAusReferenzTransport = (Func<long, object>) (objektReferenzTransport =>
        {
          if (objektReferenzTransport == 0L)
            return (object) null;
          if (this.DictVonRefTransportZuObjektInfo == null)
            throw new ArgumentNullException("DictVonRefTransportZuObjektInfo");
          SictZuObjektDiferenzZuusctand diferenzZuusctand;
          if (this.DictVonRefTransportZuObjektInfo.TryGetValue(objektReferenzTransport, out diferenzZuusctand))
            return diferenzZuusctand.ObjektKopiiRefClr;
          VolsctändigTailReferenzNict = true;
          return (object) null;
        });
        if (referenzDiferenz != null)
        {
          foreach (KeyValuePair<long, SictMeldungObjektDiferenzSeriel> keyValuePair in referenzDiferenz)
          {
            long key = keyValuePair.Key;
            SictMeldungObjektDiferenzSeriel referenziirteDiferenzSictSeriel = keyValuePair.Value;
            if (referenziirteDiferenzSictSeriel != null)
            {
              if (key < 1L)
                throw new ArgumentOutOfRangeException("ObjektRefTransport < 1");
              int type = referenziirteDiferenzSictSeriel.Type;
              longList.Add(key);
              SictZuObjektDiferenzZuusctand diferenzZuusctand = (SictZuObjektDiferenzZuusctand) null;
              this.DictVonRefTransportZuObjektInfo.TryGetValue(key, out diferenzZuusctand);
              if (diferenzZuusctand == null)
              {
                ++this.MengeObjektAnzaalAngeleegt;
                this.DictVonRefTransportZuObjektInfo[key] = diferenzZuusctand = new SictZuObjektDiferenzZuusctand((object) null, key, this.TypeBehandlungRictliinieMitScatescpaicer);
              }
              if (type != 0)
                diferenzZuusctand.TypeTransportIdent = new int?(type);
              if (type != 0)
              {
                SictSumeZuTypeAgrInfo sumeZuTypeAgrInfo = (SictSumeZuTypeAgrInfo) null;
                this.DictZuTypeTransportIdentAgregatioonInfo.TryGetValue(type, out sumeZuTypeAgrInfo);
                if (sumeZuTypeAgrInfo != null)
                  diferenzZuusctand.SictSumeZuTypeAgregatioonInfo = sumeZuTypeAgrInfo;
              }
              diferenzZuusctand.BerecneScritSumeTailInstanz(scritIndex1, referenziirteDiferenzSictSeriel.KonstruktStructClr(), ziilMemberExistentNictIgnoriire);
            }
          }
        }
        if (referenzDiferenz != null)
        {
          foreach (KeyValuePair<long, SictMeldungObjektDiferenzSeriel> keyValuePair in referenzDiferenz)
          {
            long key = keyValuePair.Key;
            SictMeldungObjektDiferenzSeriel referenziirteDiferenzSictSeriel = keyValuePair.Value;
            if (referenziirteDiferenzSictSeriel != null)
            {
              if (key < 1L)
                throw new ArgumentOutOfRangeException("ObjektRefTransport < 1");
              SictZuObjektDiferenzZuusctand diferenzZuusctand = (SictZuObjektDiferenzZuusctand) null;
              this.DictVonRefTransportZuObjektInfo.TryGetValue(key, out diferenzZuusctand);
              diferenzZuusctand.BerecneScritSumeTailMember(scritIndex1, referenziirteDiferenzSictSeriel.KonstruktStructClr(), funkReferenzClrBerecneAusReferenzTransport, ziilMemberExistentNictIgnoriire);
            }
          }
        }
        KeyValuePair<object, bool>[] keyValuePairArray = new KeyValuePair<object, bool>[listeWurzelReferenz.Length];
        KeyValuePair<long, object>[] listeWurzelRefTransportUndRefClr = new KeyValuePair<long, object>[listeWurzelReferenz.Length];
        for (int index = 0; index < listeWurzelReferenz.Length; ++index)
        {
          long key = listeWurzelReferenz[index];
          object obj = funkReferenzClrBerecneAusReferenzTransport(key);
          listeWurzelRefTransportUndRefClr[index] = new KeyValuePair<long, object>(key, obj);
        }
        bool volsctändig = hasValue && !VolsctändigTailReferenzNict;
        scritSumeErgeebnis = new SictRefNezScritSumeErgeebnis(listeWurzelRefTransportUndRefClr, hasValue, volsctändig);
        return scritSumeErgeebnis;
      }
      finally
      {
        this.SelbstScritLezteIndex = new long?(scritIndex1);
        this.ScritLezteErgeebnis = new KeyValuePair<long?, SictRefNezScritSumeErgeebnis>(nezDiferenzScrit.ScritIndex, scritSumeErgeebnis);
        foreach (KeyValuePair<long, SictZuObjektDiferenzZuusctand> keyValuePair in this.DictVonRefTransportZuObjektInfo)
        {
          SictZuObjektDiferenzZuusctand diferenzZuusctand = keyValuePair.Value;
          if (diferenzZuusctand != null)
          {
            Type ziilType = diferenzZuusctand.ZiilType;
            if (!((Type) null == ziilType))
              ++this.DictProfileZuType.ValueFürKey(ziilType, (Func<Type, SictRefNezSumeProfileZuType>) (t => new SictRefNezSumeProfileZuType())).ListeScritObjektAnzaal;
          }
        }
      }
    }

    public void MengeObjektInfoEntferneNitMeerReferenziirte()
    {
      Dictionary<long, SictZuObjektDiferenzZuusctand> transportZuObjektInfo = this.DictVonRefTransportZuObjektInfo;
      if (transportZuObjektInfo == null)
        return;
      Queue<long> QueueObjektRefTransport = new Queue<long>();
      KeyValuePair<long?, SictRefNezScritSumeErgeebnis> scritLezteErgeebnis = this.ScritLezteErgeebnis;
      if (scritLezteErgeebnis.Value != null)
      {
        KeyValuePair<long, object>[] transportUndRefClr = scritLezteErgeebnis.Value.ListeWurzelRefTransportUndRefClr;
        if (transportUndRefClr != null)
          ((IEnumerable<KeyValuePair<long, object>>) transportUndRefClr).ForEach<KeyValuePair<long, object>>((Action<KeyValuePair<long, object>>) (wurzelRefTransportUndRefClr => QueueObjektRefTransport.Enqueue(wurzelRefTransportUndRefClr.Key)));
      }
      Dictionary<long, bool> dictionary = new Dictionary<long, bool>();
      List<KeyValuePair<object, object>> ziilList = new List<KeyValuePair<object, object>>();
      while (0 < QueueObjektRefTransport.Count)
      {
        long key = QueueObjektRefTransport.Dequeue();
        if (!dictionary.ContainsKey(key))
        {
          dictionary[key] = true;
          SictZuObjektDiferenzZuusctand diferenzZuusctand;
          if (transportZuObjektInfo.TryGetValue(key, out diferenzZuusctand))
          {
            object objektKopiiRefClr = diferenzZuusctand.ObjektKopiiRefClr;
            if (objektKopiiRefClr != null)
            {
              SictMeldungValueOderRef.AusReferenziirteMengeReferenzClrVerwendetFüügeAinNaacList(this.TypeBehandlungRictliinieMitScatescpaicer, objektKopiiRefClr, (IList<KeyValuePair<object, object>>) ziilList);
              foreach (KeyValuePair<object, object> keyValuePair1 in ziilList)
              {
                KeyValuePair<object, object> ObjektVerwendetReferenzClrInAst = keyValuePair1;
                if (ObjektVerwendetReferenzClrInAst.Value != null)
                {
                  KeyValuePair<long, SictZuObjektDiferenzZuusctand> keyValuePair2 = transportZuObjektInfo.FirstOrDefault<KeyValuePair<long, SictZuObjektDiferenzZuusctand>>((Func<KeyValuePair<long, SictZuObjektDiferenzZuusctand>, bool>) (kandidaat => kandidaat.Value.ObjektKopiiRefClr == ObjektVerwendetReferenzClrInAst.Value));
                  if (keyValuePair2.Value != null)
                    QueueObjektRefTransport.Enqueue(keyValuePair2.Key);
                }
              }
              ziilList.Clear();
            }
          }
        }
      }
      foreach (long key in transportZuObjektInfo.Keys.Except<long>((IEnumerable<long>) dictionary.Keys).ToArray<long>())
        transportZuObjektInfo.Remove(key);
    }

    public void MengeObjektInfoEntferneÄltere(long scrankeScritIndex)
    {
      long? nullable = this.SelbstScritLezteIndex;
      scrankeScritIndex = Math.Min(nullable ?? 0L, scrankeScritIndex);
      Dictionary<long, SictZuObjektDiferenzZuusctand> transportZuObjektInfo = this.DictVonRefTransportZuObjektInfo;
      if (transportZuObjektInfo == null)
        return;
      List<long> second = new List<long>();
      foreach (KeyValuePair<long, SictZuObjektDiferenzZuusctand> keyValuePair in transportZuObjektInfo)
      {
        SictZuObjektDiferenzZuusctand diferenzZuusctand = keyValuePair.Value;
        if (diferenzZuusctand != null)
        {
          long num = scrankeScritIndex;
          nullable = diferenzZuusctand.MeldungLezteScritIndex;
          long valueOrDefault = nullable.GetValueOrDefault();
          if ((num <= valueOrDefault ? (nullable.HasValue ? 1 : 0) : 0) != 0)
            second.Add(keyValuePair.Key);
        }
      }
      foreach (long key in transportZuObjektInfo.Keys.Except<long>((IEnumerable<long>) second).ToArray<long>())
        transportZuObjektInfo.Remove(key);
    }

    public static T ObjektKopiiKonstrukt<T>(T zuKopiire, SictDiferenzSictParam sictParam)
    {
      SictRefNezDiferenz sictRefNezDiferenz = new SictRefNezDiferenz(sictParam);
      return (T) ((IEnumerable<object>) new SictRefNezSume(sictParam).BerecneScritSumeListeWurzelRefClrUndErfolg(sictRefNezDiferenz.BerecneScritDif(new long?(0L), new object[1]
      {
        (object) zuKopiire
      })).ListeWurzelRefClr).FirstOrDefault<object>();
    }

    public static T ObjektKopiiKonstrukt<T>(
      T zuKopiire,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer)
    {
      return SictRefNezSume.ObjektKopiiKonstrukt<T>(zuKopiire, new SictDiferenzSictParam(rictliinieMitScatescpaicer));
    }

    public SictRefNezSume(SictDiferenzSictParam sictParam)
    {
      this.SictParam = sictParam;
      this.TypeBehandlungRictliinieMitScatescpaicer = (sictParam == null ? (SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer) null : sictParam.TypeBehandlungRictliinieMitScatescpaicer) ?? new SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer((SictMengeTypeBehandlungRictliinie) null);
    }

    public SictRefNezSume(
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer)
      : this(new SictDiferenzSictParam(rictliinieMitScatescpaicer))
    {
    }
  }
}
