// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.MemberTransitNotifier
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Fasterflect;
using System.Collections.Generic;

namespace Bib3.RefNezDiferenz
{
  public class MemberTransitNotifier
  {
    public readonly object Observed;
    private readonly SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer Policy;
    private readonly IDictionary<string, object> ValuePrevFromMemberName = (IDictionary<string, object>) new Dictionary<string, object>();

    public MemberTransitNotifier(
      object observed,
      SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer policy)
    {
      this.Observed = observed;
      this.Policy = policy;
    }

    private bool Equal(object prev, object current) => object.Equals(prev, current);

    public IEnumerable<MemberTransitNotification> Propagate()
    {
      SictZuTypeBehandlung TypePolicy = this.Policy?.ZuTypeBehandlung(this.Observed?.GetType());
      foreach (SictZuMemberBehandlung memberBehandlung in ((IEnumerable<SictZuMemberBehandlung>) TypePolicy?.MengeMemberBehandlung).EmptyIfNull<SictZuMemberBehandlung>())
      {
        SictZuMemberBehandlung MemberPolicy = memberBehandlung;
        string MemberName = MemberPolicy.HerkunftMemberName;
        MemberGetter Getter = MemberPolicy.HerkunftTypeMemberGetter;
        if (Getter != null)
        {
          object ValueCurrent = Getter.Invoke(this.Observed);
          object ValuePrev;
          bool ValuePrevExisting = this.ValuePrevFromMemberName.TryGetValue(MemberName, out ValuePrev);
          this.ValuePrevFromMemberName[MemberName] = ValueCurrent;
          if (!ValuePrevExisting || !this.Equal(ValuePrev, ValueCurrent))
            yield return new MemberTransitNotification(MemberName, ValueCurrent, ValuePrev);
          MemberName = (string) null;
          Getter = (MemberGetter) null;
          ValueCurrent = (object) null;
          ValuePrev = (object) null;
          MemberPolicy = (SictZuMemberBehandlung) null;
        }
      }
    }
  }
}
