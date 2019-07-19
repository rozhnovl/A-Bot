using System;
using System.Runtime.InteropServices;

namespace BotEngine.Windows
{
	[StructLayout(LayoutKind.Explicit, Size = 80)]
	public struct EXCEPTION_RECORD32
	{
		[FieldOffset(0)]
		public EXCEPTION_CODE ExceptionCode;

		[FieldOffset(4)]
		public uint ExceptionFlags;

		[FieldOffset(8)]
		public IntPtr ExceptionRecord;

		[FieldOffset(12)]
		public IntPtr ExceptionAddress;

		[FieldOffset(16)]
		public uint NumberParameters;

		[FieldOffset(20)]
		public uint Argument0;

		[FieldOffset(24)]
		public uint Argument1;

		[FieldOffset(28)]
		public uint Argument2;

		[FieldOffset(32)]
		public uint Argument3;
	}
}
