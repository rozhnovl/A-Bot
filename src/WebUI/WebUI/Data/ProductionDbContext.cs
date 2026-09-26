using Microsoft.EntityFrameworkCore;
using WebUI.Esi;

namespace WebUI.Data;

public sealed class ProductionDbContext(DbContextOptions<ProductionDbContext> options) : DbContext(options)
{
    public DbSet<ResourceItem> Resources => Set<ResourceItem>();
    public DbSet<ProductionWork> Works => Set<ProductionWork>();
    public DbSet<PurchasePlan> Purchases => Set<PurchasePlan>();
    public DbSet<SaleRecord> Sales => Set<SaleRecord>();
    public DbSet<ProductionSettings> Settings => Set<ProductionSettings>();
    public DbSet<ImportedClientEvent> ImportedClientEvents => Set<ImportedClientEvent>();
    public DbSet<EsiStoredPage> EsiPages => Set<EsiStoredPage>();
    public DbSet<EsiStoredRecord> EsiRecords => Set<EsiStoredRecord>();
    public DbSet<EsiStoredGrant> EsiGrants => Set<EsiStoredGrant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ResourceItem>().Property(x => x.AverageCost).HasPrecision(18, 2);
        modelBuilder.Entity<ProductionWork>().Property(x => x.EstimatedCost).HasPrecision(18, 2);
        modelBuilder.Entity<ProductionWork>().Property(x => x.ExpectedNetProfit).HasPrecision(18, 2);
        modelBuilder.Entity<PurchasePlan>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<SaleRecord>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<SaleRecord>().Property(x => x.UnitCost).HasPrecision(18, 2);
        modelBuilder.Entity<SaleRecord>().Property(x => x.Fees).HasPrecision(18, 2);
        modelBuilder.Entity<SaleRecord>().Property(x => x.OtherCosts).HasPrecision(18, 2);
        modelBuilder.Entity<ProductionWork>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<PurchasePlan>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<ImportedClientEvent>().HasIndex(x => new { x.ClientId, x.EventId }).IsUnique();
        modelBuilder.Entity<EsiStoredPage>().HasIndex(x => new { x.CharacterId, x.Resource, x.Cursor }).IsUnique();
        modelBuilder.Entity<EsiStoredRecord>().HasIndex(x => new { x.CharacterId, x.Kind, x.RecordId }).IsUnique();
        modelBuilder.Entity<EsiStoredGrant>().HasKey(x => x.CharacterId);
    }
}
