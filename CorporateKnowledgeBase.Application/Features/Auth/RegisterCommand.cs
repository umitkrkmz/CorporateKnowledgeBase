namespace CorporateKnowledgeBase.Application.Features.Auth;

using MediatR;

public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string? Department) : IRequest<RegisterResult>;

public record RegisterResult(bool Succeeded, IEnumerable<string> Errors, bool IsPendingApproval = false);
