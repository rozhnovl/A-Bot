using Bib3;
using Bib3.AppDomain;
using Bib3.RefNezDiferenz;
using Bib3.RefNezDiferenz.NewtonsoftJson;
using BotEngine.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BotEngine.Interface
{
	public class AppDomainProxyByte : MarshalByRefObject, IAppDomainProxyByte
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

		public static void BerictAinfacStatic(string meldung)
		{
			StaticProxy?.NaacServerZuusctand?.BerictAinfacSclange.Enqueue(new WertZuZaitpunktStruct<string>(meldung, Glob.StopwatchZaitMiliSictInt()));
			StaticProxy?.NaacServerZuusctand?.BerictAinfacSclange?.ListeKürzeBegin(4);
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

		public override object InitializeLifetimeService()
		{
			throw new NotImplementedException();
			/*ILease lease = (ILease)base.InitializeLifetimeService();
			if (lease.CurrentState == LeaseState.Initial)
			{
				lease.InitialLeaseTime = TimeSpan.FromMinutes(60.0);
				lease.SponsorshipTimeout = TimeSpan.FromMinutes(60.0);
				lease.RenewOnCallTime = TimeSpan.FromMinutes(60.0);
			}
			return lease;*/
		}

		private InterfaceProxyMessage NaacKonsumentAusgangSctrukt()
		{
			FromInterfaceProxyToConsumerMessage[] enumerable = MengeKomponente?.Select((IInterfaceKomponente komponente) => komponente?.NaacKonsumentAusgang())?.ToArray();
			byte[][] consumer = enumerable.WhereNotDefault()?.Select((FromInterfaceProxyToConsumerMessage ausgangClient) => ausgangClient.ProtobufSerialize(1048576))?.ToArray();
			return new InterfaceProxyMessage(null, consumer);
		}

		private byte[] NaacKonsumentAusgang()
		{
			return NaacKonsumentAusgangSctrukt().ProtobufSerialize(1048576);
		}

		private void VonServerAingangSctrukt(InterfaceProxyMessage aingangSctrukt)
		{
			(aingangSctrukt?.Server?.Select((byte[] vonServerSeriel) => vonServerSeriel?.DeserializeFromUtf8<SictZuNezSictDiferenzScritAbbild>())?.ToArrayIfNotEmpty())?.ForEach(AingangVonServer);
		}

		private void VonKonsumentAingang(byte[] naacrict)
		{
			naacrict.ProtobufDeserialize<InterfaceProxyMessage>()?.Consumer?.ForEach(delegate(byte[] consumerMessageSeriel)
			{
				VonKonsumentAingang(consumerMessageSeriel.ProtobufDeserialize<FromConsumerToInterfaceProxyMessage>());
			});
		}

		private void VonKonsumentAingang(FromConsumerToInterfaceProxyMessage naacrict)
		{
			MengeKomponente?.ForEach(delegate(IInterfaceKomponente komponente)
			{
				komponente.VonKonsumentAingang(naacrict);
			});
		}

		private void AingangVonServer(SictZuNezSictDiferenzScritAbbild vonServerSctrukt)
		{
			long getTimeMilli = GetTimeMilli;
			if (vonServerSctrukt != null)
			{
				VonServerZuusctand = (VonServerSictSume.BerecneScritSumeListeWurzelRefClrUndErfolg(vonServerSctrukt)?.ListeWurzelRefClr?.FirstOrDefault() as VonServerNaacInterfaceZuusctand);
				ReaktioonAufVonServerZuusctand();
			}
			long num = getTimeMilli - NaacServerBerictLezteZait;
			if (vonServerSctrukt != null)
			{
				MengeKomponente?.ForEach(delegate(IInterfaceKomponente komponente)
				{
					komponente.VonServerZuusctandGeändert(VonServerZuusctand);
				});
			}
			if (vonServerSctrukt != null || num >= 1000)
			{
				AusgangNaacServerBerict();
			}
		}

		private void AusgangNaacServerBerict()
		{
			long zait = NaacServerBerictLezteZait = GetTimeMilli;
			NaacServerZuusctand.Zait = zait;
			NaacServerZuusctand.MengeFunkAusgefüürtAinmaal = ListeFunkAusgefüürtAinmaal?.Where((KeyValuePair<long, object> kandidaat) => VonServerZuusctand?.MengeFunkAuszufüüreAinmaal?.Any((KeyValuePair<long, object> auszufüüre) => auszufüüre.Key == kandidaat.Key) ?? false)?.ToArrayIfNotEmpty();
			((IEnumerable<byte[]>)AppDomainProxy.MengeAssemblyGelaadeIdentKopii()).PropagiireListeRepräsentatioon((IList)NaacServerZuusctand.MengeAssemblyGelaadeIdent, (Func<byte[], byte[]>)((byte[] t) => t), (Func<byte[], byte[], bool>)((byte[] t0, byte[] t1) => Glob.SequenceEqual(t0, t1)), (Action<byte[], byte[]>)null, repräsentatioonEntferneNict: false);
			object[] kweleListe = MengeKomponente?.Select((IInterfaceKomponente komponente) => komponente.NaacServerZuusctand())?.ToArray();
			kweleListe.PropagiireListeRepräsentatioonMitReprUndIdentPerClrReferenz(NaacServerZuusctand.MengeKomponente);
			Thread.MemoryBarrier();
			SictZuNezSictDiferenzScritAbbild obj2 = NaacServerSictDif.BerecneScritDif(NaacServerSictDif.ScritLezteIndex - 30, new object[1]
			{
				NaacServerZuusctand
			}, delegate(object obj)
			{
				Type type = obj.GetType();
				if (!string.Equals(type.Name, TempDebugBreakTypeName))
				{
				}
			});
			AusgangSclangeNaacServer.Enqueue(obj2.SerializeToUtf8());
		}

		private void ReaktioonAufVonServerZuusctand()
		{
			KeyValuePair<long, IFuncVoid>[] source = VonServerZuusctand?.MengeFunkAuszufüüreAinmaal?.Where((KeyValuePair<long, object> kandidaat) => !ListeFunkAusgefüürtAinmaal.Any((KeyValuePair<long, object> ausgefüürt) => ausgefüürt.Key == kandidaat.Key))?.Select((KeyValuePair<long, object> kandidaat) => new KeyValuePair<long, IFuncVoid>(kandidaat.Key, kandidaat.Value as IFuncVoid))?.Where((KeyValuePair<long, IFuncVoid> kandidaat) => kandidaat.Value != null)?.ToArrayIfNotEmpty();
			foreach (KeyValuePair<long, IFuncVoid> item in source.EmptyIfNull())
			{
				if (item.Value != null)
				{
					object value = item.Value.FüüreAus();
					ListeFunkAusgefüürtAinmaal.Add(new KeyValuePair<long, object>(item.Key, value));
				}
			}
		}

		private InterfaceProxyMessage NaacServerAusgangSctrukt()
		{
			return new InterfaceProxyMessage(AusgangSclangeNaacServer.DequeueEnum().Take(9)?.ToArray());
		}

		public byte[] AustauscKonsument(byte[] vonKonsument)
		{
			lock (Lock)
			{
				VonKonsumentAingang(vonKonsument);
				return NaacKonsumentAusgang();
			}
		}

		public void VonServerAingang(byte[] aingang)
		{
			Task task = new Task(delegate
			{
				AsyncInternVonServerAingang(aingang);
			});
			task.Start();
			task.Wait();
		}

		private void AsyncInternVonServerAingang(byte[] aingang)
		{
			lock (Lock)
			{
				try
				{
					VonServerAingangSctrukt(aingang.ProtobufDeserialize<InterfaceProxyMessage>());
				}
				catch (Exception exception)
				{
					NaacServerZuusctand.AingangExceptionLezte = new WertZuZaitpunktStruct<string>(exception.SictString(), GetTimeMilli);
				}
			}
		}

		public byte[] NaacServerAusgang()
		{
			return NaacServerAusgangSctrukt().ProtobufSerialize();
		}

		public object AppImplementationOfType(Type typeToAssignTo)
		{
			return MengeKomponente?.OfType<IInterfaceKomponenteImplementationOfType>()?.Select((IInterfaceKomponenteImplementationOfType komponente) => komponente?.Implementation(typeToAssignTo))?.WhereNotDefault()?.FirstOrDefault();
		}

		public string ClientRequest(string request)
		{
			return MengeKomponente?.Select((IInterfaceKomponente component) => component?.ClientRequest(request))?.WhereNotDefault()?.FirstOrDefault();
		}
	}
}
