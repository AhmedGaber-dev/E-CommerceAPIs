namespace ECommerce.Application.Common.Caching;

/// <summary>
/// Cache abstraction so read paths can use in-memory, Redis, or other providers without changing handlers.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
