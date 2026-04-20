using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Products;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    /// <summary>SQL projection for list/grid reads — avoids hydrating full aggregates and reduces payload.</summary>
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductListParameters parameters, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdWithCategoriesAsync(Guid id, CancellationToken cancellationToken = default);
}
