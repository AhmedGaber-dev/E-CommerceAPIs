namespace ECommerce.Application.Common.Caching;

/// <summary>
/// Increments a logical "catalog generation" so all product list cache keys rotate without enumerating entries
/// (impossible with most distributed caches). Call after any change that affects list projections.
/// </summary>
public interface IProductCatalogCacheVersion
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);

    Task BumpAsync(CancellationToken cancellationToken = default);
}
