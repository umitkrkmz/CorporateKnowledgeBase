namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Notifications;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
public class NotificationsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var notifications = await mediator.Send(new GetAllUserNotificationsQuery(userId));
        return View(notifications);
    }

    [HttpGet]
    public async Task<IActionResult> GetDropdown()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await mediator.Send(new GetUserNotificationsQuery(userId));
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await mediator.Send(new MarkNotificationsReadCommand(userId));
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GoTo(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Mark as read
        await mediator.Send(new MarkNotificationReadCommand(id, userId));

        // Get the notification to find where to redirect
        var all = await mediator.Send(new GetAllUserNotificationsQuery(userId));
        var notif = all.FirstOrDefault(n => n.Id == id);

        if (notif?.ReferenceId is null)
            return RedirectToAction("Index");

        return notif.ContentType switch
        {
            ContentType.Document => RedirectToAction("Details", "Documents", new { id = notif.ReferenceId }),
            ContentType.BlogPost => RedirectToAction("Details", "BlogPosts", new { id = notif.ReferenceId }),
            ContentType.Announcement => RedirectToAction("Index", "Announcements"),
            _ => RedirectToAction("Index")
        };
    }
}
