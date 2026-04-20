using ECommerce.Application.Common.Caching;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Categories;

public record DeleteCategoryCommand(Guid Id) : IRequest;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IProductCatalogCacheVersion _catalogVersion;

    public DeleteCategoryCommandHandler(IUnitOfWork uow, IProductCatalogCacheVersion catalogVersion)
    {
        _uow = uow;
        _catalogVersion = catalogVersion;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _uow.Categories.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.Category), request.Id);

        _uow.Categories.Remove(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        await _catalogVersion.BumpAsync(cancellationToken);
    }
}
