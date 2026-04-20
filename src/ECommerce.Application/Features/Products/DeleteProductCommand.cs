using ECommerce.Application.Common.Caching;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Products;

public record DeleteProductCommand(Guid Id) : IRequest;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly IProductCatalogCacheVersion _catalogVersion;

    public DeleteProductCommandHandler(IUnitOfWork uow, ICacheService cache, IProductCatalogCacheVersion catalogVersion)
    {
        _uow = uow;
        _cache = cache;
        _catalogVersion = catalogVersion;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _uow.Products.GetByIdWithCategoriesAsync(request.Id, cancellationToken);
        if (product == null)
            throw new NotFoundException(nameof(Domain.Entities.Product), request.Id);

        _uow.Products.Remove(product);
        await _uow.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.ProductDetail(request.Id), cancellationToken);
        await _catalogVersion.BumpAsync(cancellationToken);
    }
}
