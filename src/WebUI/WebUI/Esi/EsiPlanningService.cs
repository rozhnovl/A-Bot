using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebUI.Data;

namespace WebUI.Esi;

public sealed record EsiSellOrderPipeline(int OrderCount, decimal PotentialRevenue,
    DateTimeOffset? LastObservedAt);
public sealed record EsiObservedTradingFees(int EntryCount, decimal DebitedIsk,
    DateTimeOffset? LastObservedAt, bool HasJournalRecords);

public sealed class EsiPlanningService(IDbContextFactory<ProductionDbContext> factory)
{
    private static readonly HashSet<string> MarketFeeTypes =
    ["brokers_fee", "transaction_tax", "market_provider_tax", "market_security_tax"];

    public EsiSellOrderPipeline SellOrders()
    {
        using var db = factory.CreateDbContext();
        var rows = db.EsiRecords.AsNoTracking().Where(r => r.Kind == EsiRecordKind.Order &&
            r.Resource == EsiResource.ActiveOrders && r.IsCurrent).ToList();
        decimal potential = 0;
        var count = 0;
        foreach (var record in rows)
        {
            using var json = JsonDocument.Parse(record.RawJson);
            var order = json.RootElement;
            if (order.GetProperty("is_buy_order").GetBoolean()) continue;
            potential += order.GetProperty("price").GetDecimal() *
                order.GetProperty("volume_remain").GetInt32();
            count++;
        }
        return new EsiSellOrderPipeline(count, potential,
            rows.Count == 0 ? null : rows.Max(r => r.ObservedAt));
    }

    public EsiObservedTradingFees TradingFeesThisMonth()
    {
        using var db = factory.CreateDbContext();
        var rows = db.EsiRecords.AsNoTracking()
            .Where(r => r.Kind == EsiRecordKind.WalletJournalEntry).ToList();
        var month = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        var count = 0;
        decimal debits = 0;
        DateTimeOffset? latest = null;
        foreach (var record in rows)
        {
            using var json = JsonDocument.Parse(record.RawJson);
            var item = json.RootElement;
            if (!item.TryGetProperty("ref_type", out var kind) ||
                kind.ValueKind != JsonValueKind.String ||
                !MarketFeeTypes.Contains(kind.GetString() ?? "") ||
                !item.TryGetProperty("date", out var when) ||
                !when.TryGetDateTimeOffset(out var date) ||
                new DateOnly(date.ToLocalTime().Year, date.ToLocalTime().Month, 1) != month ||
                !item.TryGetProperty("amount", out var amount) ||
                amount.ValueKind != JsonValueKind.Number ||
                !amount.TryGetDecimal(out var signed) || signed >= 0) continue;
            debits -= signed;
            count++;
            if (latest is null || record.ObservedAt > latest) latest = record.ObservedAt;
        }
        return new EsiObservedTradingFees(count, debits, latest, rows.Count > 0);
    }
}
