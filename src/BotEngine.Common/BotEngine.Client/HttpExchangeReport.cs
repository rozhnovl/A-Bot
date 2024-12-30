using System;
using System.Net;

namespace BotEngine.Client;

public class HttpExchangeReport<TRequest, TResponse> : IHttpExchangeReport
{
	public readonly TRequest Request;

	public readonly TResponse Response;

	public string Uri { get; private set; }

	public Exception Exception { get; private set; }

	public HttpStatusCode? HttpStatusCode { get; private set; }

	protected HttpExchangeReport()
	{
	}

	public HttpExchangeReport(TRequest request, TResponse response, string uri, Exception exception, HttpStatusCode? httpStatusCode)
	{
		Request = request;
		Response = response;
		Uri = uri;
		Exception = exception;
		HttpStatusCode = httpStatusCode;
	}
}
