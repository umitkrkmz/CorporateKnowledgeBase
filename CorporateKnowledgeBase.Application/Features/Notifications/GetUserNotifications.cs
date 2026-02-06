namespace CorporateKnowledgeBase.Application.Features.Notifications;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetUserNotificationsQuery(string UserId, int Take = 5) : IRequest<NotificationListDto>;

public record GetAllUserNotificationsQuery(string UserId) : IRequest<List<NotificationItemDto>>;

public record NotificationListDto(List<NotificationItemDto> Items, int UnreadCount);

public record NotificationItemDto(
    int Id,
    string Message,
    bool IsRead,
    ContentType ContentType,
    int? ReferenceId,
    DateTime CreatedAt);

public class GetUserNotificationsHandler(IApplicationDbContext context)
    : IRequestHandler<GetUserNotificationsQuery, NotificationListDto>
{
    public async Task<NotificationListDto> Handle(
        GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var items = await context.Notifications
            .Where(n => n.UserId == request.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(request.Take)
            .Select(n => new NotificationItemDto(
                n.Id, n.Message, n.IsRead, n.ContentType, n.ReferenceId, n.CreatedAt))
            .ToListAsync(cancellationToken);

        var unreadCount = await context.Notifications
            .CountAsync(n => n.UserId == request.UserId && !n.IsRead, cancellationToken);

        return new NotificationListDto(items, unreadCount);
    }
}

public class GetAllUserNotificationsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllUserNotificationsQuery, List<NotificationItemDto>>
{
    public async Task<List<NotificationItemDto>> Handle(
        GetAllUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        return await context.Notifications
            .Where(n => n.UserId == request.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationItemDto(
                n.Id, n.Message, n.IsRead, n.ContentType, n.ReferenceId, n.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
