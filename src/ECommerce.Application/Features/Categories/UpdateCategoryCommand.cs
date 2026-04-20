using ECommerce.Application.Common.Caching;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Categories;

public record UpdateCategoryCommand(Guid Id, string Name, string? Description) : IRequest;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IProductCatalogCacheVersion _catalogVersion;

    public UpdateCategoryCommandHandler(IUnitOfWork uow, IProductCatalogCacheVersion catalogVersion)
    {
        _uow = uow;
        _catalogVersion = catalogVersion;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _uow.Categories.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.Category), request.Id);

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;
        _uow.Categories.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        await _catalogVersion.BumpAsync(cancellationToken);
    }
}
