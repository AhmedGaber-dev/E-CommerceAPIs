using ECommerce.Application.Common.Caching;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Categories;

public record CreateCategoryCommand(string Name, string? Description) : IRequest<Guid>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IProductCatalogCacheVersion _catalogVersion;

    public CreateCategoryCommandHandler(IUnitOfWork uow, IProductCatalogCacheVersion catalogVersion)
    {
        _uow = uow;
        _catalogVersion = catalogVersion;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
        await _uow.Categories.AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        await _catalogVersion.BumpAsync(cancellationToken);
        return entity.Id;
    }
}
