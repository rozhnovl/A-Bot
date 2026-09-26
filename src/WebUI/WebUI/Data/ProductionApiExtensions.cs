using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using WebUI.Esi;

namespace WebUI.Data;

public static class ProductionApiExtensions
{
    public static IEndpointRouteBuilder MapProductionApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/production");

        group.MapGet("/snapshot", (HttpRequest http, ProductionService service, IConfiguration configuration) =>
            IsAuthorized(http, configuration) ? Results.Ok(service.Snapshot()) : Results.Unauthorized());

        group.MapGet("/plan", (HttpRequest http, ProductionService service, EsiPlanningService esi, IConfiguration configuration) =>
            IsAuthorized(http, configuration) ? Results.Ok(service.Plan() with
            {
                SellOrders = esi.SellOrders(),
                ObservedTradingFees = esi.TradingFeesThisMonth()
            }) : Results.Unauthorized());

        group.MapPost("/events", Results<Accepted<ProductionSnapshot>, BadRequest<object>, UnauthorizedHttpResult> (
            HttpRequest http,
            ProductionEventRequest request,
            ProductionService service,
            IConfiguration configuration
            ) =>
        {
            if (!IsAuthorized(http, configuration)) return TypedResults.Unauthorized();
            if (string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(request.EventId) || string.IsNullOrWhiteSpace(request.EventType))
                return TypedResults.BadRequest<object>(new { error = "clientId, eventId and eventType are required" });
            if (!service.ApplyClientEvent(request, out var error))
                return TypedResults.BadRequest<object>(new { error });

            return TypedResults.Accepted("/api/production/snapshot", service.Snapshot());
        });

        return endpoints;
    }

    internal static bool IsAuthorized(HttpRequest request, IConfiguration configuration)
    {
        var configuredKey = configuration["Production:ApiKey"];
        if (!request.Headers.TryGetValue("X-Production-Api-Key", out var supplied) || string.IsNullOrWhiteSpace(configuredKey)) return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(supplied.ToString()), Encoding.UTF8.GetBytes(configuredKey));
    }
}
