using ECommerce.Application.Common.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerce.Infrastructure.Caching;

/// <summary>
/// Monotonic catalog generation for product list cache keys. Stored in the same distributed cache backend as read data.
/// </summary>
public sealed class ProductCatalogCacheVersion : IProductCatalogCacheVersion
{
    private readonly IDistributedCache _distributed;
    private const string VersionKey = CacheKeys.Prefix + "catalog:products:generation";

    public ProductCatalogCacheVersion(IDistributedCache distributed) => _distributed = distributed;

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var raw = await _distributed.GetStringAsync(VersionKey, cancellationToken).ConfigureAwait(false);
        return string.IsNullOrEmpty(raw) ? "0" : raw;
    }

    public async Task BumpAsync(CancellationToken cancellationToken = default)
    {
        var raw = await _distributed.GetStringAsync(VersionKey, cancellationToken).ConfigureAwait(false);
        _ = long.TryParse(raw, out var n);
        n++;
        await _distributed.SetStringAsync(VersionKey, n.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365)
        }, cancellationToken).ConfigureAwait(false);
    }
}
