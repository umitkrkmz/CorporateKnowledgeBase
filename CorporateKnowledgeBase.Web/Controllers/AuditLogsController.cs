namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.AuditLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AuditLogsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? action = null,
        string? sortBy = null,
        bool sortDesc = true)
    {
        var query = new GetAuditLogsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = search,
            Action = action,
            SortBy = sortBy,
            SortDescending = sortDesc
        };

        var result = await mediator.Send(query);

        ViewBag.Search = search;
        ViewBag.Action = action;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDesc = sortDesc;
        ViewBag.PageSize = pageSize;

        return View(result);
    }
}
