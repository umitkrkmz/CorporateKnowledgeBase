namespace CorporateKnowledgeBase.Application.Features.Reports;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Common.Models;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetReportsPagedQuery : IRequest<PagedResult<ReportDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public ReportStatus? Status { get; init; }
    public ContentType? ContentType { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

public class GetReportsPagedHandler(
    IApplicationDbContext context,
    IUserNameResolver userNameResolver)
    : IRequestHandler<GetReportsPagedQuery, PagedResult<ReportDto>>
{
    public async Task<PagedResult<ReportDto>> Handle(
        GetReportsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = context.ContentReports.AsQueryable();

        // Apply status filter
        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        // Apply content type filter
        if (request.ContentType.HasValue)
        {
            query = query.Where(r => r.ContentType == request.ContentType.Value);
        }

        // Apply search filter (on reason)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(r => r.Reason.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "status" => request.SortDescending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            "contenttype" => request.SortDescending
                ? query.OrderByDescending(r => r.ContentType)
                : query.OrderBy(r => r.ContentType),
            _ => request.SortDescending
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var reports = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Project with user names
        var items = new List<ReportDto>();
        foreach (var r in reports)
        {
            var reporterName = await userNameResolver.GetFullNameAsync(r.ReporterId);
            items.Add(new ReportDto(
                r.Id, r.ReporterId, reporterName, r.ContentId, r.ContentType,
                r.Reason, r.Status, r.AdminNotes, r.CreatedAt));
        }

        return PagedResult<ReportDto>.Create(items, request.Page, request.PageSize, totalCount);
    }
}
