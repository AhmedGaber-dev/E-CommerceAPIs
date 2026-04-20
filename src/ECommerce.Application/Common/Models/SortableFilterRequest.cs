namespace ECommerce.Application.Common.Models;

/// <summary>
/// Shared sorting and optional search for list endpoints.
/// </summary>
public class SortableFilterRequest : PagedRequest
{
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
