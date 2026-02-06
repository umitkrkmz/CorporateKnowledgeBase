namespace CorporateKnowledgeBase.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    Task CleanOldAuditLogsAsync(int daysToKeep = 90);
    Task CleanOldNotificationsAsync(int daysToKeep = 30);
}
