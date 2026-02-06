namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class HomeController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index()
    {
        var stats = await mediator.Send(new GetDashboardStatsQuery());
        return View(stats);
    }

    public IActionResult Privacy() => View();

    public IActionResult Terms() => View();
}
