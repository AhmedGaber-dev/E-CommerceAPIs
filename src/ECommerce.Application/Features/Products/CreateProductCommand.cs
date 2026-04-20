using ECommerce.Application.Common.Caching;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Products;

public record CreateProductCommand(string Name, string? Description, decimal Price, int StockQuantity, string Sku, IReadOnlyList<Guid> CategoryIds)
    : IRequest<Guid>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly IProductCatalogCacheVersion _catalogVersion;

    public CreateProductCommandHandler(IUnitOfWork uow, ICacheService cache, IProductCatalogCacheVersion catalogVersion)
    {
        _uow = uow;
        _cache = cache;
        _catalogVersion = catalogVersion;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Sku = request.Sku.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var categoryId in request.CategoryIds.Distinct())
        {
            var cat = await _uow.Categories.GetByIdAsync(categoryId, cancellationToken);
            if (cat == null)
                throw new Common.Exceptions.AppValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(request.CategoryIds), $"Category {categoryId} not found.")
                });
            product.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = categoryId });
        }

        await _uow.Products.AddAsync(product, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.ProductDetail(product.Id), cancellationToken);
        await _catalogVersion.BumpAsync(cancellationToken);
        return product.Id;
    }
}
