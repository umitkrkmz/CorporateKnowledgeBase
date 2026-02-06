namespace CorporateKnowledgeBase.Application.Features.BlogPosts;

using CorporateKnowledgeBase.Application.Common.Helpers;
using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateBlogPostCommand(
    string Title,
    string Content,
    string AuthorId,
    int CategoryId,
    string? TagsInput = null,
    int? DepartmentId = null) : IRequest<int>;

public class CreateBlogPostHandler(
    IApplicationDbContext context,
    INotificationService notification,
    IAIAssistantService aiService)
    : IRequestHandler<CreateBlogPostCommand, int>
{
    public async Task<int> Handle(
        CreateBlogPostCommand request, CancellationToken cancellationToken)
    {
        // Parse and resolve tags
        var tagNames = TagParser.Parse(request.TagsInput);
        var tags = new List<Tag>();
        foreach (var name in tagNames)
        {
            var existing = await context.Tags
                .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
            tags.Add(existing ?? new Tag { Name = name });
        }

        var blogPost = new BlogPost
        {
            Title = request.Title,
            Content = request.Content,
            AuthorId = request.AuthorId,
            CategoryId = request.CategoryId,
            DepartmentId = request.DepartmentId,
            CreatedAt = DateTime.UtcNow,
            Tags = tags
        };

        context.BlogPosts.Add(blogPost);
        await context.SaveChangesAsync(cancellationToken);

        await notification.SendAndSaveToAllAsync(
            $"New blog post: {request.Title}",
            ContentType.BlogPost,
            blogPost.Id,
            request.AuthorId);

        _ = Task.Run(() => aiService.UpdateBlogPostEmbeddingAsync(blogPost.Id));

        return blogPost.Id;
    }
}
