namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminSettingsController(IServiceProvider serviceProvider) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedDemoData()
    {
        var result = await SeedData.SeedDemoDataAsync(serviceProvider);

        if (result.AlreadySeeded)
            TempData["Warning"] = result.Message;
        else
            TempData["Success"] = result.Message;

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearDemoData()
    {
        var message = await SeedData.ClearDemoDataAsync(serviceProvider);
        TempData["Success"] = message;
        return RedirectToAction("Index");
    }
}
