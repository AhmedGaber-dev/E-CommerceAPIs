using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Orders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsSplitQuery()
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<PagedResult<Order>> GetPagedAsync(OrderListParameters parameters, Guid? forUserId, CancellationToken cancellationToken = default)
    {
        // Avoid cartesian explosion: filter/count/order without includes, then load the page graph with split queries.
        var query = DbSet.AsNoTracking().AsQueryable();

        if (forUserId.HasValue)
            query = query.Where(o => o.UserId == forUserId.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            Enum.TryParse<OrderStatus>(parameters.Status, true, out var status))
            query = query.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var s = parameters.Search.Trim();
            query = query.Where(o =>
                o.Id.ToString().Contains(s) ||
                o.OrderItems.Any(i => i.Product.Name.Contains(s)));
        }

        query = (parameters.SortBy?.ToLowerInvariant()) switch
        {
            "status" => parameters.SortDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
            "total" => parameters.SortDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
            "created" => parameters.SortDescending ? query.OrderByDescending(o => o.CreatedAtUtc) : query.OrderBy(o => o.CreatedAtUtc),
            _ => parameters.SortDescending ? query.OrderByDescending(o => o.CreatedAtUtc) : query.OrderBy(o => o.CreatedAtUtc)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>
        {
            Items = items,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = total
        };
    }
}
