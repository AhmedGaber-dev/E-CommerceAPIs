using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

/// <summary>
/// Unit of Work coordinates repositories and a single SaveChanges boundary.
/// Exposes module repositories plus generic access for join/aggregate entities.
/// </summary>
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IOrderRepository Orders { get; }
    ICartRepository Carts { get; }

    IGenericRepository<Role> Roles { get; }
    IGenericRepository<CartItem> CartItems { get; }
    IGenericRepository<OrderItem> OrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
