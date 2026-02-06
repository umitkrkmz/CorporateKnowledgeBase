namespace CorporateKnowledgeBase.Infrastructure.Services;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Features.Profile;
using CorporateKnowledgeBase.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class GetUserProfileHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext context)
    : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    public async Task<UserProfileDto?> Handle(
        GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) return null;

        var roles = await userManager.GetRolesAsync(user);
        var docCount = await context.Documents.CountAsync(d => d.AuthorId == request.UserId, cancellationToken);
        var blogCount = await context.BlogPosts.CountAsync(b => b.AuthorId == request.UserId, cancellationToken);

        return new UserProfileDto(
            user.Id, user.FullName, user.Email ?? "",
            user.Department, user.DepartmentId, user.JobTitle, user.ProfileImagePath,
            roles.ToList(), user.CreatedAt, user.LastLoginDate,
            docCount, blogCount,
            user.NotifyOnNewComment, user.NotifyOnNewBlogPost,
            user.NotifyOnNewDocument, user.NotifyOnNewAnnouncement);
    }
}

public class UpdateProfileHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext context)
    : IRequestHandler<UpdateProfileCommand, bool>
{
    public async Task<bool> Handle(
        UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) return false;

        user.FullName = request.FullName;
        user.DepartmentId = request.DepartmentId;
        user.JobTitle = request.JobTitle;

        // Update the department name as well (for display).
        if (request.DepartmentId.HasValue)
        {
            var dept = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken);
            user.Department = dept?.Name;
        }
        else
        {
            user.Department = null;
        }

        await userManager.UpdateAsync(user);
        return true;
    }
}

public class UploadProfilePhotoHandler(
    UserManager<ApplicationUser> userManager,
    Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    : IRequestHandler<UploadProfilePhotoCommand, string?>
{
    public async Task<string?> Handle(
        UploadProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) return null;

        // Ensure upload directory exists
        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "profiles");
        Directory.CreateDirectory(uploadsDir);

        // Delete old photo if exists
        if (!string.IsNullOrEmpty(user.ProfileImagePath))
        {
            var oldPath = Path.Combine(env.WebRootPath, user.ProfileImagePath.TrimStart('/'));
            if (File.Exists(oldPath)) File.Delete(oldPath);
        }

        // Save new photo
        var ext = Path.GetExtension(request.FileName);
        var fileName = $"{request.UserId}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);
        await File.WriteAllBytesAsync(filePath, request.FileData, cancellationToken);

        // Update user record
        user.ProfileImagePath = $"/uploads/profiles/{fileName}";
        await userManager.UpdateAsync(user);

        return user.ProfileImagePath;
    }
}

public class UpdateNotificationSettingsHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<UpdateNotificationSettingsCommand, bool>
{
    public async Task<bool> Handle(
        UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) return false;

        user.NotifyOnNewComment = request.NotifyOnNewComment;
        user.NotifyOnNewBlogPost = request.NotifyOnNewBlogPost;
        user.NotifyOnNewDocument = request.NotifyOnNewDocument;
        user.NotifyOnNewAnnouncement = request.NotifyOnNewAnnouncement;

        await userManager.UpdateAsync(user);
        return true;
    }
}
