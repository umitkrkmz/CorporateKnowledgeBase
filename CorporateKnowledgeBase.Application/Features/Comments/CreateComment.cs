namespace CorporateKnowledgeBase.Application.Features.Comments;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;

public record CreateCommentCommand(
    string Content,
    string AuthorId,
    int? DocumentId,
    int? BlogPostId,
    ContentType ContentType,
    int? ParentCommentId = null) : IRequest<int>;

public class CreateCommentHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCommentCommand, int>
{
    public async Task<int> Handle(
        CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = new Comment
        {
            Content = request.Content,
            AuthorId = request.AuthorId,
            DocumentId = request.DocumentId,
            BlogPostId = request.BlogPostId,
            ContentType = request.ContentType,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
