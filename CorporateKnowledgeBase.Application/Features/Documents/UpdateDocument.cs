namespace CorporateKnowledgeBase.Application.Features.Documents;

using CorporateKnowledgeBase.Application.Common.Helpers;
using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateDocumentCommand(
    int Id,
    string Title,
    string Content,
    int CategoryId,
    string ModifiedBy,
    string? TagsInput = null,
    int? DepartmentId = null) : IRequest<bool>;

public class UpdateDocumentHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateDocumentCommand, bool>
{
    public async Task<bool> Handle(
        UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await context.Documents
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (document is null) return false;

        // Save current state as version
        context.DocumentVersions.Add(new DocumentVersion
        {
            DocumentId = document.Id,
            Title = document.Title,
            Content = document.Content,
            ModifiedBy = request.ModifiedBy,
            CreatedAt = DateTime.UtcNow
        });

        // Update document
        document.Title = request.Title;
        document.Content = request.Content;
        document.CategoryId = request.CategoryId;
        document.DepartmentId = request.DepartmentId;
        document.UpdatedAt = DateTime.UtcNow;

        // Update tags
        var tagNames = TagParser.Parse(request.TagsInput);
        document.Tags.Clear();
        foreach (var name in tagNames)
        {
            var existing = await context.Tags
                .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
            document.Tags.Add(existing ?? new Tag { Name = name });
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
