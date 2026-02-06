namespace CorporateKnowledgeBase.Application.Features.BlogPosts;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteBlogPostCommand(int Id) : IRequest<bool>;

public class DeleteBlogPostHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteBlogPostCommand, bool>
{
    public async Task<bool> Handle(
        DeleteBlogPostCommand request, CancellationToken cancellationToken)
    {
        var blogPost = await context.BlogPosts
            .Include(b => b.Tags)
            .Include(b => b.Comments)
            .Include(b => b.Likes)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (blogPost is null) return false;

        context.BlogPosts.Remove(blogPost);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
