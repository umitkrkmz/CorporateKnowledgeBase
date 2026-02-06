namespace CorporateKnowledgeBase.Application.Features.Notifications;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record MarkNotificationsReadCommand(string UserId) : IRequest<bool>;

public record MarkNotificationReadCommand(int NotificationId, string UserId) : IRequest<bool>;

public class MarkNotificationsReadHandler(IApplicationDbContext context)
    : IRequestHandler<MarkNotificationsReadCommand, bool>
{
    public async Task<bool> Handle(
        MarkNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await context.Notifications
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var n in unread)
            n.IsRead = true;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class MarkNotificationReadHandler(IApplicationDbContext context)
    : IRequestHandler<MarkNotificationReadCommand, bool>
{
    public async Task<bool> Handle(
        MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, cancellationToken);

        if (notification is null) return false;

        notification.IsRead = true;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
