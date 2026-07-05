using Microsoft.EntityFrameworkCore;
using Play_Zone_BackEnd.Models;

namespace Play_Zone_BackEnd.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<PriceConfig> PriceConfigs => Set<PriceConfig>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionModeLog> SessionModeLogs => Set<SessionModeLog>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReceiptModeEntry> ReceiptModeEntries => Set<ReceiptModeEntry>();
    public DbSet<ReceiptOrderItem> ReceiptOrderItems => Set<ReceiptOrderItem>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }

        modelBuilder.Entity<Device>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Games).HasMaxLength(1000);
        });

        modelBuilder.Entity<PriceConfig>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DeviceType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Mode).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.PricePerHour).HasColumnType("decimal(18,2)");
            e.HasIndex(p => new { p.DeviceType, p.Mode }).IsUnique();
        });

        modelBuilder.Entity<Session>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DeviceName).HasMaxLength(200);
            e.Property(x => x.DeviceType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.HasIndex(x => x.DeviceId);
            e.HasIndex(x => x.Status);
            e.HasOne<Device>().WithMany().HasForeignKey(x => x.DeviceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SessionModeLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Mode).HasConversion<string>().HasMaxLength(20);
            e.HasOne<Session>().WithMany(s => s.ModeLogs).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.SessionId);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.HasOne<Session>().WithMany(s => s.OrderItems).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.SessionId);
        });

        modelBuilder.Entity<Receipt>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DeviceName).HasMaxLength(200);
            e.Property(x => x.DeviceType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TotalDuration).HasColumnType("time");
            e.Property(x => x.PaymentMethod).HasMaxLength(50);
            e.Property(x => x.TimeCharge).HasColumnType("decimal(18,2)");
            e.Property(x => x.OrdersTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
            e.HasOne<Session>().WithMany().HasForeignKey(x => x.SessionId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => x.SessionId);
        });

        modelBuilder.Entity<ReceiptModeEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Mode).HasMaxLength(20);
            e.Property(x => x.Duration).HasMaxLength(20);
            e.Property(x => x.Charge).HasColumnType("decimal(18,2)");
            e.HasOne<Receipt>().WithMany(r => r.ModeEntries).HasForeignKey(x => x.ReceiptId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReceiptOrderItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.HasOne<Receipt>().WithMany(r => r.OrderItems).HasForeignKey(x => x.ReceiptId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SiteSetting>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).HasMaxLength(100);
            e.Property(x => x.Value).HasMaxLength(4000);
            e.HasIndex(x => x.Key).IsUnique();
        });

        modelBuilder.Entity<ServiceRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DeviceName).HasMaxLength(200);
            e.Property(x => x.RequestType).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Status).HasMaxLength(20);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.CreatedAt);
        });
    }
}
