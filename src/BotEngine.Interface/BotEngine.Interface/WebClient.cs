using System;
using System.Net;

namespace BotEngine.Interface
{
	public class WebClient : System.Net.WebClient
	{
		private readonly object Lock = new object();

		public const string HeaderSessionId = "SessionId";

		private readonly CookieContainer CookieContainer = null;

		public int ListeRequestCount
		{
			get;
			private set;
		}

		public int ListeResponseCount
		{
			get;
			private set;
		}

		public long ListeRequestAggregatedContentLength
		{
			get;
			private set;
		}

		public long ListeResponseAggregatedContentLength
		{
			get;
			private set;
		}

		public int Timeout
		{
			get;
			set;
		} = 10000;


		public string SessionId
		{
			get;
			set;
		}

		public WebClient(CookieContainer cookieContainer)
		{
			CookieContainer = cookieContainer;
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			lock (Lock)
			{
				WebRequest webRequest = base.GetWebRequest(address);
				webRequest.Timeout = Timeout;
				HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
				if (httpWebRequest != null)
				{
					httpWebRequest.CookieContainer = CookieContainer;
					httpWebRequest.AutomaticDecompression = DecompressionMethods.Deflate;
					httpWebRequest.Headers.Add("SessionId", SessionId ?? "");
				}
				int num = ++ListeRequestCount;
				ListeRequestAggregatedContentLength += webRequest.ContentLength;
				return webRequest;
			}
		}

		private void ReadCookies(WebResponse r)
		{
			HttpWebResponse httpWebResponse = r as HttpWebResponse;
			if (httpWebResponse != null)
			{
				CookieCollection cookies = httpWebResponse.Cookies;
				CookieContainer.Add(cookies);
			}
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			lock (Lock)
			{
				WebResponse webResponse = base.GetWebResponse(request);
				ReadCookies(webResponse);
				int num = ++ListeResponseCount;
				ListeResponseAggregatedContentLength += webResponse.ContentLength;
				return webResponse;
			}
		}
	}
}
