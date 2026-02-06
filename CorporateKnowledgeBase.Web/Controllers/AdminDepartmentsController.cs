namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Departments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminDepartmentsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? sortBy = null,
        bool sortDesc = false)
    {
        var query = new GetDepartmentsPagedQuery
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
    public async Task<IActionResult> Create(CreateDepartmentCommand command)
    {
        await mediator.Send(command);
        TempData["Success"] = "Department created successfully.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var departments = await mediator.Send(new GetAllDepartmentsQuery());
        var dept = departments.FirstOrDefault(d => d.Id == id);
        if (dept is null) return NotFound();
        return View(dept);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateDepartmentCommand command)
    {
        await mediator.Send(command);
        TempData["Success"] = "Department updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteDepartmentCommand(id));
        TempData["Success"] = "Department deleted successfully.";
        return RedirectToAction("Index");
    }
}
