namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Faq;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class FaqController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index()
    {
        var isAdmin = User.IsInRole("Admin");
        var faqs = await mediator.Send(new GetAllFaqsQuery(IncludeUnpublished: isAdmin));
        return View(faqs);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateFaqCommand command)
    {
        await mediator.Send(command);
        TempData["Success"] = "FAQ item created.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var faqs = await mediator.Send(new GetAllFaqsQuery(IncludeUnpublished: true));
        var faq = faqs.FirstOrDefault(f => f.Id == id);
        if (faq is null) return NotFound();
        return View(faq);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(UpdateFaqCommand command)
    {
        await mediator.Send(command);
        TempData["Success"] = "FAQ item updated.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteFaqCommand(id));
        TempData["Success"] = "FAQ item deleted.";
        return RedirectToAction("Index");
    }
}
