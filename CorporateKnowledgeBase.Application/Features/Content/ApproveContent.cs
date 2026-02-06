namespace CorporateKnowledgeBase.Application.Features.Content;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record ApproveContentCommand(int ContentId, ContentType ContentType) : IRequest<bool>;

public class ApproveContentHandler(IApplicationDbContext context)
    : IRequestHandler<ApproveContentCommand, bool>
{
    public async Task<bool> Handle(ApproveContentCommand request, CancellationToken cancellationToken)
    {
        switch (request.ContentType)
        {
            case ContentType.Document:
                var doc = await context.Documents
                    .FirstOrDefaultAsync(d => d.Id == request.ContentId, cancellationToken);
                if (doc is null) return false;
                doc.Status = ContentStatus.Published;
                doc.RejectionReason = null;
                break;

            case ContentType.BlogPost:
                var post = await context.BlogPosts
                    .FirstOrDefaultAsync(b => b.Id == request.ContentId, cancellationToken);
                if (post is null) return false;
                post.Status = ContentStatus.Published;
                post.RejectionReason = null;
                break;

            default:
                return false;
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
