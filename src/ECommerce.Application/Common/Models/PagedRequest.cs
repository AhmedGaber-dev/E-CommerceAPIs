namespace ECommerce.Application.Common.Models;

public class PagedRequest
{
    private int _page = 1;
    private int _pageSize = 10;

    /// <summary>1-based page index.</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > 100 ? 10 : value;
    }
}
