namespace CorporateKnowledgeBase.Application.Features.Faq;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateFaqCommand(int Id, string Question, string Answer, string? Category, int SortOrder, bool IsPublished) : IRequest<bool>;

public class UpdateFaqHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateFaqCommand, bool>
{
    public async Task<bool> Handle(UpdateFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = await context.FaqItems
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (faq is null) return false;

        faq.Question = request.Question;
        faq.Answer = request.Answer;
        faq.Category = request.Category;
        faq.SortOrder = request.SortOrder;
        faq.IsPublished = request.IsPublished;
        faq.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
