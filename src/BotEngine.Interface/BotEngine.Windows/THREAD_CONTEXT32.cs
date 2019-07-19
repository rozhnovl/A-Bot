namespace BotEngine.Windows
{
	public struct THREAD_CONTEXT32
	{
		public THREAD_CONTEXT_FLAG ContextFlags;

		public uint Dr0;

		public uint Dr1;

		public uint Dr2;

		public uint Dr3;

		public uint Dr6;

		public uint Dr7;

		public FLOATING_SAVE_AREA FloatSave;

		public uint SegGs;

		public uint SegFs;

		public uint SegEs;

		public uint SegDs;

		public uint Edi;

		public uint Esi;

		public uint Ebx;

		public uint Edx;

		public uint Ecx;

		public uint Eax;

		public uint Ebp;

		public uint Eip;

		public uint SegCs;

		public uint EFlags;

		public uint Esp;

		public uint SegSs;
	}
}
