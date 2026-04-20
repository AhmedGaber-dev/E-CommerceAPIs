using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Cart;

public record AddCartItemCommand(Guid ProductId, int Quantity) : IRequest;

public class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AddCartItemCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            throw new ForbiddenAccessException("Authentication required.");

        var userId = _currentUser.UserId.Value;
        var product = await _uow.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException(nameof(Product), request.ProductId);

        var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId, cancellationToken);
        if (cart == null)
        {
            cart = new Domain.Entities.Cart { Id = Guid.NewGuid(), UserId = userId, CreatedAtUtc = DateTime.UtcNow };
            await _uow.Carts.AddAsync(cart, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            cart = (await _uow.Carts.GetByUserIdWithItemsAsync(userId, cancellationToken))!;
        }
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        var newQty = (existing?.Quantity ?? 0) + request.Quantity;
        if (newQty > product.StockQuantity)
            throw new AppValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Quantity), "Quantity exceeds available stock.")
            });

        if (existing != null)
        {
            existing.Quantity = newQty;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            _uow.CartItems.Update(existing);
        }
        else
        {
            await _uow.CartItems.AddAsync(new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                CreatedAtUtc = DateTime.UtcNow
            }, cancellationToken);
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
