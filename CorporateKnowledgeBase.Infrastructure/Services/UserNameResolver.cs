namespace CorporateKnowledgeBase.Infrastructure.Services;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

public class UserNameResolver(UserManager<ApplicationUser> userManager)
    : IUserNameResolver
{
    public async Task<string> GetFullNameAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.FullName ?? userId;
    }

    public async Task<string?> GetProfileImagePathAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.ProfileImagePath;
    }
}
