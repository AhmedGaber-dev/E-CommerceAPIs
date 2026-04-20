namespace ECommerce.Application.Common.Caching;

public static class CacheServiceExtensions
{
    public static async Task<T> GetOrCreateAsync<T>(
        this ICacheService cache,
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var existing = await cache.GetAsync<T>(key, cancellationToken);
        if (existing != null)
            return existing;

        var created = await factory(cancellationToken);
        await cache.SetAsync(key, created, absoluteExpirationRelativeToNow, cancellationToken);
        return created;
    }
}
