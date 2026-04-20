using AutoMapper;
using ECommerce.Application.Common.Caching;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Products;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private static readonly TimeSpan DetailCacheDuration = TimeSpan.FromMinutes(2);

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetProductByIdQueryHandler(IUnitOfWork uow, IMapper mapper, ICacheService cache)
    {
        _uow = uow;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.ProductDetail(request.Id);
        var dto = await _cache.GetAsync<ProductDto>(cacheKey, cancellationToken);
        if (dto != null)
            return dto;

        var entity = await _uow.Products.GetByIdWithCategoriesAsync(request.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.Product), request.Id);

        dto = _mapper.Map<ProductDto>(entity);
        await _cache.SetAsync(cacheKey, dto, DetailCacheDuration, cancellationToken);
        return dto;
    }
}
