using ECommerce.Application.Common.Caching;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Products;

public record UpdateProductCommand(Guid Id, string Name, string? Description, decimal Price, int StockQuantity, string Sku, IReadOnlyList<Guid> CategoryIds)
    : IRequest;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly IProductCatalogCacheVersion _catalogVersion;

    public UpdateProductCommandHandler(IUnitOfWork uow, ICacheService cache, IProductCatalogCacheVersion catalogVersion)
    {
        _uow = uow;
        _cache = cache;
        _catalogVersion = catalogVersion;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _uow.Products.GetByIdWithCategoriesAsync(request.Id, cancellationToken);
        if (product == null)
            throw new NotFoundException(nameof(Product), request.Id);

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.Sku = request.Sku.Trim();
        product.UpdatedAtUtc = DateTime.UtcNow;

        product.ProductCategories.Clear();
        foreach (var categoryId in request.CategoryIds.Distinct())
        {
            var cat = await _uow.Categories.GetByIdAsync(categoryId, cancellationToken);
            if (cat == null)
                throw new AppValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(request.CategoryIds), $"Category {categoryId} not found.")
                });
            product.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = categoryId });
        }

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.ProductDetail(product.Id), cancellationToken);
        await _catalogVersion.BumpAsync(cancellationToken);
    }
}
