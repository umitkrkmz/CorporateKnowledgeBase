namespace CorporateKnowledgeBase.Application.Features.Users;

using CorporateKnowledgeBase.Application.Common.Models;
using MediatR;

public record GetUsersPagedQuery : IRequest<PagedResult<UserDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? Status { get; init; } // "pending", "approved", or null for all
    public string? Role { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

// Note: Handler is implemented in Infrastructure layer as it requires Identity access
