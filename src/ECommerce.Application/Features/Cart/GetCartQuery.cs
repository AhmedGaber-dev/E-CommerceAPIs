using AutoMapper;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Cart;

public record GetCartQuery : IRequest<CartDto>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetCartQueryHandler(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            throw new ForbiddenAccessException("Authentication required.");

        var cart = await _uow.Carts.GetByUserIdWithItemsAsync(_currentUser.UserId.Value, cancellationToken);
        if (cart == null)
        {
            cart = new Domain.Entities.Cart
            {
                Id = Guid.NewGuid(),
                UserId = _currentUser.UserId.Value,
                CreatedAtUtc = DateTime.UtcNow
            };
            await _uow.Carts.AddAsync(cart, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        var dto = _mapper.Map<CartDto>(cart);
        dto.Subtotal = dto.Items.Sum(i => i.UnitPrice * i.Quantity);
        return dto;
    }
}
