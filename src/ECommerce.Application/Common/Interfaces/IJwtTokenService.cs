using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(User user, CancellationToken cancellationToken = default);
}
