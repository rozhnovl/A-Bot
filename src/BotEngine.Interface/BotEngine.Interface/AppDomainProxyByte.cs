using Bib3;
using Bib3.RefNezDiferenz;
using Bib3.RefNezDiferenz.NewtonsoftJson;
using BotEngine.Common;
using System.Collections;
using System.Reflection;

namespace BotEngine.Interface
{
	public class AppDomainProxyByte : MarshalByRefObject
	{
		private readonly object Lock = new object();

		private static AppDomainProxyByte StaticProxy;

		public static readonly SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer ServerKomRictliinieScatescpaicer = new SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer(ServerKomRictliinieKonstrukt());

		public static readonly List<IInterfaceKomponente> MengeKomponente = new List<IInterfaceKomponente>();

		private long NaacServerBerictLezteZait = 0L;

		private SictRefNezSume VonServerSictSume = new SictRefNezSume(ServerKomRictliinieScatescpaicer);

		private SictRefNezDiferenz NaacServerSictDif = new SictRefNezDiferenz(ServerKomRictliinieScatescpaicer);

		public const int NaacServerSictDifWiiderhoolungDistanz = 30;

		private VonServerNaacInterfaceZuusctand VonServerZuusctand;

		private readonly VonInterfaceNaacServerZuusctand NaacServerZuusctand = new VonInterfaceNaacServerZuusctand();

		private readonly List<KeyValuePair<long, object>> ListeFunkAusgefüürtAinmaal = new List<KeyValuePair<long, object>>();

		private string TempDebugBreakTypeName = null;

		private readonly Queue<byte[]> AusgangSclangeNaacServer = new Queue<byte[]>();

		private static long GetTimeMilli => TimesourceConfig.StaticConfig.TimeContinuousMilli;

		public static SictMengeTypeBehandlungRictliinie ServerKomRictliinieKonstrukt()
		{
			return SictMengeTypeBehandlungRictliinieNewtonsoftJson.KonstruktMengeTypeBehandlungRictliinie();
		}

		public static object InterfaceKomponenteKonstrukt(Type type, object[] args = null, bool konstruktWenBeraitsVorhande = false)
		{
			if (null == type)
			{
				return null;
			}
			IInterfaceKomponente interfaceKomponente = MengeKomponente?.FirstOrDefault((IInterfaceKomponente komponente) => komponente?.GetType() == type);
			if (interfaceKomponente != null && !konstruktWenBeraitsVorhande)
			{
				return null;
			}
			object obj = Activator.CreateInstance(type, args);
			MengeKomponente.Add(obj as IInterfaceKomponente);
			return null;
		}

		public static object InterfaceKomponenteKonstrukt(string assemblyName, string typeName, object[] args = null, bool konstruktWenBeraitsVorhande = false)
		{
			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()?.FirstOrDefault((Assembly kandidaat) => string.Equals(kandidaat?.GetName().FullName, assemblyName)) ?? AppDomain.CurrentDomain.GetAssemblies()?.FirstOrDefault((Assembly kandidaat) => string.Equals(kandidaat?.GetName().Name, assemblyName));
			Type type = assembly.GetTypes()?.FirstOrDefault((Type kandidaat) => string.Equals(kandidaat.AssemblyQualifiedName, typeName)) ?? assembly.GetTypes()?.FirstOrDefault((Type kandidaat) => string.Equals(kandidaat.FullName, typeName));
			return InterfaceKomponenteKonstrukt(type, args, konstruktWenBeraitsVorhande);
		}

		public AppDomainProxyByte()
		{
			StaticProxy = this;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}

		private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()?.FirstOrDefault((Assembly kandidaat) => string.Equals(kandidaat.GetName().FullName, args?.Name));
			if (null != assembly)
			{
				return assembly;
			}
			return AppDomain.CurrentDomain.GetAssemblies()?.FirstOrDefault((Assembly kandidaat) => string.Equals(kandidaat.GetName().Name, args?.Name));
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception exception = e.ExceptionObject as Exception;
			NaacServerZuusctand.ExceptionLezte = new WertZuZaitpunktStruct<string>(exception.SictString(), GetTimeMilli);
		}
	}
}
