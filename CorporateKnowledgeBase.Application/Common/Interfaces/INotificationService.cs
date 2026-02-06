namespace CorporateKnowledgeBase.Application.Common.Interfaces;

using CorporateKnowledgeBase.Domain.Enums;

public interface INotificationService
{
    Task SendToAllAsync(string message);
    Task SendToUserAsync(string userId, string message);
    Task SendAndSaveToAllAsync(string message, ContentType contentType, int? referenceId = null, string? excludeUserId = null);
    Task SendAndSaveToUserAsync(string userId, string message, ContentType contentType, int? referenceId = null);
}
