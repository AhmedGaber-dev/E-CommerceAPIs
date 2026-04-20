using AutoMapper;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Constants;
using MediatR;

namespace ECommerce.Application.Features.Orders;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetOrderByIdQueryHandler(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _uow.Orders.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (order == null)
            throw new NotFoundException(nameof(Domain.Entities.Order), request.Id);

        if (!_currentUser.IsInRole(Roles.Admin) && order.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        return _mapper.Map<OrderDto>(order);
    }
}
