namespace CorporateKnowledgeBase.Application.Features.Tags;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetTagsPagedQuery : IRequest<PagedResult<TagDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

public class GetTagsPagedHandler(IApplicationDbContext context)
    : IRequestHandler<GetTagsPagedQuery, PagedResult<TagDto>>
{
    public async Task<PagedResult<TagDto>> Handle(
        GetTagsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = context.Tags.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(t => t.Name)
                : query.OrderBy(t => t.Name),
            _ => request.SortDescending
                ? query.OrderByDescending(t => t.Name)
                : query.OrderBy(t => t.Name)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TagDto(t.Id, t.Name))
            .ToListAsync(cancellationToken);

        return PagedResult<TagDto>.Create(items, request.Page, request.PageSize, totalCount);
    }
}
