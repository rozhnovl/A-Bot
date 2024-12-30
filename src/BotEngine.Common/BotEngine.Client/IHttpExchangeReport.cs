using System;
using System.Net;

namespace BotEngine.Client;

public interface IHttpExchangeReport
{
	string Uri { get; }

	Exception Exception { get; }

	HttpStatusCode? HttpStatusCode { get; }
}
