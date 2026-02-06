namespace CorporateKnowledgeBase.Application.Features.Faq;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteFaqCommand(int Id) : IRequest<bool>;

public class DeleteFaqHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteFaqCommand, bool>
{
    public async Task<bool> Handle(DeleteFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = await context.FaqItems
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (faq is null) return false;

        context.FaqItems.Remove(faq);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
