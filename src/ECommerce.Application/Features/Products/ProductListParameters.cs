using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Features.Products;

public class ProductListParameters : SortableFilterRequest
{
    /// <summary>Optional category filter (product must be linked to this category).</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Minimum unit price (inclusive).</summary>
    public decimal? MinPrice { get; set; }

    /// <summary>Maximum unit price (inclusive).</summary>
    public decimal? MaxPrice { get; set; }

    // SortBy: name, price, created, stock
}
