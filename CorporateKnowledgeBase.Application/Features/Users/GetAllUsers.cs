namespace CorporateKnowledgeBase.Application.Features.Users;

using MediatR;

public record GetAllUsersQuery : IRequest<List<UserDto>>;

public record UserDto(
    string Id,
    string FullName,
    string Email,
    string? Department,
    List<string> Roles,
    bool IsApproved,
    DateTime CreatedAt);
