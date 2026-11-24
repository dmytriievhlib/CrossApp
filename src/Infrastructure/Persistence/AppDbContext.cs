using Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductRow> Products => Set<ProductRow>();

    public DbSet<BatchRow> Batches => Set<BatchRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductRow>(entity =>
        {
            entity.ToTable("Products");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(p => p.Sku)
                .IsUnique();

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(p => p.Unit)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasMany(p => p.Batches)
                .WithOne(b => b.Product)
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BatchRow>(entity =>
        {
            entity.ToTable("Batches");

            entity.HasKey(b => b.Id);

            entity.Property(b => b.ProductId)
                .IsRequired();

            entity.Property(b => b.Amount)
                .IsRequired();

            entity.Property(b => b.ReceivedAt)
                .IsRequired();
        });
    }
}