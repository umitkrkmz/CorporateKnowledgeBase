namespace CorporateKnowledgeBase.Application.Features.Documents;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Common.Models;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetDocumentsPagedQuery : IRequest<PagedResult<DocumentDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int? CategoryId { get; init; }
    public string? Tag { get; init; }
    public string? UserId { get; init; }
    public bool IncludeAll { get; init; }
    public int? UserDepartmentId { get; init; }
}

public class GetDocumentsPagedHandler(IApplicationDbContext context)
    : IRequestHandler<GetDocumentsPagedQuery, PagedResult<DocumentDto>>
{
    public async Task<PagedResult<DocumentDto>> Handle(
        GetDocumentsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = context.Documents
            .Include(d => d.Category)
            .Include(d => d.Tags)
            .AsQueryable();

        // Filter by tag
        if (!string.IsNullOrEmpty(request.Tag))
            query = query.Where(d => d.Tags.Any(t => t.Name == request.Tag));

        // Filter by category
        if (request.CategoryId.HasValue)
            query = query.Where(d => d.CategoryId == request.CategoryId.Value);

        // Filter by status: show Published to everyone, show own drafts to author
        if (!request.IncludeAll)
        {
            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(d =>
                    d.Status == ContentStatus.Published ||
                    d.AuthorId == request.UserId);
            }
            else
            {
                query = query.Where(d => d.Status == ContentStatus.Published);
            }
        }

        // Department filter
        if (!request.IncludeAll && request.UserDepartmentId.HasValue)
        {
            query = query.Where(d =>
                d.DepartmentId == null ||
                d.DepartmentId == request.UserDepartmentId);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.Title.ToLower().Contains(searchTerm) ||
                d.Content.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "title" => request.SortDescending
                ? query.OrderByDescending(d => d.Title)
                : query.OrderBy(d => d.Title),
            "views" => request.SortDescending
                ? query.OrderByDescending(d => d.ViewCount)
                : query.OrderBy(d => d.ViewCount),
            "category" => request.SortDescending
                ? query.OrderByDescending(d => d.Category.Name)
                : query.OrderBy(d => d.Category.Name),
            _ => request.SortDescending
                ? query.OrderBy(d => d.CreatedAt)
                : query.OrderByDescending(d => d.CreatedAt) // Default: newest first
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DocumentDto(
                d.Id,
                d.Title,
                d.AuthorId,
                d.Category.Name,
                d.ViewCount,
                d.CreatedAt,
                d.Status,
                d.Tags.Select(t => t.Name).ToList()))
            .ToListAsync(cancellationToken);

        return PagedResult<DocumentDto>.Create(items, request.Page, request.PageSize, totalCount);
    }
}
