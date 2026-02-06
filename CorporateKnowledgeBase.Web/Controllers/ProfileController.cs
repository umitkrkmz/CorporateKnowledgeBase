namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Features.Profile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class ProfileController(IMediator mediator, IApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await mediator.Send(new GetUserProfileQuery(userId));
        if (profile is null) return NotFound();
        return View(profile);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await mediator.Send(new GetUserProfileQuery(userId));
        if (profile is null) return NotFound();

        await LoadDepartmentsAsync(profile.DepartmentId);
        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string fullName, int? departmentId, string? jobTitle)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await mediator.Send(new UpdateProfileCommand(userId, fullName, departmentId, jobTitle));
        TempData["Message"] = "Profile updated successfully.";
        return RedirectToAction("Index");
    }

    private async Task LoadDepartmentsAsync(int? selectedDepartmentId)
    {
        var departments = await context.Departments
            .OrderBy(d => d.Name)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name,
                Selected = d.Id == selectedDepartmentId
            })
            .ToListAsync();

        departments.Insert(0, new SelectListItem { Value = "", Text = "-- Select Department --" });
        ViewBag.Departments = departments;
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(IFormFile photo)
    {
        if (photo is null || photo.Length == 0)
        {
            TempData["Error"] = "Please select a valid image file.";
            return RedirectToAction("Edit");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            TempData["Error"] = "Only image files (JPG, PNG, GIF, WebP) are allowed.";
            return RedirectToAction("Edit");
        }

        if (photo.Length > 5 * 1024 * 1024)
        {
            TempData["Error"] = "File size must be less than 5 MB.";
            return RedirectToAction("Edit");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        using var ms = new MemoryStream();
        await photo.CopyToAsync(ms);

        await mediator.Send(new UploadProfilePhotoCommand(userId, photo.FileName, ms.ToArray()));
        TempData["Message"] = "Profile photo updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> NotificationSettings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await mediator.Send(new GetUserProfileQuery(userId));
        if (profile is null) return NotFound();
        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> NotificationSettings(
        bool notifyOnNewComment, bool notifyOnNewBlogPost,
        bool notifyOnNewDocument, bool notifyOnNewAnnouncement)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await mediator.Send(new UpdateNotificationSettingsCommand(
            userId, notifyOnNewComment, notifyOnNewBlogPost,
            notifyOnNewDocument, notifyOnNewAnnouncement));
        TempData["Message"] = "Notification settings saved.";
        return RedirectToAction("Index");
    }
}
