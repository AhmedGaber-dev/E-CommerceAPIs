using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using MediatR;

namespace ECommerce.Application.Features.Orders;

/// <summary>Creates an order from the current user's cart and clears the cart on success.</summary>
public record CreateOrderFromCartCommand : IRequest<Guid>;

public class CreateOrderFromCartCommandHandler : IRequestHandler<CreateOrderFromCartCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateOrderFromCartCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateOrderFromCartCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            throw new ForbiddenAccessException("Authentication required.");

        var userId = _currentUser.UserId.Value;
        var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId, cancellationToken);
        if (cart == null || cart.Items.Count == 0)
            throw new AppValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(string.Empty, "Cart is empty.")
            });

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = 0,
            CreatedAtUtc = DateTime.UtcNow
        };

        decimal total = 0;
        foreach (var line in cart.Items)
        {
            var product = await _uow.Products.GetByIdAsync(line.ProductId, cancellationToken);
            if (product == null)
                throw new AppValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(line.ProductId), "Product no longer exists.")
                });
            if (product.StockQuantity < line.Quantity)
                throw new AppValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(line.Quantity), $"Insufficient stock for {product.Name}.")
                });

            var unitPrice = product.Price;
            total += unitPrice * line.Quantity;

            order.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                CreatedAtUtc = DateTime.UtcNow
            });

            product.StockQuantity -= line.Quantity;
            product.UpdatedAtUtc = DateTime.UtcNow;
            _uow.Products.Update(product);
        }

        order.TotalAmount = total;
        await _uow.Orders.AddAsync(order, cancellationToken);

        foreach (var line in cart.Items.ToList())
            _uow.CartItems.Remove(line);

        await _uow.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}
