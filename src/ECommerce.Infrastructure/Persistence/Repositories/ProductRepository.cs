using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Products;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByIdWithCategoriesAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsSplitQuery()
            .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductListParameters parameters, CancellationToken cancellationToken = default)
    {
        // Filters and sorting on a lightweight query — no graph hydration until the final page projection.
        var query = DbSet.AsNoTracking().AsQueryable();

        if (parameters.CategoryId.HasValue)
        {
            var cid = parameters.CategoryId.Value;
            query = query.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == cid));
        }

        if (parameters.MinPrice.HasValue)
            query = query.Where(p => p.Price >= parameters.MinPrice.Value);
        if (parameters.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var s = parameters.Search.Trim();
            query = query.Where(p => p.Name.Contains(s) || p.Sku.Contains(s) ||
                                     (p.Description != null && p.Description.Contains(s)));
        }

        query = (parameters.SortBy?.ToLowerInvariant()) switch
        {
            "name" => parameters.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => parameters.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "stock" => parameters.SortDescending ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
            "created" => parameters.SortDescending ? query.OrderByDescending(p => p.CreatedAtUtc) : query.OrderBy(p => p.CreatedAtUtc),
            _ => parameters.SortDescending ? query.OrderByDescending(p => p.CreatedAtUtc) : query.OrderBy(p => p.CreatedAtUtc)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Sku = p.Sku,
                CreatedAtUtc = p.CreatedAtUtc,
                CategoryIds = p.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                CategoryNames = p.ProductCategories.Select(pc => pc.Category.Name).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = items,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = total
        };
    }
}
