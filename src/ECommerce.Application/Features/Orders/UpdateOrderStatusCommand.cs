using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Enums;
using MediatR;

namespace ECommerce.Application.Features.Orders;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IUnitOfWork _uow;

    public UpdateOrderStatusCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _uow.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException(nameof(Domain.Entities.Order), request.OrderId);

        order.Status = request.Status;
        order.UpdatedAtUtc = DateTime.UtcNow;
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
