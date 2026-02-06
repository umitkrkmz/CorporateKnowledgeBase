namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class SearchController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return View(new SearchResultDto([], []));

        ViewBag.SearchTerm = q;
        var results = await mediator.Send(new SearchQuery(q));
        return View(results);
    }
}
