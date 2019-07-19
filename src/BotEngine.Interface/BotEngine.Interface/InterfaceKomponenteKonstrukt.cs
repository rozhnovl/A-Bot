using Bib3;

namespace BotEngine.Interface
{
	public class InterfaceKomponenteKonstrukt : IFuncVoid
	{
		private readonly string AssemblyName;

		private readonly string TypeName;

		public readonly object[] args;

		public readonly bool KonstruktWenBeraitsVorhande;

		public InterfaceKomponenteKonstrukt()
		{
		}

		public InterfaceKomponenteKonstrukt(string assemblyName, string typeName, object[] args = null)
		{
			AssemblyName = assemblyName;
			TypeName = typeName;
			this.args = args;
		}

		public object FüüreAus()
		{
			return AppDomainProxyByte.InterfaceKomponenteKonstrukt(AssemblyName, TypeName, args, KonstruktWenBeraitsVorhande);
		}
	}
}
