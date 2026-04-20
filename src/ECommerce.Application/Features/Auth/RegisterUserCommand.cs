using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Auth;

public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName) : IRequest<Guid>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IUnitOfWork uow, IPasswordHasher passwordHasher)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _uow.Users.GetByEmailAsync(request.Email, cancellationToken) != null)
            throw new Common.Exceptions.AppValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Email), "Email is already registered.")
            });

        var roles = await _uow.Roles.ListAsync(r => r.Name == Roles.User, cancellationToken);
        var userRole = roles.FirstOrDefault() ?? throw new InvalidOperationException("User role is not seeded.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            RoleId = userRole.Id,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _uow.Users.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
