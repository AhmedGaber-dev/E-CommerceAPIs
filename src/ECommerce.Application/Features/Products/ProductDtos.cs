namespace ECommerce.Application.Features.Products;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Sku { get; set; } = string.Empty;
    public IReadOnlyList<Guid> CategoryIds { get; set; } = Array.Empty<Guid>();
    public IReadOnlyList<string> CategoryNames { get; set; } = Array.Empty<string>();
    public DateTime CreatedAtUtc { get; set; }
}
