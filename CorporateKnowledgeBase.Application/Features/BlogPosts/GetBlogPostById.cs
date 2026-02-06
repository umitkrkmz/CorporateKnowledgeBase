namespace CorporateKnowledgeBase.Application.Features.BlogPosts;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetBlogPostByIdQuery(int Id) : IRequest<BlogPostDetailDto?>;

public record BlogPostDetailDto(
    int Id,
    string Title,
    string Content,
    string AuthorId,
    int CategoryId,
    string CategoryName,
    int ViewCount,
    DateTime CreatedAt,
    ContentStatus Status,
    string? RejectionReason,
    List<string> Tags);

public class GetBlogPostByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetBlogPostByIdQuery, BlogPostDetailDto?>
{
    public async Task<BlogPostDetailDto?> Handle(
        GetBlogPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await context.BlogPosts
            .Include(b => b.Category)
            .Include(b => b.Tags)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (post is null) return null;

        return new BlogPostDetailDto(
            post.Id, post.Title, post.Content, post.AuthorId,
            post.CategoryId, post.Category.Name, post.ViewCount, post.CreatedAt,
            post.Status, post.RejectionReason,
            post.Tags.Select(t => t.Name).ToList());
    }
}
