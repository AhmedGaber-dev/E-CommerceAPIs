using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ECommerce.Application.Common.Caching;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using MediatR;

namespace ECommerce.Application.Features.Products;

public record GetProductsQuery(ProductListParameters Parameters) : IRequest<PagedResult<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private static readonly TimeSpan ListCacheDuration = TimeSpan.FromSeconds(60);

    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly IProductCatalogCacheVersion _catalogVersion;

    public GetProductsQueryHandler(IUnitOfWork uow, ICacheService cache, IProductCatalogCacheVersion catalogVersion)
    {
        _uow = uow;
        _cache = cache;
        _catalogVersion = catalogVersion;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var versionToken = await _catalogVersion.GetTokenAsync(cancellationToken);
        var key = CacheKeys.ProductPagedList(versionToken, ComputeKey(request.Parameters));

        return await _cache.GetOrCreateAsync(
            key,
            async ct => await _uow.Products.GetPagedAsync(request.Parameters, ct),
            ListCacheDuration,
            cancellationToken);
    }

    private static string ComputeKey(ProductListParameters p)
    {
        var json = JsonSerializer.Serialize(p);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash);
    }
}
