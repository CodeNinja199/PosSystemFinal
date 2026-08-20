using Microsoft.EntityFrameworkCore;
using Pos.Domain.Entities;

namespace Pos.Infrastructure.Data;

// Registered in Program.cs with AddDbContext, which makes it scoped: one context per request.
// Used by the EF Core repositories; the dotnet-ef tool reads it to build migrations.
public class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories
    {
        get
        {
            return Set<Category>();
        }
    }

    public DbSet<Product> Products
    {
        get
        {
            return Set<Product>();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .Property(category => category.Name)
            .HasMaxLength(60);

        modelBuilder.Entity<Product>()
            .Property(product => product.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Product>()
            .Property(product => product.Price)
            .HasPrecision(18, 2);

        // Product.CategoryId is a foreign key to Category; the database refuses to delete a category that still has products.
        modelBuilder.Entity<Product>()
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}