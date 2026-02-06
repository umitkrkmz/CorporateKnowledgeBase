namespace CorporateKnowledgeBase.Application.Features.Profile;

using MediatR;

public record GetUserProfileQuery(string UserId) : IRequest<UserProfileDto?>;

public record UserProfileDto(
    string Id,
    string FullName,
    string Email,
    string? Department,
    int? DepartmentId,
    string? JobTitle,
    string? ProfileImagePath,
    List<string> Roles,
    DateTime CreatedAt,
    DateTime? LastLoginDate,
    int DocumentCount,
    int BlogPostCount,
    bool NotifyOnNewComment,
    bool NotifyOnNewBlogPost,
    bool NotifyOnNewDocument,
    bool NotifyOnNewAnnouncement);
