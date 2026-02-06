namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Tags;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminTagsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? sortBy = null,
        bool sortDesc = false)
    {
        var query = new GetTagsPagedQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = search,
            SortBy = sortBy,
            SortDescending = sortDesc
        };

        var result = await mediator.Send(query);

        ViewBag.Search = search;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDesc = sortDesc;
        ViewBag.PageSize = pageSize;

        return View(result);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(CreateTagCommand command)
    {
        await mediator.Send(command);
        TempData["Success"] = "Tag created successfully.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var tags = await mediator.Send(new GetAllTagsQuery());
        var tag = tags.FirstOrDefault(t => t.Id == id);
        if (tag is null) return NotFound();
        return View(tag);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateTagCommand command)
    {
        await mediator.Send(command);
        TempData["Success"] = "Tag updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteTagCommand(id));
        TempData["Success"] = "Tag deleted successfully.";
        return RedirectToAction("Index");
    }
}
