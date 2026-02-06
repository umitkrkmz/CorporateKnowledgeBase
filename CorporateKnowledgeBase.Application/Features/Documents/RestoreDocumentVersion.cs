namespace CorporateKnowledgeBase.Application.Features.Documents;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record RestoreDocumentVersionCommand(
    int VersionId,
    string RestoredBy) : IRequest<int?>;

public class RestoreDocumentVersionHandler(IApplicationDbContext context)
    : IRequestHandler<RestoreDocumentVersionCommand, int?>
{
    public async Task<int?> Handle(
        RestoreDocumentVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await context.DocumentVersions
            .FirstOrDefaultAsync(v => v.Id == request.VersionId, cancellationToken);

        if (version is null) return null;

        var document = await context.Documents
            .FirstOrDefaultAsync(d => d.Id == version.DocumentId, cancellationToken);

        if (document is null) return null;

        // Auto-backup current state before restoring
        context.DocumentVersions.Add(new DocumentVersion
        {
            DocumentId = document.Id,
            Title = document.Title,
            Content = document.Content,
            ModifiedBy = request.RestoredBy,
            CreatedAt = DateTime.UtcNow
        });

        // Restore from the selected version
        document.Title = version.Title;
        document.Content = version.Content;
        document.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return document.Id;
    }
}
