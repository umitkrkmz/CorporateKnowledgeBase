namespace CorporateKnowledgeBase.Application.Features.Categories;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllCategoriesQuery : IRequest<List<CategoryDto>>;

public record CategoryDto(int Id, string Name, string? Description);

public class GetAllCategoriesHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(
        GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync(cancellationToken);
    }
}
