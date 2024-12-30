using System.Runtime.InteropServices;

namespace Optimat.EveOnline;

[StructLayout(LayoutKind.Explicit)]
public struct SictPyObjAusrictCPython32
{
	[FieldOffset(0)]
	public uint RefCount;

	[FieldOffset(4)]
	public uint RefType;
}
