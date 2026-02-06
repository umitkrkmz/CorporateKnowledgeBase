namespace CorporateKnowledgeBase.Application.Features.Tags;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateTagCommand(int Id, string Name) : IRequest<bool>;

public class UpdateTagHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateTagCommand, bool>
{
    public async Task<bool> Handle(
        UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await context.Tags
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tag is null) return false;

        tag.Name = request.Name;
        tag.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
