namespace CorporateKnowledgeBase.Application.Features.Announcements;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateAnnouncementCommand(
    int Id,
    string Title,
    string Content) : IRequest<bool>;

public class UpdateAnnouncementHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateAnnouncementCommand, bool>
{
    public async Task<bool> Handle(
        UpdateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await context.Announcements
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (announcement is null) return false;

        announcement.Title = request.Title;
        announcement.Content = request.Content;
        announcement.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
