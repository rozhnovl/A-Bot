using System.Runtime.InteropServices;

namespace Optimat.EveOnline;

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct SictPyDictEntry32
{
	public const int EntryListeOktetAnzaal = 12;

	[FieldOffset(0)]
	public uint hash;

	[FieldOffset(4)]
	public uint ReferenzKey;

	[FieldOffset(8)]
	public uint ReferenzValue;
}
