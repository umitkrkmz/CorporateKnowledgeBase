namespace CorporateKnowledgeBase.Application.Features.Announcements;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;

public record CreateAnnouncementCommand(
    string Title,
    string Content,
    string AuthorId) : IRequest<int>;

public class CreateAnnouncementHandler(
    IApplicationDbContext context,
    INotificationService notification)
    : IRequestHandler<CreateAnnouncementCommand, int>
{
    public async Task<int> Handle(
        CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = new Announcement
        {
            Title = request.Title,
            Content = request.Content,
            AuthorId = request.AuthorId,
            CreatedAt = DateTime.UtcNow
        };

        context.Announcements.Add(announcement);
        await context.SaveChangesAsync(cancellationToken);

        await notification.SendAndSaveToAllAsync(
            $"New announcement: {request.Title}",
            ContentType.Announcement,
            announcement.Id,
            request.AuthorId);

        return announcement.Id;
    }
}
