using AutoMapper;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Domain.Constants;
using MediatR;

namespace ECommerce.Application.Features.Orders;

public record GetOrdersQuery(OrderListParameters Parameters) : IRequest<PagedResult<OrderDto>>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetOrdersQueryHandler(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var parameters = request.Parameters;
        Guid? scopeUserId = null;

        if (_currentUser.IsInRole(Roles.Admin))
        {
            scopeUserId = parameters.UserId;
        }
        else
        {
            if (!_currentUser.UserId.HasValue)
                throw new ForbiddenAccessException("Authentication required.");
            if (parameters.UserId.HasValue && parameters.UserId != _currentUser.UserId)
                throw new ForbiddenAccessException("You may only view your own orders.");
            scopeUserId = _currentUser.UserId;
        }

        var page = await _uow.Orders.GetPagedAsync(parameters, scopeUserId, cancellationToken);
        return new PagedResult<OrderDto>
        {
            Items = _mapper.Map<IReadOnlyList<OrderDto>>(page.Items),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
    }
}
