namespace CorporateKnowledgeBase.Application.Features.Likes;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record ToggleLikeCommand(
    string UserId,
    int ContentId,
    ContentType ContentType) : IRequest<LikeResultDto>;

public record LikeResultDto(int Count, bool IsLiked);

public class ToggleLikeHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleLikeCommand, LikeResultDto>
{
    public async Task<LikeResultDto> Handle(
        ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        var existing = request.ContentType == ContentType.Document
            ? await context.UserLikes.FirstOrDefaultAsync(
                l => l.UserId == request.UserId && l.DocumentId == request.ContentId, cancellationToken)
            : await context.UserLikes.FirstOrDefaultAsync(
                l => l.UserId == request.UserId && l.BlogPostId == request.ContentId, cancellationToken);

        if (existing is not null)
        {
            context.UserLikes.Remove(existing);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var like = new UserLike
            {
                UserId = request.UserId,
                ContentType = request.ContentType,
                CreatedAt = DateTime.UtcNow
            };

            if (request.ContentType == ContentType.Document)
                like.DocumentId = request.ContentId;
            else
                like.BlogPostId = request.ContentId;

            context.UserLikes.Add(like);
            await context.SaveChangesAsync(cancellationToken);
        }

        var count = request.ContentType == ContentType.Document
            ? await context.UserLikes.CountAsync(l => l.DocumentId == request.ContentId, cancellationToken)
            : await context.UserLikes.CountAsync(l => l.BlogPostId == request.ContentId, cancellationToken);

        // toggle logic: existed -> removed (not liked), didn't exist -> added (liked)
        var isLiked = existing is null;

        return new LikeResultDto(count, isLiked);
    }
}
