namespace CorporateKnowledgeBase.Application.Features.BlogPosts;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllBlogPostsQuery(string? Tag = null, string? UserId = null, bool IncludeAll = false, int? UserDepartmentId = null) : IRequest<List<BlogPostDto>>;

public record BlogPostDto(
    int Id,
    string Title,
    string AuthorId,
    string CategoryName,
    int ViewCount,
    DateTime CreatedAt,
    ContentStatus Status,
    List<string> Tags);

public class GetAllBlogPostsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllBlogPostsQuery, List<BlogPostDto>>
{
    public async Task<List<BlogPostDto>> Handle(
        GetAllBlogPostsQuery request, CancellationToken cancellationToken)
    {
        var query = context.BlogPosts
            .Include(b => b.Category)
            .Include(b => b.Tags)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Tag))
            query = query.Where(b => b.Tags.Any(t => t.Name == request.Tag));

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

        return await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BlogPostDto(
                b.Id, b.Title, b.AuthorId,
                b.Category.Name, b.ViewCount, b.CreatedAt,
                b.Status,
                b.Tags.Select(t => t.Name).ToList()))
            .ToListAsync(cancellationToken);
    }
}
