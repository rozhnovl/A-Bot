namespace WebUI.Data;

public sealed record ProductionEventRequest(
    string ClientId,
    string EventType,
    string? EventId = null,
    DateTimeOffset? CapturedAt = null,
    string? Resource = null,
    string? Product = null,
    string? Customer = null,
    decimal? Quantity = null,
    decimal? UnitPrice = null,
    decimal? UnitCost = null,
    decimal? Fees = null,
    decimal? OtherCosts = null,
    decimal? Progress = null,
    string? Supplier = null,
    int? WorkId = null,
    string? Metadata = null);

public sealed record ProductionSnapshot(
    IReadOnlyList<ResourceItem> Resources,
    IReadOnlyList<ProductionWork> Works,
    IReadOnlyList<PurchasePlan> Purchases,
    IReadOnlyList<SaleRecord> Sales,
    DateTimeOffset GeneratedAt);

public sealed record ProductSalesSummary(string Product, int UnitsSoldThisMonth, decimal AverageSalePrice, decimal RevenueThisMonth);

public sealed record ProductionPlan(
    DateOnly Month,
    decimal NetProfitGoal,
    decimal ReconciledNetProfit,
    int UnreconciledSalesCount,
    decimal OpenProductionNetProfitPotential,
    int UnpricedOpenWorksCount,
    decimal ProjectedNetProfit,
    decimal ProfitGapAfterPipeline,
    decimal RequiredNetProfitPerRemainingDay,
    decimal RevenueThisMonth,
    IReadOnlyList<ProductSalesSummary> Products,
    IReadOnlyList<ProductionWork> OpenWorks,
    IReadOnlyList<PurchasePlan> OpenPurchases,
    DateTimeOffset GeneratedAt,
    WebUI.Esi.EsiSellOrderPipeline? SellOrders = null,
    WebUI.Esi.EsiObservedTradingFees? ObservedTradingFees = null);
