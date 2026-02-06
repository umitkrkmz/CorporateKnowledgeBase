namespace CorporateKnowledgeBase.Application.Features.AuditLogs;

using CorporateKnowledgeBase.Application.Common.Extensions;
using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAuditLogsQuery : IRequest<PagedResult<AuditLogDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? Action { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

public record AuditLogDto(
    int Id,
    string UserName,
    string Action,
    string EntityName,
    int? EntityId,
    DateTime CreatedAt);

public class GetAuditLogsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    public async Task<PagedResult<AuditLogDto>> Handle(
        GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = context.AuditLogs.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(a =>
                a.UserName.ToLower().Contains(searchTerm) ||
                a.EntityName.ToLower().Contains(searchTerm) ||
                a.Action.ToLower().Contains(searchTerm));
        }

        // Apply action filter
        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(a => a.Action == request.Action);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "username" => request.SortDescending
                ? query.OrderByDescending(a => a.UserName)
                : query.OrderBy(a => a.UserName),
            "action" => request.SortDescending
                ? query.OrderByDescending(a => a.Action)
                : query.OrderBy(a => a.Action),
            "entityname" => request.SortDescending
                ? query.OrderByDescending(a => a.EntityName)
                : query.OrderBy(a => a.EntityName),
            _ => request.SortDescending
                ? query.OrderByDescending(a => a.CreatedAt)
                : query.OrderBy(a => a.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto(
                a.Id, a.UserName, a.Action,
                a.EntityName, a.EntityId, a.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<AuditLogDto>.Create(items, request.Page, request.PageSize, totalCount);
    }
}
