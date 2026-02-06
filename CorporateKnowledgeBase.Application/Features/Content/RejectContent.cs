namespace CorporateKnowledgeBase.Application.Features.Content;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record RejectContentCommand(int ContentId, ContentType ContentType, string Reason) : IRequest<bool>;

public class RejectContentHandler(IApplicationDbContext context)
    : IRequestHandler<RejectContentCommand, bool>
{
    public async Task<bool> Handle(RejectContentCommand request, CancellationToken cancellationToken)
    {
        switch (request.ContentType)
        {
            case ContentType.Document:
                var doc = await context.Documents
                    .FirstOrDefaultAsync(d => d.Id == request.ContentId, cancellationToken);
                if (doc is null) return false;
                doc.Status = ContentStatus.Rejected;
                doc.RejectionReason = request.Reason;
                break;

            case ContentType.BlogPost:
                var post = await context.BlogPosts
                    .FirstOrDefaultAsync(b => b.Id == request.ContentId, cancellationToken);
                if (post is null) return false;
                post.Status = ContentStatus.Rejected;
                post.RejectionReason = request.Reason;
                break;

            default:
                return false;
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
