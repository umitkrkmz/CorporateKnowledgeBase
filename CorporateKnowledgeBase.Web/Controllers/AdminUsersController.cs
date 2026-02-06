namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminUsersController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? status = null,
        string? role = null,
        string? sortBy = null,
        bool sortDesc = true)
    {
        var query = new GetUsersPagedQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = search,
            Status = status,
            Role = role,
            SortBy = sortBy,
            SortDescending = sortDesc
        };

        var result = await mediator.Send(query);

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Role = role;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDesc = sortDesc;
        ViewBag.PageSize = pageSize;

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> EditRoles(string id)
    {
        var users = await mediator.Send(new GetAllUsersQuery());
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null) return NotFound();
        ViewBag.AllRoles = new[] { "Admin", "Editor", "Member" };
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> EditRoles(string id, List<string> roles)
    {
        await mediator.Send(new UpdateUserRolesCommand(id, roles));
        TempData["Success"] = "User roles updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Approve(string id)
    {
        await mediator.Send(new ApproveUserCommand(id));
        TempData["Success"] = "User has been approved.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Reject(string id)
    {
        await mediator.Send(new RejectUserCommand(id));
        TempData["Success"] = "User has been rejected and removed.";
        return RedirectToAction("Index");
    }
}
