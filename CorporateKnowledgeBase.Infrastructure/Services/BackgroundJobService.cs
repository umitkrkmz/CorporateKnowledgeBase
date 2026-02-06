namespace CorporateKnowledgeBase.Infrastructure.Services;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public class BackgroundJobService(IApplicationDbContext context)
    : IBackgroundJobService
{
    public async Task CleanOldAuditLogsAsync(int daysToKeep = 90)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysToKeep);
        var oldLogs = await context.AuditLogs
            .Where(a => a.CreatedAt < cutoff)
            .ToListAsync();

        if (oldLogs.Any())
        {
            context.AuditLogs.RemoveRange(oldLogs);
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }

    public async Task CleanOldNotificationsAsync(int daysToKeep = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysToKeep);
        var oldNotifications = await context.Notifications
            .Where(n => n.CreatedAt < cutoff)
            .ToListAsync();

        if (oldNotifications.Any())
        {
            context.Notifications.RemoveRange(oldNotifications);
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
