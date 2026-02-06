namespace CorporateKnowledgeBase.Application.Features.Documents;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteDocumentCommand(int Id) : IRequest<bool>;

public class DeleteDocumentHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteDocumentCommand, bool>
{
    public async Task<bool> Handle(
        DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await context.Documents
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (document is null) return false;

        context.Documents.Remove(document);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
