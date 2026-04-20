using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Cart;

public record ClearCartCommand : IRequest;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ClearCartCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            throw new ForbiddenAccessException("Authentication required.");

        var cart = await _uow.Carts.GetByUserIdWithItemsAsync(_currentUser.UserId.Value, cancellationToken);
        if (cart == null)
            return;

        foreach (var line in cart.Items.ToList())
            _uow.CartItems.Remove(line);

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
