namespace CorporateKnowledgeBase.Infrastructure.Services;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using CorporateKnowledgeBase.Domain.Enums;
using CorporateKnowledgeBase.Infrastructure.Hubs;
using CorporateKnowledgeBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class NotificationService(
    IHubContext<NotificationHub> hubContext,
    IApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
    : INotificationService
{
    public async Task SendToAllAsync(string message)
    {
        await hubContext.Clients.All.SendAsync("ReceiveNotification", message);
    }

    public async Task SendToUserAsync(string userId, string message)
    {
        await hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", message);
    }

    public async Task SendAndSaveToAllAsync(string message, ContentType contentType, int? referenceId = null, string? excludeUserId = null)
    {
        // Send real-time via SignalR
        await hubContext.Clients.All.SendAsync("ReceiveNotification", message);

        // Save to DB for all users
        var users = await userManager.Users.ToListAsync();
        foreach (var user in users)
        {
            if (user.Id == excludeUserId) continue;

            context.Notifications.Add(new Notification
            {
                UserId = user.Id,
                Message = message,
                ContentType = contentType,
                ReferenceId = referenceId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync(CancellationToken.None);
    }

    public async Task SendAndSaveToUserAsync(string userId, string message, ContentType contentType, int? referenceId = null)
    {
        // Send real-time via SignalR
        await hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", message);

        // Save to DB
        context.Notifications.Add(new Notification
        {
            UserId = userId,
            Message = message,
            ContentType = contentType,
            ReferenceId = referenceId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);
    }
}
