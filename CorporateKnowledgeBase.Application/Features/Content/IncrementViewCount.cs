namespace CorporateKnowledgeBase.Application.Features.Content;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record IncrementViewCountCommand(int ContentId, ContentType ContentType) : IRequest;

public class IncrementViewCountHandler(IApplicationDbContext context)
    : IRequestHandler<IncrementViewCountCommand>
{
    public async Task Handle(IncrementViewCountCommand request, CancellationToken cancellationToken)
    {
        if (request.ContentType == ContentType.Document)
        {
            var document = await context.Documents
                .FirstOrDefaultAsync(d => d.Id == request.ContentId, cancellationToken);

            if (document != null)
            {
                document.ViewCount++;
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        else if (request.ContentType == ContentType.BlogPost)
        {
            var blogPost = await context.BlogPosts
                .FirstOrDefaultAsync(b => b.Id == request.ContentId, cancellationToken);

            if (blogPost != null)
            {
                blogPost.ViewCount++;
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
