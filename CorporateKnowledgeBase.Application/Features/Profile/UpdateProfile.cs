namespace CorporateKnowledgeBase.Application.Features.Profile;

using MediatR;

public record UpdateProfileCommand(
    string UserId,
    string FullName,
    int? DepartmentId,
    string? JobTitle) : IRequest<bool>;

public record UpdateNotificationSettingsCommand(
    string UserId,
    bool NotifyOnNewComment,
    bool NotifyOnNewBlogPost,
    bool NotifyOnNewDocument,
    bool NotifyOnNewAnnouncement) : IRequest<bool>;
