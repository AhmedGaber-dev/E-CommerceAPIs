using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Users;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await DbSet.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByIdWithRoleAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<PagedResult<User>> GetPagedAsync(UserListParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(u => u.Role).AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var s = parameters.Search.Trim();
            query = query.Where(u =>
                u.Email.Contains(s) ||
                u.FirstName.Contains(s) ||
                u.LastName.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Role))
        {
            var role = parameters.Role.Trim();
            query = query.Where(u => u.Role.Name == role);
        }

        query = (parameters.SortBy?.ToLowerInvariant()) switch
        {
            "email" => parameters.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "role" => parameters.SortDescending ? query.OrderByDescending(u => u.Role.Name) : query.OrderBy(u => u.Role.Name),
            "created" => parameters.SortDescending ? query.OrderByDescending(u => u.CreatedAtUtc) : query.OrderBy(u => u.CreatedAtUtc),
            _ => parameters.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedResult<User>
        {
            Items = items,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = total
        };
    }
}
