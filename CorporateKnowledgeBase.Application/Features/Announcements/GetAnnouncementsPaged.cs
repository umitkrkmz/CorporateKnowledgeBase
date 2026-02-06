namespace CorporateKnowledgeBase.Application.Features.Announcements;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAnnouncementsPagedQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null) : IRequest<PagedResult<AnnouncementDto>>;

public class GetAnnouncementsPagedHandler(IApplicationDbContext context)
    : IRequestHandler<GetAnnouncementsPagedQuery, PagedResult<AnnouncementDto>>
{
    public async Task<PagedResult<AnnouncementDto>> Handle(
        GetAnnouncementsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = context.Announcements.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(a =>
                a.Title.ToLower().Contains(searchLower) ||
                a.Content.ToLower().Contains(searchLower));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and get items
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AnnouncementDto(
                a.Id, a.Title, a.Content, a.AuthorId, a.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<AnnouncementDto>.Create(
            items,
            request.Page,
            request.PageSize,
            totalCount);
    }
}
