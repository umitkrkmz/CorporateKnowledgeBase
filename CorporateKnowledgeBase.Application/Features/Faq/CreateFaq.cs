namespace CorporateKnowledgeBase.Application.Features.Faq;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using MediatR;

public record CreateFaqCommand(string Question, string Answer, string? Category, int SortOrder, bool IsPublished) : IRequest<int>;

public class CreateFaqHandler(IApplicationDbContext context)
    : IRequestHandler<CreateFaqCommand, int>
{
    public async Task<int> Handle(CreateFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = new FaqItem
        {
            Question = request.Question,
            Answer = request.Answer,
            Category = request.Category,
            SortOrder = request.SortOrder,
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow
        };

        context.FaqItems.Add(faq);
        await context.SaveChangesAsync(cancellationToken);
        return faq.Id;
    }
}
