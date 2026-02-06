namespace CorporateKnowledgeBase.Application.Features.BlogPosts;

using CorporateKnowledgeBase.Application.Common.Helpers;
using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateBlogPostCommand(
    int Id,
    string Title,
    string Content,
    int CategoryId,
    string ModifiedBy,
    string? TagsInput = null,
    int? DepartmentId = null) : IRequest<bool>;

public class UpdateBlogPostHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateBlogPostCommand, bool>
{
    public async Task<bool> Handle(
        UpdateBlogPostCommand request, CancellationToken cancellationToken)
    {
        var blogPost = await context.BlogPosts
            .Include(b => b.Tags)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (blogPost is null) return false;

        blogPost.Title = request.Title;
        blogPost.Content = request.Content;
        blogPost.CategoryId = request.CategoryId;
        blogPost.DepartmentId = request.DepartmentId;
        blogPost.UpdatedAt = DateTime.UtcNow;

        // Update tags
        var tagNames = TagParser.Parse(request.TagsInput);
        blogPost.Tags.Clear();
        foreach (var name in tagNames)
        {
            var existing = await context.Tags
                .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
            blogPost.Tags.Add(existing ?? new Tag { Name = name });
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
