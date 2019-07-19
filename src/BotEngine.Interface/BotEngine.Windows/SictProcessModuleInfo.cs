using System.Diagnostics;

namespace BotEngine.Windows
{
	public struct SictProcessModuleInfo
	{
		public long BaseAddress;

		public long ModuleMemorySize;

		public string ModuleName;

		public string FileName;

		public SictProcessModuleInfo(long baseAddress, long moduleMemorySize, string moduleName, string fileName)
		{
			BaseAddress = baseAddress;
			ModuleMemorySize = moduleMemorySize;
			ModuleName = moduleName;
			FileName = fileName;
		}

		public SictProcessModuleInfo(ProcessModule module)
		{
			this = new SictProcessModuleInfo(module.BaseAddress.ToInt64(), module.ModuleMemorySize, module.ModuleName, module.FileName);
		}
	}
}
