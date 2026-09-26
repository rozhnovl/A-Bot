using Microsoft.EntityFrameworkCore;
using System.Data;

namespace WebUI.Data;

// A small single-process store. Every command commits one transaction, then refreshes
// the read model used by the Blazor pages. The database remains the source of truth.
public sealed class ProductionService
{
    private readonly IDbContextFactory<ProductionDbContext> dbFactory;
    private readonly object gate = new();
    public const decimal DefaultMonthlyNetProfitGoal = 10_000_000_000m;

    public List<ResourceItem> Resources { get; } = [];
    public List<ProductionWork> Works { get; } = [];
    public List<PurchasePlan> Purchases { get; } = [];
    public List<SaleRecord> Sales { get; } = [];
    public decimal MonthlyNetProfitGoal { get; private set; } = DefaultMonthlyNetProfitGoal;
    public event Action? Changed;

    public ProductionService(IDbContextFactory<ProductionDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
        using var db = dbFactory.CreateDbContext();
        db.Database.EnsureCreated();
        EnsureProfitSchema(db);
        lock (gate) Reload(db);
    }

    public decimal InventoryValue { get { lock (gate) return Resources.Sum(r => r.Quantity * r.AverageCost); } }
    public decimal PlannedSpend { get { lock (gate) return Works.Where(w => w.Status != WorkStatus.Done).Sum(w => w.EstimatedCost) + Purchases.Where(p => p.Status != PurchaseStatus.Ordered).Sum(p => p.Total); } }
    public decimal RevenueThisMonth { get { lock (gate) return Sales.Where(IsThisMonth).Sum(s => s.Total); } }
    public decimal CostOfSalesThisMonth { get { lock (gate) return Sales.Where(IsThisMonth).Sum(s => s.Quantity * (s.UnitCost ?? 0)); } }
    public decimal ReconciledNetProfitThisMonth { get { lock (gate) return Sales.Where(IsThisMonth).Sum(s => s.NetProfit ?? 0); } }
    public int UnreconciledSalesThisMonth { get { lock (gate) return Sales.Count(s => IsThisMonth(s) && s.NetProfit is null); } }
    public int ActiveWorks { get { lock (gate) return Works.Count(w => w.Status is WorkStatus.InProgress or WorkStatus.Planned); } }
    public int LowStockCount { get { lock (gate) return Resources.Count(r => r.Status is "Низкий остаток" or "Под заказ"); } }

    public ProductionSnapshot Snapshot()
    {
        lock (gate) return new(Resources.Select(Clone).ToList(), Works.Select(Clone).ToList(), Purchases.Select(Clone).ToList(), Sales.Select(Clone).ToList(), DateTimeOffset.UtcNow);
    }

    public ProductionPlan Plan()
    {
        lock (gate)
        {
            var today = DateTime.Today;
            var monthSales = Sales.Where(IsThisMonth).ToList();
            var realized = monthSales.Sum(s => s.NetProfit ?? 0);
            var unreconciled = monthSales.Count(s => s.NetProfit is null);
            var openWorks = Works.Where(w => w.Status != WorkStatus.Done).Select(Clone).ToList();
            var dueWorks = openWorks.Where(w => w.DueDate.Year == today.Year && w.DueDate.Month == today.Month).ToList();
            var potential = dueWorks.Sum(w => w.ExpectedNetProfit ?? 0);
            var projected = realized + potential;
            var products = Sales.Where(IsThisMonth).GroupBy(s => s.Product, StringComparer.OrdinalIgnoreCase)
                .Select(g => new ProductSalesSummary(g.Key, g.Sum(s => s.Quantity), g.Sum(s => s.Total) / g.Sum(s => s.Quantity), g.Sum(s => s.Total)))
                .OrderByDescending(x => x.RevenueThisMonth).ToList();
            var remainingDays = Math.Max(1, DateTime.DaysInMonth(today.Year, today.Month) - today.Day + 1);
            return new ProductionPlan(new DateOnly(today.Year, today.Month, 1), MonthlyNetProfitGoal, realized, unreconciled,
                potential, dueWorks.Count(w => w.ExpectedNetProfit is null), projected,
                Math.Max(0, MonthlyNetProfitGoal - projected), Math.Max(0, MonthlyNetProfitGoal - projected) / remainingDays,
                monthSales.Sum(s => s.Total),
                products, openWorks, Purchases.Where(p => p.Status != PurchaseStatus.Ordered).Select(Clone).ToList(), DateTimeOffset.UtcNow);
        }
    }

    public void SetMonthlyNetProfitGoal(decimal goal)
    {
        if (goal <= 0) return;
        Commit(db =>
        {
            var settings = db.Settings.Find(1);
            if (settings is null) db.Settings.Add(new ProductionSettings { Id = 1, MonthlyNetProfitGoal = goal });
            else settings.MonthlyNetProfitGoal = goal;
        });
    }

    public void AddResource(string name, string category, decimal quantity, decimal cost)
    {
        if (string.IsNullOrWhiteSpace(name) || quantity <= 0 || cost < 0) return;
        Commit(db => ReceiveResource(db, name, category, quantity, cost));
    }

    public void ReceiveResource(string name, decimal quantity, decimal cost)
    {
        if (string.IsNullOrWhiteSpace(name) || quantity <= 0 || cost < 0) return;
        Commit(db => ReceiveResource(db, name, "Другое", quantity, cost));
    }

    private static void ReceiveResource(ProductionDbContext db, string name, string category, decimal quantity, decimal cost)
    {
        var trimmed = name.Trim();
        var resource = db.Resources.ToList().FirstOrDefault(r => r.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        if (resource is null)
        {
            db.Resources.Add(new ResourceItem { Name = trimmed, Category = string.IsNullOrWhiteSpace(category) ? "Другое" : category.Trim(), Quantity = quantity, AverageCost = cost, Unit = "ед.", Status = "В наличии" });
        }
        else
        {
            resource.AverageCost = (resource.Quantity * resource.AverageCost + quantity * cost) / (resource.Quantity + quantity);
            resource.Quantity += quantity;
            resource.Status = "В наличии";
        }
    }

    public void AddWork(string product, int quantity, decimal cost, DateTime dueDate, decimal? expectedRevenue = null, decimal? expectedNetProfit = null)
    {
        if (string.IsNullOrWhiteSpace(product) || quantity <= 0 || cost < 0) return;
        Commit(db => db.Works.Add(new ProductionWork { Product = product.Trim(), Type = "Сборка", Quantity = quantity, EstimatedCost = cost, ExpectedRevenue = expectedRevenue is > 0 ? expectedRevenue.Value : 0, ExpectedNetProfit = expectedNetProfit, DueDate = dueDate, Status = WorkStatus.Planned, Progress = 0 }));
    }

    public void SetExpectedNetProfit(int workId, decimal? expectedNetProfit)
    {
        Commit(db => { var work = db.Works.Find(workId); if (work is not null) work.ExpectedNetProfit = expectedNetProfit; });
    }

    public bool UpdateProgress(string product, int progress)
    {
        if (string.IsNullOrWhiteSpace(product) || progress is < 0 or > 100) return false;
        var found = false;
        Commit(db =>
        {
            var work = db.Works.Where(w => w.Status != WorkStatus.Done).OrderByDescending(w => w.Id).ToList().FirstOrDefault(w => w.Product.Equals(product.Trim(), StringComparison.OrdinalIgnoreCase));
            if (work is null) return;
            found = true;
            work.Progress = progress;
            work.Status = progress == 100 ? WorkStatus.Done : WorkStatus.InProgress;
        });
        return found;
    }

    public void AddPurchase(string resource, decimal quantity, decimal price, string supplier, DateTime dueDate, int? workId = null)
    {
        if (string.IsNullOrWhiteSpace(resource) || quantity <= 0 || price < 0) return;
        Commit(db => db.Purchases.Add(new PurchasePlan { Resource = resource.Trim(), Quantity = quantity, UnitPrice = price, Supplier = string.IsNullOrWhiteSpace(supplier) ? "Не выбран" : supplier.Trim(), DueDate = dueDate, WorkId = workId is > 0 && db.Works.Any(w => w.Id == workId) ? workId : null, Status = PurchaseStatus.ToOrder }));
    }

    public void AddSale(string product, int quantity, decimal price, string customer, decimal? unitCost = null, decimal? fees = null, decimal? otherCosts = null)
    {
        if (string.IsNullOrWhiteSpace(product) || quantity <= 0 || price < 0 || unitCost is < 0 || fees is < 0 || otherCosts is < 0) return;
        Commit(db => db.Sales.Add(new SaleRecord { Product = product.Trim(), Quantity = quantity, UnitPrice = price, UnitCost = unitCost, Fees = fees, OtherCosts = otherCosts, Date = DateTime.Today, Customer = string.IsNullOrWhiteSpace(customer) ? "Без клиента" : customer.Trim() }));
    }

    public void ReconcileSale(int saleId, decimal unitCost, decimal fees, decimal otherCosts)
    {
        if (unitCost < 0 || fees < 0 || otherCosts < 0) return;
        Commit(db => { var sale = db.Sales.Find(saleId); if (sale is not null) { sale.UnitCost = unitCost; sale.Fees = fees; sale.OtherCosts = otherCosts; } });
    }

    public bool ApplyClientEvent(ProductionEventRequest request, out string error)
    {
        error = "";
        if (string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(request.EventId) || string.IsNullOrWhiteSpace(request.EventType))
        { error = "clientId, eventId and eventType are required"; return false; }
        if (request.ClientId.Length > 100 || request.EventId.Length > 200)
        { error = "clientId or eventId is too long"; return false; }
        var type = request.EventType.Trim().ToLowerInvariant();
        if (type is not ("resource-received" or "sale" or "production-started" or "production-progress" or "purchase-ordered"))
        { error = $"unsupported eventType '{request.EventType}'"; return false; }
        if (type is "resource-received" or "purchase-ordered" && (string.IsNullOrWhiteSpace(request.Resource) || request.Quantity is not > 0 || request.UnitPrice is not >= 0))
        { error = "resource, positive quantity and non-negative unitPrice are required"; return false; }
        if (type is "sale" && (string.IsNullOrWhiteSpace(request.Product) || request.Quantity is not >= 1 || request.Quantity > int.MaxValue || request.Quantity != decimal.Truncate(request.Quantity.Value) || request.UnitPrice is not >= 0))
        { error = "product, whole positive quantity and non-negative unitPrice are required"; return false; }
        if (type is "production-started" && (string.IsNullOrWhiteSpace(request.Product) || request.Quantity is not >= 1 || request.Quantity > int.MaxValue || request.Quantity != decimal.Truncate(request.Quantity.Value)))
        { error = "product and whole positive quantity are required"; return false; }
        if (type is "production-progress" && (string.IsNullOrWhiteSpace(request.Product) || request.Progress is < 0 or > 100 or null || request.Progress != decimal.Truncate(request.Progress.Value)))
        { error = "product and progress from 0 to 100 are required"; return false; }
        if (request.UnitCost is < 0 || request.Fees is < 0 || request.OtherCosts is < 0) { error = "costs cannot be negative"; return false; }

        var applied = true;
        Commit(db =>
        {
            if (db.ImportedClientEvents.Any(e => e.ClientId == request.ClientId && e.EventId == request.EventId)) return;
            switch (type)
            {
                case "resource-received": ReceiveResource(db, request.Resource!, "Другое", request.Quantity!.Value, request.UnitPrice!.Value); break;
                case "sale": db.Sales.Add(new SaleRecord { Product = request.Product!.Trim(), Quantity = (int)request.Quantity!.Value, UnitPrice = request.UnitPrice!.Value, UnitCost = request.UnitCost, Fees = request.Fees, OtherCosts = request.OtherCosts, Date = request.CapturedAt?.UtcDateTime ?? DateTime.UtcNow, Customer = request.Customer ?? request.ClientId }); break;
                case "production-started": db.Works.Add(new ProductionWork { Product = request.Product!.Trim(), Type = "Сборка", Quantity = (int)request.Quantity!.Value, ExpectedRevenue = request.UnitPrice ?? 0, DueDate = DateTime.UtcNow.AddDays(7), Status = WorkStatus.InProgress }); break;
                case "production-progress":
                    var work = db.Works.Where(w => w.Status != WorkStatus.Done).OrderByDescending(w => w.Id).ToList().FirstOrDefault(w => w.Product.Equals(request.Product!.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (work is null) { applied = false; break; }
                    work.Progress = (int)request.Progress!.Value;
                    work.Status = work.Progress == 100 ? WorkStatus.Done : WorkStatus.InProgress;
                    break;
                case "purchase-ordered": db.Purchases.Add(new PurchasePlan { Resource = request.Resource!.Trim(), Quantity = request.Quantity!.Value, UnitPrice = request.UnitPrice!.Value, Supplier = request.Supplier ?? request.ClientId, DueDate = DateTime.UtcNow.AddDays(7), WorkId = request.WorkId is > 0 && db.Works.Any(w => w.Id == request.WorkId) ? request.WorkId : null, Status = PurchaseStatus.Ordered }); break;
            }
            if (applied) db.ImportedClientEvents.Add(new ImportedClientEvent { ClientId = request.ClientId, EventId = request.EventId!, EventType = type, CapturedAt = request.CapturedAt?.UtcDateTime ?? DateTime.UtcNow });
        });
        if (!applied) error = "active production work not found";
        return applied;
    }

    public int ImportEsiSales(string clientId, IReadOnlyList<ProductionEventRequest> sales)
    {
        if (sales.Count == 0) return 0;
        var imported = 0;
        Commit(db =>
        {
            var known = db.ImportedClientEvents.AsNoTracking().Where(e => e.ClientId == clientId)
                .Select(e => e.EventId).ToHashSet(StringComparer.Ordinal);
            foreach (var sale in sales)
            {
                if (sale.ClientId != clientId || sale.EventId is null || !known.Add(sale.EventId) ||
                    sale.Product is null || sale.Quantity is not >= 1 || sale.Quantity > int.MaxValue ||
                    sale.UnitPrice is not >= 0) continue;
                db.Sales.Add(new SaleRecord { Product = sale.Product, Quantity = (int)sale.Quantity.Value,
                    UnitPrice = sale.UnitPrice.Value, UnitCost = null,
                    Date = sale.CapturedAt?.UtcDateTime ?? DateTime.UtcNow,
                    Customer = sale.Customer ?? "EVE ESI" });
                db.ImportedClientEvents.Add(new ImportedClientEvent { ClientId = clientId,
                    EventId = sale.EventId, EventType = "sale",
                    CapturedAt = sale.CapturedAt?.UtcDateTime ?? DateTime.UtcNow });
                imported++;
            }
        });
        return imported;
    }

    private void Commit(Action<ProductionDbContext> operation)
    {
        bool changed;
        lock (gate)
        {
            using var db = dbFactory.CreateDbContext();
            using var transaction = db.Database.BeginTransaction();
            operation(db);
            changed = db.ChangeTracker.HasChanges();
            if (changed)
            {
                db.SaveChanges();
                transaction.Commit();
                Reload(db);
            }
        }
        if (changed) Changed?.Invoke();
    }

    private void Reload(ProductionDbContext db)
    {
        Resources.Clear(); Resources.AddRange(db.Resources.AsNoTracking().OrderByDescending(x => x.Id));
        Works.Clear(); Works.AddRange(db.Works.AsNoTracking().OrderByDescending(x => x.Id));
        Purchases.Clear(); Purchases.AddRange(db.Purchases.AsNoTracking().OrderByDescending(x => x.Id));
        Sales.Clear(); Sales.AddRange(db.Sales.AsNoTracking().OrderByDescending(x => x.Id));
        MonthlyNetProfitGoal = db.Settings.AsNoTracking().FirstOrDefault(x => x.Id == 1)?.MonthlyNetProfitGoal ?? DefaultMonthlyNetProfitGoal;
    }

    private static bool IsThisMonth(SaleRecord sale) => sale.Date.Year == DateTime.Today.Year && sale.Date.Month == DateTime.Today.Month;
    private static ResourceItem Clone(ResourceItem x) => new() { Id = x.Id, Name = x.Name, Category = x.Category, Quantity = x.Quantity, AverageCost = x.AverageCost, Unit = x.Unit, Status = x.Status };
    private static ProductionWork Clone(ProductionWork x) => new() { Id = x.Id, Product = x.Product, Type = x.Type, Quantity = x.Quantity, EstimatedCost = x.EstimatedCost, ExpectedRevenue = x.ExpectedRevenue, ExpectedNetProfit = x.ExpectedNetProfit, DueDate = x.DueDate, Status = x.Status, Progress = x.Progress };
    private static PurchasePlan Clone(PurchasePlan x) => new() { Id = x.Id, Resource = x.Resource, Quantity = x.Quantity, UnitPrice = x.UnitPrice, Supplier = x.Supplier, DueDate = x.DueDate, WorkId = x.WorkId, Status = x.Status };
    private static SaleRecord Clone(SaleRecord x) => new() { Id = x.Id, Product = x.Product, Quantity = x.Quantity, UnitPrice = x.UnitPrice, UnitCost = x.UnitCost, Fees = x.Fees, OtherCosts = x.OtherCosts, Date = x.Date, Customer = x.Customer };

    private static void EnsureProfitSchema(ProductionDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere) connection.Open();
        try
        {
            static HashSet<string> Columns(System.Data.Common.DbConnection connection, string table)
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info({table})";
                using var reader = command.ExecuteReader();
                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                while (reader.Read()) names.Add(reader.GetString(1));
                return names;
            }
            var settings = Columns(connection, "Settings");
            var sales = Columns(connection, "Sales");
            var works = Columns(connection, "Works");
            using var transaction = db.Database.BeginTransaction();
            if (settings.Contains("MonthlyRevenueGoal") && !settings.Contains("MonthlyNetProfitGoal"))
                db.Database.ExecuteSqlRaw("ALTER TABLE Settings RENAME COLUMN MonthlyRevenueGoal TO MonthlyNetProfitGoal");
            if (!sales.Contains("Fees")) db.Database.ExecuteSqlRaw("ALTER TABLE Sales ADD COLUMN Fees TEXT NULL");
            if (!sales.Contains("OtherCosts")) db.Database.ExecuteSqlRaw("ALTER TABLE Sales ADD COLUMN OtherCosts TEXT NULL");
            if (!works.Contains("ExpectedNetProfit")) db.Database.ExecuteSqlRaw("ALTER TABLE Works ADD COLUMN ExpectedNetProfit TEXT NULL");
            transaction.Commit();
        }
        finally { if (openedHere) connection.Close(); }
    }
}

public enum WorkStatus { Planned, InProgress, Done }
public enum PurchaseStatus { ToOrder, PartlyOrdered, Ordered }

public sealed class ResourceItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public string Unit { get; set; } = "ед.";
    public string Status { get; set; } = "В наличии";
}

public sealed class ProductionWork
{
    public int Id { get; set; }
    public string Product { get; set; } = "";
    public string Type { get; set; } = "";
    public int Quantity { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal ExpectedRevenue { get; set; }
    public decimal? ExpectedNetProfit { get; set; }
    public DateTime DueDate { get; set; }
    public WorkStatus Status { get; set; }
    public int Progress { get; set; }
}

public sealed class PurchasePlan
{
    public int Id { get; set; }
    public string Resource { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Supplier { get; set; } = "";
    public DateTime DueDate { get; set; }
    public int? WorkId { get; set; }
    public PurchaseStatus Status { get; set; }
    public decimal Total => Quantity * UnitPrice;
}

public sealed class SaleRecord
{
    public int Id { get; set; }
    public string Product { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? Fees { get; set; }
    public decimal? OtherCosts { get; set; }
    public DateTime Date { get; set; }
    public string Customer { get; set; } = "";
    public decimal Total => Quantity * UnitPrice;
    public decimal? NetProfit => UnitCost is null || Fees is null || OtherCosts is null
        ? null : Total - Quantity * UnitCost.Value - Fees.Value - OtherCosts.Value;
}

public sealed class ProductionSettings
{
    public int Id { get; set; }
    public decimal MonthlyNetProfitGoal { get; set; }
}

public sealed class ImportedClientEvent
{
    public int Id { get; set; }
    public string ClientId { get; set; } = "";
    public string EventId { get; set; } = "";
    public string EventType { get; set; } = "";
    public DateTime CapturedAt { get; set; }
}
