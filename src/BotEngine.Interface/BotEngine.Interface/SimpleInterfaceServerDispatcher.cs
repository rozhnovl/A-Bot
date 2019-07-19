using Bib3;
using Bib3.RateLimit;
using Bib3.Synchronization;
using BotEngine.Client;
using BotEngine.Common;
using System;
using System.Timers;

namespace BotEngine.Interface
{
	public class SimpleInterfaceServerDispatcher
	{
		public class ExchangeReport
		{
			public PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> AuthExchange;

			public PropertyGenTimespanInt64<HttpExchangeReport<FromClientToServerMessage, FromServerToClientMessage>> PayloadExchange;
		}

		private readonly object @lock = new object();

		private readonly Timer exchangeTimer = new Timer();

		private PropertyGenTimespanInt64<InterfaceAppManager> InterfaceAppManagerAtTime;

		public LicenseClientConfig LicenseClientConfig;

		public LicenseClient LicenseClient;

		public Type InterfaceAppDomainSetupType;

		public bool InterfaceAppDomainSetupTypeLoadFromMainModule;

		public int ExchangeAuthTimeDistanceMinMilli = 4000;

		private readonly IRateLimitStateInt ExchangeAuthRateLimit = new RateLimitStateIntSingle();

		private readonly IRateLimitStateInt ExchangePayloadRateLimit = new RateLimitStateIntSingle();

		public InterfaceAppManager InterfaceAppManager
		{
			get
			{
				PropertyGenTimespanInt64<InterfaceAppManager> interfaceAppManagerAtTime = InterfaceAppManagerAtTime;
				return (interfaceAppManagerAtTime != null) ? interfaceAppManagerAtTime.Value : null;
			}
		}

		public virtual bool AppInterfaceAvailable => false;

		public virtual bool SessionContinue => InterfaceAppManager != null && (AppInterfaceAvailable || (LicenseClient?.AuthCompleted ?? false));

		private static long GetTimeMilli()
		{
			return TimesourceConfig.StaticConfig?.TimeContinuousMilli ?? 0;
		}

		public ExchangeReport Exchange(LicenseClientConfig licenseClientConfig = null)
		{
			return Exchange(licenseClientConfig ?? LicenseClientConfig, AppInterfaceAvailable ? new int?(1000) : null);
		}

		public static int? AuthTimeDistanceRecommended(AuthResponse authResponse)
		{
			return (authResponse == null) ? null : new int?((!authResponse.Success()) ? 60000 : 0);
		}

		public void Reset()
		{
			lock (@lock)
			{
				CyclicExchangeStop();
				LicenseClient = null;
				InterfaceAppManagerAtTime = null;
			}
		}

		public ExchangeReport Exchange(LicenseClientConfig licenseClientConfig, int? skipIfNotNeededForSessionKeepAliveSafetyMarginMilli)
		{
			lock (@lock)
			{
				PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> authExchange = null;
				PropertyGenTimespanInt64<HttpExchangeReport<FromClientToServerMessage, FromServerToClientMessage>> propertyGenTimespanInt = null;
				LicenseClientConfig obj = licenseClientConfig ?? LicenseClientConfig;
				licenseClientConfig = obj;
				LicenseClientConfig = obj;
				LicenseClient licenseClient = LicenseClient;
				if (licenseClient == null)
				{
					licenseClient = (LicenseClient = new LicenseClient());
				}
				if (licenseClientConfig != null)
				{
					licenseClient.ServerAddress = licenseClientConfig?.ApiVersionAddress;
					licenseClient.Request = licenseClientConfig?.Request;
				}
				if (SessionContinue)
				{
					IRateLimitStateInt exchangePayloadRateLimit = ExchangePayloadRateLimit;
					long timeMilli = GetTimeMilli();
					int? obj2;
					if (licenseClient == null)
					{
						obj2 = null;
					}
					else
					{
						PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> exchangeAuthLast = licenseClient.ExchangeAuthLast;
						obj2 = ((exchangeAuthLast == null) ? null : exchangeAuthLast.Value?.Response?.RequestTimeDistanceMaxMilli) - skipIfNotNeededForSessionKeepAliveSafetyMarginMilli;
					}
					if (!exchangePayloadRateLimit.AttemptPass(timeMilli, Math.Max(1000, obj2 ?? 0)))
					{
						return null;
					}
					object obj3;
					if (licenseClient == null)
					{
						obj3 = null;
					}
					else
					{
						PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> exchangeAuthLast2 = licenseClient.ExchangeAuthLast;
						obj3 = ((exchangeAuthLast2 == null) ? null : exchangeAuthLast2.Value?.Response?.SessionId);
					}
					string sessionId = (string)obj3;
					FromClientToServerMessage request = new FromClientToServerMessage
					{
						SessionId = sessionId,
						Interface = InterfaceAppManager?.ToServer(),
						Time = GetTimeMilli()
					};
					propertyGenTimespanInt = licenseClient?.ExchangePayload(request);
					FromServerToClientMessage fromServerToClientMessage = (propertyGenTimespanInt == null) ? null : propertyGenTimespanInt.Value?.Response;
					if (fromServerToClientMessage != null)
					{
						InterfaceAppManager?.FromServer(fromServerToClientMessage.Interface);
					}
				}
				else if (licenseClient?.AuthCompleted ?? false)
				{
					long? obj4;
					if (licenseClient == null)
					{
						obj4 = null;
					}
					else
					{
						PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> exchangeAuthLast3 = licenseClient.ExchangeAuthLast;
						obj4 = ((exchangeAuthLast3 != null) ? new long?(exchangeAuthLast3.End) : null);
					}
					long? num = obj4;
					PropertyGenTimespanInt64<InterfaceAppManager> interfaceAppManagerAtTime = InterfaceAppManagerAtTime;
					if (!(num < ((interfaceAppManagerAtTime != null) ? new long?(interfaceAppManagerAtTime.Begin) : null)))
					{
						InterfaceAppManagerAtTime = new PropertyGenTimespanInt64<InterfaceAppManager>(new InterfaceAppManager(InterfaceAppDomainSetupType, InterfaceAppDomainSetupTypeLoadFromMainModule), GetTimeMilli());
					}
				}
				else
				{
					int exchangeAuthTimeDistanceMinMilli = ExchangeAuthTimeDistanceMinMilli;
					object authResponse;
					if (licenseClient == null)
					{
						authResponse = null;
					}
					else
					{
						PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> exchangeAuthLast4 = licenseClient.ExchangeAuthLast;
						authResponse = ((exchangeAuthLast4 == null) ? null : exchangeAuthLast4.Value?.Response);
					}
					int num2 = Math.Max(exchangeAuthTimeDistanceMinMilli, AuthTimeDistanceRecommended((AuthResponse)authResponse) ?? 0);
					if (ExchangeAuthRateLimit.AttemptPass(GetTimeMilli(), num2))
					{
						authExchange = licenseClient?.ExchangeAuth();
					}
				}
				return new ExchangeReport
				{
					AuthExchange = authExchange,
					PayloadExchange = propertyGenTimespanInt
				};
			}
		}

		public SimpleInterfaceServerDispatcher()
		{
			exchangeTimer.Elapsed += ExchangeTimer_Elapsed;
			exchangeTimer.Interval = 400.0;
			exchangeTimer.AutoReset = true;
		}

		public void CyclicExchangeStart()
		{
			exchangeTimer.Start();
			//Exchange();
		}

		public void CyclicExchangeStop()
		{
			exchangeTimer.Stop();
		}

		private void ExchangeTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			@lock.IfLockIsAvailableEnter(delegate
			{
				//Exchange();
			});
		}
	}
}
