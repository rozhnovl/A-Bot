using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebUI.Data;

namespace WebUI.Esi;

// Only wallet sell transactions are indisputable realized sales. Assets, orders and
// industry jobs stay as observations until a user reconciles them with local plans.
public sealed class EsiSalesProjection(
    IDbContextFactory<ProductionDbContext> factory,
    ProductionService production,
    ILogger<EsiSalesProjection> logger)
{
    public async Task ProjectAsync(long characterId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var clientId = $"esi-character-{characterId}";
        var known = (await db.ImportedClientEvents.AsNoTracking()
            .Where(e => e.ClientId == clientId).Select(e => e.EventId).ToListAsync(ct))
            .ToHashSet(StringComparer.Ordinal);
        var records = await db.EsiRecords.AsNoTracking().Where(r =>
            r.CharacterId == characterId && r.Kind == EsiRecordKind.WalletTransaction)
            .OrderBy(r => r.RecordId).ToListAsync(ct);
        var sales = new List<ProductionEventRequest>();
        foreach (var record in records)
        {
            ct.ThrowIfCancellationRequested();
            var eventId = $"transaction-{record.RecordId}-sale";
            if (known.Contains(eventId)) continue;
            using var json = JsonDocument.Parse(record.RawJson);
            var row = json.RootElement;
            if (row.GetProperty("is_buy").GetBoolean()) continue;
            // A corporation trade can also appear in a character wallet feed.
            // Keep the personal ledger single-owner until corporation divisions are modelled.
            if (row.TryGetProperty("is_personal", out var personal) && !personal.GetBoolean()) continue;
            var typeId = row.GetProperty("type_id").GetInt32();
            var quantity = row.GetProperty("quantity").GetInt32();
            var price = row.GetProperty("unit_price").GetDecimal();
            var date = row.GetProperty("date").GetDateTimeOffset();
            var request = new ProductionEventRequest(clientId, "sale", eventId,
                date, Product: $"type_id:{typeId}", Customer: "EVE ESI",
                Quantity: quantity, UnitPrice: price);
            sales.Add(request);
        }
        var imported = production.ImportEsiSales(clientId, sales);
        if (imported > 0)
            logger.LogInformation("Imported {Count} confirmed ESI sales for character {CharacterId}.",
                imported, characterId);
    }
}
