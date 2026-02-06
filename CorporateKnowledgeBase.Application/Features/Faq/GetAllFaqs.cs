namespace CorporateKnowledgeBase.Application.Features.Faq;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllFaqsQuery(bool IncludeUnpublished = false) : IRequest<List<FaqDto>>;

public record FaqDto(int Id, string Question, string Answer, string? Category, int SortOrder, bool IsPublished);

public class GetAllFaqsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllFaqsQuery, List<FaqDto>>
{
    public async Task<List<FaqDto>> Handle(GetAllFaqsQuery request, CancellationToken cancellationToken)
    {
        var query = context.FaqItems.AsQueryable();

        if (!request.IncludeUnpublished)
            query = query.Where(f => f.IsPublished);

        return await query
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Category)
            .Select(f => new FaqDto(f.Id, f.Question, f.Answer, f.Category, f.SortOrder, f.IsPublished))
            .ToListAsync(cancellationToken);
    }
}
