namespace CorporateKnowledgeBase.Application.Features.Auth;

using MediatR;

public record LoginCommand(
    string Email,
    string Password,
    bool RememberMe) : IRequest<LoginResult>;

public record LoginResult(bool Succeeded, string? Error, bool IsPendingApproval = false);
