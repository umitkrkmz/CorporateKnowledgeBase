namespace CorporateKnowledgeBase.Application.Features.Documents;

using CorporateKnowledgeBase.Application.Common.Helpers;
using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateDocumentCommand(
    string Title,
    string Content,
    string AuthorId,
    int CategoryId,
    string? TagsInput = null,
    int? DepartmentId = null) : IRequest<int>;

public class CreateDocumentHandler(
    IApplicationDbContext context,
    INotificationService notification,
    IAIAssistantService aiService)
    : IRequestHandler<CreateDocumentCommand, int>
{
    public async Task<int> Handle(
        CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        // Parse and resolve tags
        var tagNames = TagParser.Parse(request.TagsInput);
        var tags = new List<Tag>();
        foreach (var name in tagNames)
        {
            var existing = await context.Tags
                .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
            tags.Add(existing ?? new Tag { Name = name });
        }

        var document = new TechnicalDocument
        {
            Title = request.Title,
            Content = request.Content,
            AuthorId = request.AuthorId,
            CategoryId = request.CategoryId,
            DepartmentId = request.DepartmentId,
            Tags = tags
        };

        context.Documents.Add(document);
        await context.SaveChangesAsync(cancellationToken);

        await notification.SendAndSaveToAllAsync(
            $"New document added: {request.Title}",
            ContentType.Document,
            document.Id,
            request.AuthorId);

        _ = Task.Run(() => aiService.UpdateDocumentEmbeddingAsync(document.Id));

        return document.Id;
    }
}
