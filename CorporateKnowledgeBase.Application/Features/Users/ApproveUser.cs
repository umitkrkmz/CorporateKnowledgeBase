namespace CorporateKnowledgeBase.Application.Features.Users;

using MediatR;

public record ApproveUserCommand(string UserId) : IRequest<bool>;
