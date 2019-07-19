using Bib3;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace BotEngine.Interface.Windows
{
	public static class WMIExtension
	{
		public static IEnumerable<ProcessInfoFromWMI> ReadSetProcessInfo()
		{
			using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\cimv2", "SELECT ProcessId, CommandLine FROM Win32_Process"))
			{
				foreach (ManagementBaseObject process in (searcher.Get()?.OfType<ManagementBaseObject>()).EmptyIfNull())
				{
					string commandline = process["CommandLine"] as string;
					int? processId = (process["ProcessId"]?.ToString())?.TryParseInt();
					yield return new ProcessInfoFromWMI
					{
						ProcessId = processId,
						Commandline = commandline
					};
				}
			}
		}
	}
}
