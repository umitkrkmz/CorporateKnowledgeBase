namespace CorporateKnowledgeBase.Application.Features.Announcements;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteAnnouncementCommand(int Id) : IRequest<bool>;

public class DeleteAnnouncementHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteAnnouncementCommand, bool>
{
    public async Task<bool> Handle(
        DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await context.Announcements
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (announcement is null) return false;

        context.Announcements.Remove(announcement);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
