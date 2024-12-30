using System.Reflection;
using Fasterflect;

namespace Optimat.EveOnline;

public class SictZuMemberAusDictEntryInfo
{
	public readonly MemberInfo MemberInfo;

	public readonly MemberSetter Setter;

	public readonly MemberGetter Getter;

	public SictZuMemberAusDictEntryInfo(MemberInfo MemberInfo, MemberSetter Setter, MemberGetter Getter)
	{
		this.MemberInfo = MemberInfo;
		this.Setter = Setter;
		this.Getter = Getter;
	}
}
