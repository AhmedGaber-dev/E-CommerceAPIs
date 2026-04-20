using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Users;

public record UpdateUserCommand(Guid Id, string FirstName, string LastName, string? RoleName) : IRequest;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUnitOfWork _uow;

    public UpdateUserCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdWithRoleAsync(request.Id, cancellationToken);
        if (user == null)
            throw new NotFoundException(nameof(Domain.Entities.User), request.Id);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.UpdatedAtUtc = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.RoleName))
        {
            var roles = await _uow.Roles.ListAsync(r => r.Name == request.RoleName.Trim(), cancellationToken);
            var role = roles.FirstOrDefault();
            if (role == null)
                throw new AppValidationException(new[] { new FluentValidation.Results.ValidationFailure(nameof(request.RoleName), "Invalid role.") });
            user.RoleId = role.Id;
        }

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
