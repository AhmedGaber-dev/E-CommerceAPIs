using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Cart;

public record RemoveCartItemCommand(Guid CartItemId) : IRequest;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RemoveCartItemCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            throw new ForbiddenAccessException("Authentication required.");

        var item = await _uow.CartItems.GetByIdAsync(request.CartItemId, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(Domain.Entities.CartItem), request.CartItemId);

        var cart = await _uow.Carts.GetByIdAsync(item.CartId, cancellationToken);
        if (cart == null || cart.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        _uow.CartItems.Remove(item);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
