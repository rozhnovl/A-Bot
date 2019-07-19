using Bib3;
using BotEngine.Client;
using BotEngine.Common;
using System;
using System.Net;
using System.Text;

namespace BotEngine.Interface
{
	public class LicenseClient
	{
		public const string ServerAddressComponentAuth = "auth";

		public const string ServerAddressComponentPayload = "payload";

		private readonly object Lock = new object();

		private readonly CookieContainer CookieContainer = new CookieContainer();

		private readonly WebClient WebClient;

		public AuthRequest Request;

		public string ServerAddress;

		public int Timeout = 4000;

		public PropertyGenTimespanInt64<IHttpExchangeReport> HttpExchangeLast
		{
			get;
			private set;
		}

		public PropertyGenTimespanInt64<IHttpExchangeReport> HttpExchangeSuccessfulLast
		{
			get;
			private set;
		}

		public PropertyGenTimespanInt64<IHttpExchangeReport> HttpExchangeFailedLast
		{
			get;
			private set;
		}

		public PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> ExchangeAuthLast
		{
			get;
			private set;
		}

		public PropertyGenTimespanInt64<HttpExchangeReport<FromClientToServerMessage, FromServerToClientMessage>> ExchangePayloadLast
		{
			get;
			private set;
		}

		public int ListeRequestCount => WebClient.ListeRequestCount;

		public int ListeResponseCount => WebClient.ListeResponseCount;

		public long ListeRequestAggregatedContentLength => WebClient.ListeRequestAggregatedContentLength;

		public long ListeResponseAggregatedContentLength => WebClient.ListeResponseAggregatedContentLength;

		public bool AuthCompleted
		{
			get
			{
				PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> exchangeAuthLast = ExchangeAuthLast;
				int result;
				if (((exchangeAuthLast == null) ? null : exchangeAuthLast.Value?.AuthSuccess()) ?? false)
				{
					PropertyGenTimespanInt64<HttpExchangeReport<FromClientToServerMessage, FromServerToClientMessage>> exchangePayloadLast = ExchangePayloadLast;
					if (((exchangePayloadLast == null) ? null : exchangePayloadLast.Value?.ErrorUnauthorized()) ?? false)
					{
						PropertyGenTimespanInt64<HttpExchangeReport<FromClientToServerMessage, FromServerToClientMessage>> exchangePayloadLast2 = ExchangePayloadLast;
						long? num = (exchangePayloadLast2 != null) ? new long?(exchangePayloadLast2.End) : null;
						PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> exchangeAuthLast2 = ExchangeAuthLast;
						result = ((num < ((exchangeAuthLast2 != null) ? new long?(exchangeAuthLast2.Begin) : null)) ? 1 : 0);
					}
					else
					{
						result = 1;
					}
				}
				else
				{
					result = 0;
				}
				return (byte)result != 0;
			}
		}

		public string UriPlusComponent(string component)
		{
			return ServerAddress.EnsureEndsWith("/") + component;
		}

		public LicenseClient()
		{
			WebClient = new WebClient(CookieContainer);
		}

		private byte[] UploadData(string uri, byte[] uploadData)
		{
			WebClient.Headers[HttpRequestHeader.ContentType] = "application/octet-stream";
			return WebClient.UploadData(uri, "POST", uploadData);
		}

		private byte[] UploadDataDeflate(string uri, byte[] uploadData)
		{
			return UploadData(uri, uploadData).DeflateDeKompres(10000000L);
		}

		public PropertyGenTimespanInt64<HttpExchangeReport<TRequest, TResponse>> HttpExchangeLocked<TRequest, TResponse>(string uriComponent, TRequest request, bool deflate = false) where TResponse : class
		{
			lock (Lock)
			{
				long timeContinuousMilli = TimesourceConfig.StaticConfig.TimeContinuousMilli;
				PropertyGenTimespanInt64<HttpExchangeReport<TRequest, TResponse>> propertyGenTimespanInt = null;
				string text = UriPlusComponent(uriComponent);
				TResponse response = null;
				Exception exception = null;
				HttpStatusCode? httpStatusCode = null;
				try
				{
					string sessionId = null;
					IRequestInSession requestInSession = request as IRequestInSession;
					if (requestInSession != null)
					{
						sessionId = requestInSession.SessionIdAsString();
					}
					try
					{
						WebClient.Timeout = Timeout;
						WebClient.SessionId = sessionId;
						byte[] bytes = Encoding.UTF8.GetBytes(request.SerializeToString());
						Func<string, byte[], byte[]> func = deflate ? new Func<string, byte[], byte[]>(UploadDataDeflate) : new Func<string, byte[], byte[]>(UploadData);
						byte[] array = func(text, bytes);
						if (array.IsNullOrEmpty())
						{
							throw new ArgumentException("Response empty");
						}
						response = ((array != null) ? array.DeserializeFromUtf8<TResponse>() : null);
						httpStatusCode = HttpStatusCode.OK;
					}
					catch (WebException ex)
					{
						httpStatusCode = (ex?.Response as HttpWebResponse)?.StatusCode;
						exception = ex;
					}
					catch (Exception ex2)
					{
						exception = ex2;
					}
				}
				finally
				{
					long timeContinuousMilli2 = TimesourceConfig.StaticConfig.TimeContinuousMilli;
					propertyGenTimespanInt = new PropertyGenTimespanInt64<HttpExchangeReport<TRequest, TResponse>>(new HttpExchangeReport<TRequest, TResponse>(request, response, text, exception, httpStatusCode), timeContinuousMilli, timeContinuousMilli2);
					HttpExchangeLast = propertyGenTimespanInt.CastToHttpExchange();
					PropertyGenTimespanInt64<IHttpExchangeReport> propertyGenTimespanInt2 = propertyGenTimespanInt.CastToHttpExchange();
					if (propertyGenTimespanInt.Value.HttpExchangeSuccess() ?? false)
					{
						HttpExchangeSuccessfulLast = propertyGenTimespanInt2;
					}
					else
					{
						HttpExchangeFailedLast = propertyGenTimespanInt2;
					}
				}
				return propertyGenTimespanInt;
			}
		}

		public PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> ExchangeAuth()
		{
			return ExchangeAuth(new AuthRequest
			{
				ProofOfWork = AuthRequest.ProofOfWorkConstruct(),
				ServiceId = Request?.ServiceId,
				ServiceInterfaceId = Request?.ServiceInterfaceId,
				LicenseKey = Request?.LicenseKey,
				ReffererId = Request?.ReffererId,
				Consume = (Request?.Consume ?? false)
			});
		}

		public PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> ExchangeAuth(AuthRequest authRequest)
		{
			return ExchangeAuthLast = HttpExchangeLocked<AuthRequest, AuthResponse>("auth", authRequest);
		}

		public PropertyGenTimespanInt64<HttpExchangeReport<FromClientToServerMessage, FromServerToClientMessage>> ExchangePayload(FromClientToServerMessage request)
		{
			request.ProofOfWork = AuthRequest.ProofOfWorkConstruct(4000);
			PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> exchangeAuthLast = ExchangeAuthLast;
			request.SessionId = ((exchangeAuthLast == null) ? null : exchangeAuthLast.Value?.Response?.SessionId);
			return ExchangePayloadLast = HttpExchangeLocked<FromClientToServerMessage, FromServerToClientMessage>("payload", request, deflate: true);
		}
	}
}
