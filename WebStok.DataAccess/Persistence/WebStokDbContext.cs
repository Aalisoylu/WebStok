using Microsoft.EntityFrameworkCore;
using WebStok.Domain.Entities;

namespace WebStok.DataAccess.Persistence;

public class WebStokDbContext : DbContext
{
    public WebStokDbContext(
        DbContextOptions<WebStokDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Lot> Lots => Set<Lot>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<OrganizationalUnit> OrganizationalUnits =>
    Set<OrganizationalUnit>();

    public DbSet<UserWarehouse> UserWarehouses =>
    Set<UserWarehouse>();

    public DbSet<UserSession> UserSessions =>
    Set<UserSession>();


    public DbSet<StockOperation> StockOperations =>
    Set<StockOperation>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<StockMovement>()
    .HasOne<StockMovement>()
    .WithMany()
    .HasForeignKey(x => x.RelatedMovementId)
    .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<StockOperation>(entity =>
{
    entity.HasKey(x => x.Id);

    entity.Property(x => x.Type).IsRequired();
    entity.Property(x => x.RequestHash).IsRequired();

    entity.HasOne<AppUser>()
        .WithMany()
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Restrict);
});

modelBuilder.Entity<StockMovement>()
    .HasOne<StockOperation>()
    .WithMany()
    .HasForeignKey(x => x.OperationId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<OrganizationalUnit>(entity =>
{
    entity.Property(x => x.Code).IsRequired();
    entity.Property(x => x.Name).IsRequired();

    entity.HasIndex(x => x.Code).IsUnique();

    entity.HasOne(x => x.Parent)
        .WithMany()
        .HasForeignKey(x => x.ParentId)
        .OnDelete(DeleteBehavior.Restrict);
});

modelBuilder.Entity<AppUser>(entity =>
{
    entity.Property(x => x.Username).IsRequired();
    entity.Property(x => x.NormalizedUsername).IsRequired();
    entity.Property(x => x.DisplayName).IsRequired();
    entity.Property(x => x.PasswordHash).IsRequired();

    entity.HasIndex(x => x.NormalizedUsername).IsUnique();

    entity.HasOne(x => x.OrganizationalUnit)
        .WithMany()
        .HasForeignKey(x => x.OrganizationalUnitId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(x => x.Manager)
        .WithMany()
        .HasForeignKey(x => x.ManagerId)
        .OnDelete(DeleteBehavior.Restrict);
});

modelBuilder.Entity<UserWarehouse>(entity =>
{
    entity.HasKey(x => new { x.UserId, x.WarehouseId });

    entity.HasOne(x => x.User)
        .WithMany()
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(x => x.Warehouse)
        .WithMany()
        .HasForeignKey(x => x.WarehouseId)
        .OnDelete(DeleteBehavior.Restrict);
});

modelBuilder.Entity<UserSession>(entity =>
{
    entity.HasKey(x => x.Id);

    entity.HasIndex(x => x.UserId);
    entity.HasIndex(x => x.ExpiresAtUtc);

    entity.HasOne(x => x.User)
        .WithMany()
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Restrict);
});

modelBuilder.Entity<StockMovement>()
    .HasOne<AppUser>()
    .WithMany()
    .HasForeignKey(x => x.PerformedByUserId)
    .OnDelete(DeleteBehavior.Restrict);



        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(x => x.Name).IsRequired();

            entity.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.Property(x => x.Code).IsRequired();
            entity.Property(x => x.Name).IsRequired();

            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(x => x.Code).IsRequired();
            entity.Property(x => x.Barcode).IsRequired();
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Unit).IsRequired();

            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Barcode).IsUnique();

            entity.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Lot>(entity =>
        {
            entity.Property(x => x.LotNumber).IsRequired();

            entity.HasIndex(x => new { x.ProductId, x.LotNumber })
                .IsUnique();

            entity.HasOne(x => x.Product)
                .WithMany(x => x.Lots)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.Property(x => x.Description).IsRequired();

            entity.HasIndex(x => new { x.WarehouseId, x.LotId });
            entity.HasIndex(x => x.OperationId);

            entity.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Lot)
                .WithMany()
                .HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}