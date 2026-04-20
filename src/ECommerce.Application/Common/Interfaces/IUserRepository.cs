using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Users;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<User>> GetPagedAsync(UserListParameters parameters, CancellationToken cancellationToken = default);
}
