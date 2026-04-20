namespace ECommerce.Domain.Common;

/// <summary>
/// Base type for persisted entities with audit timestamps.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
