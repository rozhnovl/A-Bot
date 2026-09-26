using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using WebUI.Data;
using WebUI.Esi;

using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder<ProductionDbContext>().UseSqlite(connection).Options;
var factory = new TestDbFactory(options);
var production = new ProductionService(factory);
Check(production.Resources.Count == 0 && production.Sales.Count == 0,
    "A fresh database must not contain demo transactions.");
Check(production.MonthlyNetProfitGoal == 10_000_000_000m, "Default net-profit goal must be 10B ISK.");

production.AddResource("Tritanium", "Mineral", 10, 5);
production.ReceiveResource("Tritanium", 10, 15);
Check(production.Resources.Single().Quantity == 20 &&
      production.Resources.Single().AverageCost == 10,
    "Received resources must use weighted average acquisition cost.");

var manualSale = new ProductionEventRequest("bot-test", "sale", "tx-42",
    DateTimeOffset.UtcNow, Product: "type_id:34", Quantity: 2, UnitPrice: 100);
Check(production.ApplyClientEvent(manualSale, out _), "First sale event rejected.");
Check(production.ApplyClientEvent(manualSale, out _), "Repeated sale event rejected.");
Check(production.Sales.Count == 1 && production.RevenueThisMonth == 200,
    "Repeated event must not double-count revenue.");
Check(production.UnreconciledSalesThisMonth == 1 && production.ReconciledNetProfitThisMonth == 0,
    "Uncosted sale must not count as net profit.");
Check(production.Plan().RevenueThisMonth == 200 && production.Plan().ReconciledNetProfit == 0,
    "The plan must expose revenue separately from reconciled net profit.");
production.ReconcileSale(production.Sales.Single().Id, 30, 5, 2);
Check(production.ReconciledNetProfitThisMonth == 133 && production.UnreconciledSalesThisMonth == 0,
    "Net profit must deduct quantity cost, fees and other costs.");
production.AddWork("type_id:35", 1, 40, DateTime.Today, 100);
Check(production.Plan().OpenProductionNetProfitPotential == 0 && production.Plan().UnpricedOpenWorksCount == 1,
    "Revenue-only work must not become profit potential.");
production.SetExpectedNetProfit(production.Works.Single().Id, 45);
Check(production.Plan().ProjectedNetProfit == 178,
    "Only explicitly estimated net profit may enter the production forecast.");

using (var legacyConnection = new SqliteConnection("Data Source=:memory:"))
{
    legacyConnection.Open();
    var legacyOptions = new DbContextOptionsBuilder<ProductionDbContext>().UseSqlite(legacyConnection).Options;
    var legacyFactory = new TestDbFactory(legacyOptions);
    using (var legacyDb = legacyFactory.CreateDbContext())
    {
        legacyDb.Database.EnsureCreated();
        legacyDb.Settings.Add(new ProductionSettings { Id = 1, MonthlyNetProfitGoal = 7_000_000_000m });
        legacyDb.Sales.Add(new SaleRecord { Product = "old sale", Quantity = 1, UnitPrice = 100, UnitCost = 40, Date = DateTime.Today });
        legacyDb.Works.Add(new ProductionWork { Product = "old work", Quantity = 1, ExpectedRevenue = 200, DueDate = DateTime.Today });
        legacyDb.SaveChanges();
        legacyDb.Database.ExecuteSqlRaw("ALTER TABLE Settings RENAME COLUMN MonthlyNetProfitGoal TO MonthlyRevenueGoal");
        legacyDb.Database.ExecuteSqlRaw("ALTER TABLE Sales DROP COLUMN Fees");
        legacyDb.Database.ExecuteSqlRaw("ALTER TABLE Sales DROP COLUMN OtherCosts");
        legacyDb.Database.ExecuteSqlRaw("ALTER TABLE Works DROP COLUMN ExpectedNetProfit");
    }
    var upgraded = new ProductionService(legacyFactory);
    Check(upgraded.MonthlyNetProfitGoal == 7_000_000_000m && upgraded.Sales.Single().NetProfit is null &&
          upgraded.Works.Single().ExpectedNetProfit is null,
        "Legacy SQLite data and goal must survive the profit schema upgrade without invented costs.");
}

var store = new EsiSqliteStore(factory, Options.Create(new EsiOptions()),
    new EphemeralDataProtectionProvider());
var now = DateTimeOffset.UtcNow;
await store.SavePageAsync(OrderPage("1", "101", 2, now), CancellationToken.None);
using (var db = factory.CreateDbContext())
    Check(db.EsiRecords.Count() == 0, "An incomplete numbered snapshot must not publish records.");
try
{
    await store.CompleteResourceAsync(9001, EsiResource.ActiveOrders, 2, CancellationToken.None);
    throw new Exception("An incomplete snapshot was accepted.");
}
catch (InvalidDataException) { }
await store.SavePageAsync(OrderPage("2", "102", 2, now), CancellationToken.None);
await store.CompleteResourceAsync(9001, EsiResource.ActiveOrders, 2, CancellationToken.None);
using (var db = factory.CreateDbContext())
    Check(db.EsiRecords.Count(r => r.Kind == EsiRecordKind.Order && r.IsCurrent) == 2,
        "A complete order snapshot must publish both pages.");

await store.SavePageAsync(OrderPage("1", "101", 1, now.AddMinutes(1)), CancellationToken.None);
await store.CompleteResourceAsync(9001, EsiResource.ActiveOrders, 1, CancellationToken.None);
using (var db = factory.CreateDbContext())
    Check(db.EsiRecords.Single(r => r.RecordId == 102).IsCurrent == false,
        "A removed order must be marked stale only after full snapshot completion.");
var orderPlan = new EsiPlanningService(factory).SellOrders();
Check(orderPlan.OrderCount == 1 && orderPlan.PotentialRevenue == 500,
    "Only current sell orders may contribute to order potential.");

var journalJson = "[{\"id\":901,\"date\":\"" + DateTimeOffset.UtcNow.ToString("O") +
    "\",\"ref_type\":\"brokers_fee\",\"amount\":-12.5}]";
await store.SavePageAsync(new EsiSnapshotPage(9001, EsiResource.WalletJournal,
    EsiRecordKind.WalletJournalEntry, "1", journalJson, [901],
    new EsiCacheState(null, now.AddMinutes(5), 1), now, 1), CancellationToken.None);
using (var db = factory.CreateDbContext())
    Check(db.EsiRecords.Count(r => r.Kind == EsiRecordKind.WalletJournalEntry) == 0,
        "An incomplete journal page must not become an observed fee.");
await store.CompleteResourceAsync(9001, EsiResource.WalletJournal, 1, CancellationToken.None);
Check(new EsiPlanningService(factory).TradingFeesThisMonth().DebitedIsk == 12.5m,
    "Negative broker fee must be counted as an observed market debit.");

var walletJson = "[{\"transaction_id\":777,\"date\":\"" +
    DateTimeOffset.UtcNow.ToString("O") +
    "\",\"is_buy\":false,\"is_personal\":true,\"type_id\":35,\"quantity\":3,\"unit_price\":40}," +
    "{\"transaction_id\":778,\"date\":\"" + DateTimeOffset.UtcNow.ToString("O") +
    "\",\"is_buy\":false,\"is_personal\":false,\"type_id\":35,\"quantity\":5,\"unit_price\":40}]";
await store.SavePageAsync(new EsiSnapshotPage(9001, EsiResource.WalletTransactions,
    EsiRecordKind.WalletTransaction, "first", walletJson, [777, 778],
    new EsiCacheState(null, now.AddMinutes(5)), now, null), CancellationToken.None);
var projection = new EsiSalesProjection(factory, production,
    NullLogger<EsiSalesProjection>.Instance);
await projection.ProjectAsync(9001, CancellationToken.None);
await projection.ProjectAsync(9001, CancellationToken.None);
Check(production.Sales.Count == 2 && production.RevenueThisMonth == 320,
    "Repeated ESI projection must import one wallet sale exactly once.");
Check(production.ReconciledNetProfitThisMonth == 133 && production.UnreconciledSalesThisMonth == 1,
    "ESI sale without costs must remain outside net-profit fact.");

var requestedUrls = new List<string>();
var handler = new RecordingHandler(request =>
{
    requestedUrls.Add(request.RequestUri!.ToString());
    Check(request.Headers.Contains("X-Compatibility-Date"), "Compatibility date was not sent.");
    Check(request.Headers.Authorization?.Scheme == "Bearer", "Bearer token was not sent.");
    var response = new HttpResponseMessage(request.Headers.IfNoneMatch.Count == 0
        ? HttpStatusCode.OK : HttpStatusCode.NotModified);
    response.Headers.ETag = new EntityTagHeaderValue("\"test-etag\"");
    response.Headers.TryAddWithoutValidation("Expires", DateTimeOffset.UtcNow.AddMinutes(5).ToString("R"));
    response.Headers.TryAddWithoutValidation("X-Pages", "1");
    response.Content = new StringContent("[]");
    return response;
});
var esiHttp = new EsiHttpClient(new HttpClient(handler), Options.Create(new EsiOptions()));
var first = await esiHttp.GetPageAsync(9001, EsiResource.ActiveOrders, "2",
    "test-token", null, CancellationToken.None);
var unchanged = await esiHttp.GetPageAsync(9001, EsiResource.ActiveOrders, "2",
    "test-token", first.Cache.ETag, CancellationToken.None);
Check(requestedUrls.All(url => !url.Contains("page=2", StringComparison.OrdinalIgnoreCase)),
    "Character active orders must not be requested with an unsupported page parameter.");
Check(!first.NotModified && unchanged.NotModified && first.Cache.ETag == "\"test-etag\"",
    "ESI ETag and 304 handling failed.");
var throttledClient = new EsiHttpClient(new HttpClient(new RecordingHandler(_ =>
{
    var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
    response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(7));
    return response;
})), Options.Create(new EsiOptions()));
try
{
    await throttledClient.GetPageAsync(9001, EsiResource.Assets, "1", "test-token",
        null, CancellationToken.None);
    throw new Exception("HTTP 429 was not treated as backoff.");
}
catch (EsiBackoffException ex)
{
    Check(ex.RetryAfter == TimeSpan.FromSeconds(7), "Retry-After was ignored.");
}
var health = new EsiSyncHealth();
health.Attempt(9001);
health.Success(9001);
var lastSuccess = health.Snapshot().LastSuccessAt;
health.Failure(9001, "ESI unavailable");
Check(health.Snapshot().State == "error" && health.Snapshot().LastSuccessAt == lastSuccess,
    "A failed sync must preserve the last successful timestamp.");
var oauthState = new EsiOAuthState(new EphemeralDataProtectionProvider());
var ticket = oauthState.Issue("operator-1");
Check(!oauthState.TryConsume(ticket, "operator-2"), "OAuth state must reject a different operator.");
var ownTicket = oauthState.Issue("operator-1");
Check(oauthState.TryConsume(ownTicket, "operator-1") &&
      !oauthState.TryConsume(ownTicket, "operator-1"),
    "OAuth state must be one-time for the initiating operator.");
Console.WriteLine("Production integration checks passed: inventory, idempotency, atomic snapshots, wallet projection, journal fees, ESI cache/backoff and sync health.");

static EsiSnapshotPage OrderPage(string cursor, string id, int total, DateTimeOffset fetchedAt)
{
    var raw = $"[{{\"order_id\":{id},\"is_buy_order\":false,\"price\":100,\"volume_remain\":5}}]";
    return new EsiSnapshotPage(9001, EsiResource.ActiveOrders, EsiRecordKind.Order,
        cursor, raw, [long.Parse(id)], new EsiCacheState(null, fetchedAt.AddMinutes(5), total),
        fetchedAt, total);
}

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

sealed class TestDbFactory(DbContextOptions<ProductionDbContext> options)
    : IDbContextFactory<ProductionDbContext>
{
    public ProductionDbContext CreateDbContext() => new(options);
    public Task<ProductionDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());
}

sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken) => Task.FromResult(respond(request));
}
