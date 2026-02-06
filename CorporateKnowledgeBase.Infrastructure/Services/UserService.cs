namespace CorporateKnowledgeBase.Infrastructure.Services;

using CorporateKnowledgeBase.Application.Common.Models;
using CorporateKnowledgeBase.Application.Features.Users;
using CorporateKnowledgeBase.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class GetAllUsersHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(
        GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);

        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new UserDto(
                user.Id, user.FullName, user.Email ?? "",
                user.Department, roles.ToList(), user.IsApproved, user.CreatedAt));
        }

        return result;
    }
}

public class GetUsersPagedHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetUsersPagedQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> Handle(
        GetUsersPagedQuery request, CancellationToken cancellationToken)
    {
        var query = userManager.Users.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(searchTerm) ||
                (u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
                (u.Department != null && u.Department.ToLower().Contains(searchTerm)));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = request.Status.ToLower() switch
            {
                "pending" => query.Where(u => !u.IsApproved),
                "approved" => query.Where(u => u.IsApproved),
                _ => query
            };
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "fullname" or "name" => request.SortDescending
                ? query.OrderByDescending(u => u.FullName)
                : query.OrderBy(u => u.FullName),
            "email" => request.SortDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "department" => request.SortDescending
                ? query.OrderByDescending(u => u.Department)
                : query.OrderBy(u => u.Department),
            _ => request.SortDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Get roles and filter by role if specified
        var items = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            // Skip if role filter doesn't match
            if (!string.IsNullOrWhiteSpace(request.Role) &&
                !roles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            items.Add(new UserDto(
                user.Id, user.FullName, user.Email ?? "",
                user.Department, roles.ToList(), user.IsApproved, user.CreatedAt));
        }

        return PagedResult<UserDto>.Create(items, request.Page, request.PageSize, totalCount);
    }
}

public class UpdateUserRolesHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<UpdateUserRolesCommand, bool>
{
    public async Task<bool> Handle(
        UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) return false;

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRolesAsync(user, request.Roles);

        return true;
    }
}

public class ApproveUserHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<ApproveUserCommand, bool>
{
    public async Task<bool> Handle(
        ApproveUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) return false;

        user.IsApproved = true;
        await userManager.UpdateAsync(user);
        return true;
    }
}

public class RejectUserHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<RejectUserCommand, bool>
{
    public async Task<bool> Handle(
        RejectUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) return false;

        // Delete the rejected user
        await userManager.DeleteAsync(user);
        return true;
    }
}
