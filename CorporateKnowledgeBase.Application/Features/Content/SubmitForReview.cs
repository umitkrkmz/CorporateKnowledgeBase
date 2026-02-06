namespace CorporateKnowledgeBase.Application.Features.Content;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record SubmitForReviewCommand(int ContentId, ContentType ContentType, string UserId) : IRequest<bool>;

public class SubmitForReviewHandler(IApplicationDbContext context)
    : IRequestHandler<SubmitForReviewCommand, bool>
{
    public async Task<bool> Handle(SubmitForReviewCommand request, CancellationToken cancellationToken)
    {
        switch (request.ContentType)
        {
            case ContentType.Document:
                var doc = await context.Documents
                    .FirstOrDefaultAsync(d => d.Id == request.ContentId && d.AuthorId == request.UserId, cancellationToken);
                if (doc is null || doc.Status != ContentStatus.Draft) return false;
                doc.Status = ContentStatus.PendingReview;
                break;

            case ContentType.BlogPost:
                var post = await context.BlogPosts
                    .FirstOrDefaultAsync(b => b.Id == request.ContentId && b.AuthorId == request.UserId, cancellationToken);
                if (post is null || post.Status != ContentStatus.Draft) return false;
                post.Status = ContentStatus.PendingReview;
                break;

            default:
                return false;
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
