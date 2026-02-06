namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Auth;
using CorporateKnowledgeBase.Infrastructure.Identity;
using CorporateKnowledgeBase.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class AccountController(
    IMediator mediator,
    SignInManager<ApplicationUser> signInManager) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        var vm = new AuthViewModel { ShowRegister = false };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Login(AuthViewModel vm)
    {
        ModelState.Clear();
        var result = await mediator.Send(vm.Login);

        if (result.IsPendingApproval)
        {
            TempData["PendingApproval"] = "Your account is pending administrator approval. Please try again later.";
            vm.ShowRegister = false;
            return View(vm);
        }

        if (result.Succeeded) return RedirectToAction("Index", "Home");

        ModelState.AddModelError("Login.Email", result.Error!);
        vm.ShowRegister = false;
        return View(vm);
    }

    [HttpGet]
    public IActionResult Register()
    {
        var vm = new AuthViewModel { ShowRegister = true };
        return View("Login", vm);
    }

    [HttpPost]
    public async Task<IActionResult> Register(AuthViewModel vm)
    {
        ModelState.Clear();
        var result = await mediator.Send(vm.Register);

        if (result.IsPendingApproval)
        {
            TempData["RegistrationSuccess"] = "Your account has been created successfully. An administrator will review and approve your account shortly.";
            return RedirectToAction("Login");
        }

        if (result.Succeeded) return RedirectToAction("Login");

        foreach (var error in result.Errors)
            ModelState.AddModelError("Register.Email", error);
        vm.ShowRegister = true;
        return View("Login", vm);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
