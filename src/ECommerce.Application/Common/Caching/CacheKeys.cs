namespace ECommerce.Application.Common.Caching;

/// <summary>Central cache key fragments to avoid typos and ease Redis prefixing.</summary>
public static class CacheKeys
{
    public const string Prefix = "ecommerce:";

    public static string ProductDetail(Guid productId) => $"{Prefix}product:detail:{productId:D}";

    public static string ProductPagedList(string catalogVersionToken, string parameterHash) =>
        $"{Prefix}products:paged:{catalogVersionToken}:{parameterHash}";
}
