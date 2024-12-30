using System;
using System.Net;
using Bib3;
using Bib3.AppDomain;

namespace BotEngine.Client;

public static class Extension
{
	public static StructPropertyGenIntervalInt64<IHttpExchangeReport> CastToHttpExchange<T>(this StructPropertyGenIntervalInt64<T> @in) where T : IHttpExchangeReport
	{
		return @in.CastGen<T, IHttpExchangeReport>();
	}

	public static StructPropertyGenIntervalInt64<IHttpExchangeReport>? CastToHttpExchange<T>(this StructPropertyGenIntervalInt64<T>? @in) where T : IHttpExchangeReport
	{
		return @in.CastGen<T, IHttpExchangeReport>();
	}

	public static PropertyGenTimespanInt64<IHttpExchangeReport> CastToHttpExchange<T>(this PropertyGenTimespanInt64<T> @in) where T : IHttpExchangeReport
	{
		return @in?.CastScpez<T, IHttpExchangeReport>();
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

	public static HttpExchangeReport<OutRequestT, OutResponseT> Map<InRequestT, InResponseT, OutRequestT, OutResponseT>(this HttpExchangeReport<InRequestT, InResponseT> orig, Func<InRequestT, OutRequestT> transformRequest, Func<InResponseT, OutResponseT> transformResponse)
	{
		if (orig == null)
		{
			return null;
		}
		return new HttpExchangeReport<OutRequestT, OutResponseT>(transformRequest(orig.Request), transformResponse(orig.Response), orig.Uri, orig.Exception, orig.HttpStatusCode);
	}

	public static HttpExchangeReport<RequestT, ResponseT> Map<RequestT, ResponseT>(this HttpExchangeReport<RequestT, ResponseT> orig, Func<RequestT, RequestT> transformRequest)
	{
		return orig.Map(transformRequest, (ResponseT t) => t);
	}

	public static string InterfaceIdFromAssembly(this byte[] assembly)
	{
		return (assembly == null) ? null : Convert.ToBase64String(AppDomainProxy.IdentBerecne(assembly));
	}
}
