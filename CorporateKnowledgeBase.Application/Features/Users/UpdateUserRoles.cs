namespace CorporateKnowledgeBase.Application.Features.Users;

using MediatR;

public record UpdateUserRolesCommand(string UserId, List<string> Roles) : IRequest<bool>;
