namespace CorporateKnowledgeBase.Application.Features.Users;

using MediatR;

public record RejectUserCommand(string UserId) : IRequest<bool>;
