namespace CorporateKnowledgeBase.Application.Features.Comments;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetCommentsQuery(int ContentId, ContentType ContentType) : IRequest<List<CommentDto>>;

public record CommentDto(
    int Id,
    string Content,
    string AuthorId,
    string AuthorName,
    string? AuthorImagePath,
    int? ParentCommentId,
    DateTime CreatedAt,
    List<CommentDto> Replies);

public class GetCommentsHandler(
    IApplicationDbContext context,
    IUserNameResolver userNameResolver)
    : IRequestHandler<GetCommentsQuery, List<CommentDto>>
{
    public async Task<List<CommentDto>> Handle(
        GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Comments.AsQueryable();

        query = request.ContentType == ContentType.Document
            ? query.Where(c => c.DocumentId == request.ContentId)
            : query.Where(c => c.BlogPostId == request.ContentId);

        var allComments = await query
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var dtoMap = new Dictionary<int, CommentDto>();
        var rootComments = new List<CommentDto>();

        foreach (var c in allComments)
        {
            var name = await userNameResolver.GetFullNameAsync(c.AuthorId);
            var image = await userNameResolver.GetProfileImagePathAsync(c.AuthorId);
            var dto = new CommentDto(c.Id, c.Content, c.AuthorId, name, image, c.ParentCommentId, c.CreatedAt, new List<CommentDto>());
            dtoMap[c.Id] = dto;

            if (c.ParentCommentId.HasValue && dtoMap.TryGetValue(c.ParentCommentId.Value, out var parent))
            {
                parent.Replies.Add(dto);
            }
            else
            {
                rootComments.Add(dto);
            }
        }

        return rootComments;
    }
}
