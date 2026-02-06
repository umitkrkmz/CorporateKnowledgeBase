namespace CorporateKnowledgeBase.Application.Features.Documents;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetDocumentVersionsQuery(int DocumentId) : IRequest<List<DocumentVersionDto>>;

public record DocumentVersionDto(
    int Id,
    string Title,
    string Content,
    string ModifiedBy,
    DateTime CreatedAt);

public class GetDocumentVersionsHandler(
    IApplicationDbContext context,
    IUserNameResolver userNameResolver)
    : IRequestHandler<GetDocumentVersionsQuery, List<DocumentVersionDto>>
{
    public async Task<List<DocumentVersionDto>> Handle(
        GetDocumentVersionsQuery request, CancellationToken cancellationToken)
    {
        var versions = await context.DocumentVersions
            .Where(v => v.DocumentId == request.DocumentId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<DocumentVersionDto>();
        foreach (var v in versions)
        {
            var fullName = await userNameResolver.GetFullNameAsync(v.ModifiedBy);
            result.Add(new DocumentVersionDto(
                v.Id, v.Title, v.Content, fullName, v.CreatedAt));
        }

        return result;
    }
}
