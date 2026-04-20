using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Orders;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<PagedResult<Order>> GetPagedAsync(OrderListParameters parameters, Guid? forUserId, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
}
