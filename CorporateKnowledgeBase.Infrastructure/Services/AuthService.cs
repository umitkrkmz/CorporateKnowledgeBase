namespace CorporateKnowledgeBase.Infrastructure.Services;

using CorporateKnowledgeBase.Application.Features.Auth;
using CorporateKnowledgeBase.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

public class LoginHandler(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return new LoginResult(false, "Invalid email or password.");

        // Check password first
        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return new LoginResult(false, "Invalid email or password.");

        // Check if user is approved
        if (!user.IsApproved)
            return new LoginResult(false, null, IsPendingApproval: true);

        // Sign in
        await signInManager.SignInAsync(user, request.RememberMe);

        user.LastLoginDate = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        return new LoginResult(true, null);
    }
}

public class RegisterHandler(
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(
        RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            Department = request.Department,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Member");
            // Do NOT auto-sign in - user must wait for admin approval
        }

        return new RegisterResult(
            result.Succeeded,
            result.Errors.Select(e => e.Description),
            IsPendingApproval: result.Succeeded);
    }
}
