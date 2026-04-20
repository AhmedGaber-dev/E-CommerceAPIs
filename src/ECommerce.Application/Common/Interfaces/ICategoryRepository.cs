using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Categories;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<PagedResult<Category>> GetPagedAsync(CategoryListParameters parameters, CancellationToken cancellationToken = default);
}
