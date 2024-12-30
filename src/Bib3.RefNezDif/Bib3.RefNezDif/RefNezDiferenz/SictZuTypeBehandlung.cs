// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.SictZuTypeBehandlung
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;

namespace Bib3.RefNezDiferenz
{
  public class SictZuTypeBehandlung
  {
    public readonly Type HerkunftType;
    public readonly Type ZiilType;
    public readonly bool AbbildFraigaabe;
    public readonly bool BehandlungAlsAtom;
    public readonly bool BehandlungAlsReferenz;
    public readonly bool ErfordertKopiiRekurs;
    public readonly bool BehandlungAlsCollection;
    public readonly Type CollectionElementType;
    public readonly bool CollectionElementTypeErfordertKopiiRekurs;
    public readonly SictZuTypeBehandlung.CollectionClearDelegate CollectionDelegateClear;
    public readonly SictZuTypeBehandlung.CollectionElementFüügeAinDelegate CollectionDelegateElementFüügeAin;
    public readonly SictZuMemberBehandlung[] MengeMemberBehandlung;
    public readonly bool AlsListeByte;

    public SictZuTypeBehandlung(
      Type herkunftType,
      Type ziilType,
      bool abbildFraigaabe,
      bool behandlungAlsAtom,
      bool behandlungAlsReferenz,
      bool erfordertKopiiRekurs,
      bool behandlungAlsCollection,
      Type collectionElementType,
      bool collectionElementTypeErfordertKopiiRekurs,
      SictZuTypeBehandlung.CollectionClearDelegate collectionDelegateClear,
      SictZuTypeBehandlung.CollectionElementFüügeAinDelegate collectionDelegateElementFüügeAin,
      SictZuMemberBehandlung[] mengeMemberBehandlung,
      bool alsListeByte)
    {
      this.HerkunftType = herkunftType;
      this.ZiilType = ziilType;
      this.AbbildFraigaabe = abbildFraigaabe;
      this.BehandlungAlsAtom = behandlungAlsAtom;
      this.BehandlungAlsReferenz = behandlungAlsReferenz;
      this.ErfordertKopiiRekurs = erfordertKopiiRekurs;
      this.BehandlungAlsCollection = behandlungAlsCollection;
      this.CollectionElementType = collectionElementType;
      this.CollectionElementTypeErfordertKopiiRekurs = collectionElementTypeErfordertKopiiRekurs;
      this.CollectionDelegateClear = collectionDelegateClear;
      this.CollectionDelegateElementFüügeAin = collectionDelegateElementFüügeAin;
      this.MengeMemberBehandlung = mengeMemberBehandlung;
      this.AlsListeByte = alsListeByte;
    }

    public delegate void CollectionElementFüügeAinDelegate(object collection, object element);

    public delegate void CollectionClearDelegate(object collection);
  }
}
