using System;
using System.Net;
using Bib3;
using Bib3.AppDomain;

namespace BotEngine.Client;

public static class Extension
{
	public static PropertyGenTimespanInt64<IHttpExchangeReport> CastToHttpExchange<T>(this PropertyGenTimespanInt64<T> @in) where T : IHttpExchangeReport
	{
		return @in == null ? null : IntervalExtension.MapValue<T, IHttpExchangeReport>(@in, ainWert => (object)ainWert as IHttpExchangeReport);
	}

	public static bool? ErrorUnauthorized(this IHttpExchangeReport httpExchange)
	{
		HttpStatusCode? httpStatusCode = httpExchange?.HttpStatusCode;
		if (!httpStatusCode.HasValue)
		{
			return null;
		}
		return HttpStatusCode.Unauthorized == httpStatusCode;
	}

	public static bool? HttpExchangeSuccess(this IHttpExchangeReport httpExchange)
	{
		if (httpExchange == null)
		{
			return null;
		}
		return HttpStatusCode.OK == httpExchange?.HttpStatusCode;
	}

	public static bool Success(this AuthResponse authResponse)
	{
		return 0 < authResponse?.SessionId?.Length;
	}

	public static bool? AuthSuccess(this HttpExchangeReport<AuthRequest, AuthResponse> httpExchangeReport)
	{
		return httpExchangeReport?.Response?.Success();
	}

}
