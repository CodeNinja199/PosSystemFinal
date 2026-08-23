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

    public DbSet<Store> Stores
    {
        get
        {
            return Set<Store>();
        }
    }

    public DbSet<Category> Categories
    {
        get
        {
            return Set<Category>();
        }
    }

    public DbSet<User> Users
    {
        get
        {
            return Set<User>();
        }
    }

    public DbSet<Product> Products
    {
        get
        {
            return Set<Product>();
        }
    }

    public DbSet<Order> Orders
    {
        get
        {
            return Set<Order>();
        }
    }

    public DbSet<OrderItem> OrderItems
    {
        get
        {
            return Set<OrderItem>();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Store>()
            .Property(store => store.Name)
            .HasMaxLength(100);

        // Every tenant-owned row points at its store; stores are never deleted, so Restrict only documents the intent.
        modelBuilder.Entity<User>()
            .HasOne<Store>()
            .WithMany()
            .HasForeignKey(user => user.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasOne<Store>()
            .WithMany()
            .HasForeignKey(category => category.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne<Store>()
            .WithMany()
            .HasForeignKey(product => product.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne<Store>()
            .WithMany()
            .HasForeignKey(order => order.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .Property(user => user.FullName)
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(user => user.Email)
            .HasMaxLength(200);

        // Two users can never share an email; the database enforces what AuthService checks first.
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

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

        modelBuilder.Entity<Order>()
            .Property(order => order.Total)
            .HasPrecision(18, 2);

        // An order belongs to a user; users are never deleted, so Restrict only documents the intent.
        modelBuilder.Entity<Order>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .Property(orderItem => orderItem.ProductName)
            .HasMaxLength(100);

        modelBuilder.Entity<OrderItem>()
            .Property(orderItem => orderItem.UnitPrice)
            .HasPrecision(18, 2);

        // An order item points at its product; the database refuses to delete a product that appears in an order.
        modelBuilder.Entity<OrderItem>()
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(orderItem => orderItem.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}