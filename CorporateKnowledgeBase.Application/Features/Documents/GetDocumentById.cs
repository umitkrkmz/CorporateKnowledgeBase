namespace CorporateKnowledgeBase.Application.Features.Documents;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetDocumentByIdQuery(int Id) : IRequest<DocumentDetailDto?>;

public record DocumentDetailDto(
    int Id,
    string Title,
    string Content,
    string AuthorId,
    int CategoryId,
    string CategoryName,
    int ViewCount,
    DateTime CreatedAt,
    ContentStatus Status,
    string? RejectionReason,
    List<string> Tags);

public class GetDocumentByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetDocumentByIdQuery, DocumentDetailDto?>
{
    public async Task<DocumentDetailDto?> Handle(
        GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var doc = await context.Documents
            .Include(d => d.Category)
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (doc is null) return null;

        return new DocumentDetailDto(
            doc.Id, doc.Title, doc.Content, doc.AuthorId,
            doc.CategoryId, doc.Category.Name, doc.ViewCount, doc.CreatedAt,
            doc.Status, doc.RejectionReason,
            doc.Tags.Select(t => t.Name).ToList());
    }
}
