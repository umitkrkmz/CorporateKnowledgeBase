namespace CorporateKnowledgeBase.Infrastructure.Services;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

public class AuthorizationChecker(UserManager<ApplicationUser> userManager)
    : IAuthorizationChecker
{
    public async Task<bool> CanEditContentAsync(string userId, string authorId)
    {
        // Author can always edit their own content
        if (userId == authorId) return true;

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return false;

        var roles = await userManager.GetRolesAsync(user);
        // Editor and Admin can edit any content
        return roles.Contains("Admin") || roles.Contains("Editor");
    }

    public async Task<bool> CanDeleteContentAsync(string userId, string authorId)
    {
        // Author can delete their own content
        if (userId == authorId) return true;

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return false;

        var roles = await userManager.GetRolesAsync(user);
        // Only Admin can delete others' content
        return roles.Contains("Admin");
    }

    public async Task<bool> IsApprovedAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.IsApproved ?? false;
    }
}
