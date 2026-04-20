using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence;

public static class ApplicationDbContextSeed
{
    /// <summary>Idempotent data seed. Run database migrations separately (e.g. CI/CD or controlled startup) before calling this.</summary>
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (!await context.Roles.AnyAsync(cancellationToken))
        {
            var adminRole = new Role { Id = Guid.NewGuid(), Name = Roles.Admin, CreatedAtUtc = DateTime.UtcNow };
            var userRole = new Role { Id = Guid.NewGuid(), Name = Roles.User, CreatedAtUtc = DateTime.UtcNow };
            await context.Roles.AddRangeAsync(new[] { adminRole, userRole }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var admin = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@ecommerce.local",
                // Password: Admin123!
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", workFactor: 12),
                FirstName = "System",
                LastName = "Administrator",
                RoleId = adminRole.Id,
                CreatedAtUtc = DateTime.UtcNow
            };
            await context.Users.AddAsync(admin, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded roles and admin user (admin@ecommerce.local / Admin123!).");
        }

        if (!await context.Categories.AnyAsync(cancellationToken))
        {
            var electronics = new Category { Id = Guid.NewGuid(), Name = "Electronics", Description = "Devices and accessories", CreatedAtUtc = DateTime.UtcNow };
            var books = new Category { Id = Guid.NewGuid(), Name = "Books", Description = "Physical and digital titles", CreatedAtUtc = DateTime.UtcNow };
            await context.Categories.AddRangeAsync(new[] { electronics, books }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var p1 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Wireless Headphones",
                Description = "Noise-cancelling over-ear headphones",
                Price = 199.99m,
                StockQuantity = 50,
                Sku = "WH-1000",
                CreatedAtUtc = DateTime.UtcNow
            };
            var p2 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Paperback: Clean Architecture",
                Description = "A Craftsman's Guide to Software Structure and Design",
                Price = 42.00m,
                StockQuantity = 120,
                Sku = "BOOK-CA-01",
                CreatedAtUtc = DateTime.UtcNow
            };
            var p3 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "E-Reader Pro",
                Description = "Backlit e-reader with weeks of battery life",
                Price = 129.00m,
                StockQuantity = 35,
                Sku = "ER-PRO-9",
                CreatedAtUtc = DateTime.UtcNow
            };
            await context.Products.AddRangeAsync(new[] { p1, p2, p3 }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            context.ProductCategories.AddRange(
                new ProductCategory { ProductId = p1.Id, CategoryId = electronics.Id },
                new ProductCategory { ProductId = p2.Id, CategoryId = books.Id },
                new ProductCategory { ProductId = p3.Id, CategoryId = electronics.Id },
                new ProductCategory { ProductId = p3.Id, CategoryId = books.Id });

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded sample categories and products.");
        }
    }
}
