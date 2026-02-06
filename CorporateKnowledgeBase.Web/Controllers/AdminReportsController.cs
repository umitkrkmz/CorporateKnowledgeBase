namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Reports;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminReportsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        ReportStatus? status = null,
        ContentType? contentType = null,
        string? sortBy = null,
        bool sortDesc = true)
    {
        var query = new GetReportsPagedQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = search,
            Status = status,
            ContentType = contentType,
            SortBy = sortBy,
            SortDescending = sortDesc
        };

        var result = await mediator.Send(query);

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.ContentType = contentType;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDesc = sortDesc;
        ViewBag.PageSize = pageSize;

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Review(int id)
    {
        var report = await mediator.Send(new GetReportByIdQuery(id));
        if (report is null) return NotFound();
        return View(report);
    }

    [HttpPost]
    public async Task<IActionResult> Review(int id, ReportStatus status, string? adminNotes)
    {
        await mediator.Send(new ReviewReportCommand(id, status, adminNotes));
        TempData["Success"] = "Report has been updated.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteContent(int id)
    {
        await mediator.Send(new DeleteReportedContentCommand(id));
        TempData["Success"] = "Content deleted and report closed.";
        return RedirectToAction("Index");
    }
}
