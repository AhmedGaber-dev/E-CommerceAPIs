using System.Text.Json;
using System.Text.Json.Serialization;
using ECommerce.Application.Common.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerce.Infrastructure.Caching;

/// <summary>
/// JSON-backed distributed cache: swap <see cref="IDistributedCache"/> implementation to Redis for multi-instance deployments.
/// </summary>
public sealed class DistributedJsonCacheService : ICacheService
{
    private readonly IDistributedCache _distributed;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public DistributedJsonCacheService(IDistributedCache distributed) => _distributed = distributed;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await _distributed.GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (bytes == null || bytes.Length == 0)
            return default;
        return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        await _distributed.SetAsync(key, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
        }, cancellationToken).ConfigureAwait(false);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        _distributed.RemoveAsync(key, cancellationToken);
}
