namespace CorporateKnowledgeBase.Application.Features.Tags;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteTagCommand(int Id) : IRequest<bool>;

public class DeleteTagHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteTagCommand, bool>
{
    public async Task<bool> Handle(
        DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await context.Tags
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tag is null) return false;

        context.Tags.Remove(tag);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
