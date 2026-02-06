namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminCategoriesController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? sortBy = null,
        bool sortDesc = false)
    {
        var query = new GetCategoriesPagedQuery
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
    public async Task<IActionResult> Create(CreateCategoryCommand command)
    {
        await mediator.Send(command);
        TempData["Success"] = "Category created successfully.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var categories = await mediator.Send(new GetAllCategoriesQuery());
        var category = categories.FirstOrDefault(c => c.Id == id);
        if (category is null) return NotFound();
        return View(category);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateCategoryCommand command)
    {
        await mediator.Send(command);
        TempData["Success"] = "Category updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteCategoryCommand(id));
        TempData["Success"] = "Category deleted successfully.";
        return RedirectToAction("Index");
    }
}
