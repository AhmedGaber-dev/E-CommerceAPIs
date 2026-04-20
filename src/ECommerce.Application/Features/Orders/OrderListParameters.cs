using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Features.Orders;

public class OrderListParameters : SortableFilterRequest
{
    /// <summary>Filter by status name (Pending, Paid, Shipped, Cancelled).</summary>
    public string? Status { get; set; }

    public Guid? UserId { get; set; }
}
