namespace CorporateKnowledgeBase.Application.Features.Tags;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllTagsQuery : IRequest<List<TagDto>>;

public record TagDto(int Id, string Name);

public class GetAllTagsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllTagsQuery, List<TagDto>>
{
    public async Task<List<TagDto>> Handle(
        GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        return await context.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name))
            .ToListAsync(cancellationToken);
    }
}
