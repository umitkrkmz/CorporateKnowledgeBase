namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Announcements;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class AnnouncementsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
    {
        var result = await mediator.Send(new GetAnnouncementsPagedQuery(page, pageSize, search));

        ViewBag.Search = search;
        ViewBag.PageSize = pageSize;

        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var announcement = await mediator.Send(new GetAnnouncementByIdQuery(id));
        if (announcement is null) return NotFound();
        return View(announcement);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAnnouncementCommand command)
    {
        await mediator.Send(command);
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var announcement = await mediator.Send(new GetAnnouncementByIdQuery(id));
        if (announcement is null) return NotFound();
        return View(announcement);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, string title, string content)
    {
        await mediator.Send(new UpdateAnnouncementCommand(id, title, content));
        return RedirectToAction("Details", new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteAnnouncementCommand(id));
        return RedirectToAction("Index");
    }
}
