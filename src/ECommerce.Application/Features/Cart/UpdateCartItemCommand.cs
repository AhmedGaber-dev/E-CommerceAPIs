using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Cart;

public record UpdateCartItemCommand(Guid CartItemId, int Quantity) : IRequest;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateCartItemCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            throw new ForbiddenAccessException("Authentication required.");

        var item = await _uow.CartItems.GetByIdAsync(request.CartItemId, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(Domain.Entities.CartItem), request.CartItemId);

        var cart = await _uow.Carts.GetByIdAsync(item.CartId, cancellationToken);
        if (cart == null || cart.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        var product = await _uow.Products.GetByIdAsync(item.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException(nameof(Domain.Entities.Product), item.ProductId);

        if (request.Quantity > product.StockQuantity)
            throw new AppValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Quantity), "Quantity exceeds available stock.")
            });

        if (request.Quantity <= 0)
        {
            _uow.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = request.Quantity;
            item.UpdatedAtUtc = DateTime.UtcNow;
            _uow.CartItems.Update(item);
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
