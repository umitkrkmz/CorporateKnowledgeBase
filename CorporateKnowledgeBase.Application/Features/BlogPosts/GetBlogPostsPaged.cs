namespace CorporateKnowledgeBase.Application.Features.BlogPosts;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Common.Models;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetBlogPostsPagedQuery : IRequest<PagedResult<BlogPostDto>>
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

public class GetBlogPostsPagedHandler(IApplicationDbContext context)
    : IRequestHandler<GetBlogPostsPagedQuery, PagedResult<BlogPostDto>>
{
    public async Task<PagedResult<BlogPostDto>> Handle(
        GetBlogPostsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = context.BlogPosts
            .Include(b => b.Category)
            .Include(b => b.Tags)
            .AsQueryable();

        // Filter by tag
        if (!string.IsNullOrEmpty(request.Tag))
            query = query.Where(b => b.Tags.Any(t => t.Name == request.Tag));

        // Filter by category
        if (request.CategoryId.HasValue)
            query = query.Where(b => b.CategoryId == request.CategoryId.Value);

        // Filter by status
        if (!request.IncludeAll)
        {
            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(b =>
                    b.Status == ContentStatus.Published ||
                    b.AuthorId == request.UserId);
            }
            else
            {
                query = query.Where(b => b.Status == ContentStatus.Published);
            }
        }

        // Department filter
        if (!request.IncludeAll && request.UserDepartmentId.HasValue)
        {
            query = query.Where(b =>
                b.DepartmentId == null ||
                b.DepartmentId == request.UserDepartmentId);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(searchTerm) ||
                b.Content.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "title" => request.SortDescending
                ? query.OrderByDescending(b => b.Title)
                : query.OrderBy(b => b.Title),
            "views" => request.SortDescending
                ? query.OrderByDescending(b => b.ViewCount)
                : query.OrderBy(b => b.ViewCount),
            "category" => request.SortDescending
                ? query.OrderByDescending(b => b.Category.Name)
                : query.OrderBy(b => b.Category.Name),
            _ => request.SortDescending
                ? query.OrderBy(b => b.CreatedAt)
                : query.OrderByDescending(b => b.CreatedAt) // Default: newest first
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BlogPostDto(
                b.Id,
                b.Title,
                b.AuthorId,
                b.Category.Name,
                b.ViewCount,
                b.CreatedAt,
                b.Status,
                b.Tags.Select(t => t.Name).ToList()))
            .ToListAsync(cancellationToken);

        return PagedResult<BlogPostDto>.Create(items, request.Page, request.PageSize, totalCount);
    }
}
