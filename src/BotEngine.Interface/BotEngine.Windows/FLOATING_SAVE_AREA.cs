using System.Runtime.InteropServices;

namespace BotEngine.Windows
{
	[StructLayout(LayoutKind.Explicit, Size = 112)]
	public struct FLOATING_SAVE_AREA
	{
		[FieldOffset(0)]
		private uint ControlWord;

		[FieldOffset(4)]
		private uint StatusWord;

		[FieldOffset(8)]
		private uint TagWord;

		[FieldOffset(12)]
		private uint ErrorOffset;

		[FieldOffset(16)]
		private uint ErrorSelector;

		[FieldOffset(20)]
		private uint DataOffset;

		[FieldOffset(24)]
		private uint DataSelector;
	}
}
