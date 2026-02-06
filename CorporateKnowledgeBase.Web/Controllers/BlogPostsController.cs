namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Common.Helpers;
using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Features.BlogPosts;
using CorporateKnowledgeBase.Application.Features.Categories;
using CorporateKnowledgeBase.Application.Features.Comments;
using CorporateKnowledgeBase.Application.Features.Content;
using CorporateKnowledgeBase.Application.Features.Departments;
using CorporateKnowledgeBase.Domain.Enums;
using CorporateKnowledgeBase.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

public class BlogPostsController(
    IMediator mediator,
    IApplicationDbContext context,
    IAuthorizationChecker authChecker,
    IWikiLinkResolver wikiLinkResolver,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(
        string? tag,
        string? search,
        int? categoryId,
        string? sortBy,
        bool sortDesc = false,
        int page = 1,
        int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isEditorOrAdmin = User.IsInRole("Admin") || User.IsInRole("Editor");
        int? userDeptId = null;
        if (userId != null)
        {
            var appUser = await userManager.FindByIdAsync(userId);
            userDeptId = appUser?.DepartmentId;
        }

        var result = await mediator.Send(new GetBlogPostsPagedQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = search,
            SortBy = sortBy,
            SortDescending = sortDesc,
            CategoryId = categoryId,
            Tag = tag,
            UserId = userId,
            IncludeAll = isEditorOrAdmin,
            UserDepartmentId = userDeptId
        });

        // Pass filter values to view
        ViewBag.CurrentTag = tag;
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentCategoryId = categoryId;
        ViewBag.CurrentSortBy = sortBy;
        ViewBag.CurrentSortDesc = sortDesc;
        ViewBag.CurrentPageSize = pageSize;
        ViewBag.Categories = await mediator.Send(new GetAllCategoriesQuery());

        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var post = await mediator.Send(new GetBlogPostByIdQuery(id));
        if (post is null) return NotFound();

        // Increment view count
        await mediator.Send(new IncrementViewCountCommand(id, ContentType.BlogPost));

        // Load comments
        var comments = await mediator.Send(new GetCommentsQuery(id, ContentType.BlogPost));
        ViewBag.Comments = comments;

        // Load like info
        var likeCount = await context.UserLikes.CountAsync(l => l.BlogPostId == id);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isLiked = userId != null && await context.UserLikes
            .AnyAsync(l => l.BlogPostId == id && l.UserId == userId);
        ViewBag.LikeCount = likeCount;
        ViewBag.IsLiked = isLiked;

        // Authorization checks for action buttons
        if (userId != null)
        {
            ViewBag.CanEdit = await authChecker.CanEditContentAsync(userId, post.AuthorId);
            ViewBag.CanDelete = await authChecker.CanDeleteContentAsync(userId, post.AuthorId);
            ViewBag.IsApproved = await authChecker.IsApprovedAsync(userId);
        }
        else
        {
            ViewBag.CanEdit = false;
            ViewBag.CanDelete = false;
            ViewBag.IsApproved = false;
        }

        // Resolve wiki links
        ViewBag.ResolvedContent = await wikiLinkResolver.ResolveAsync(post.Content);

        return View(post);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await authChecker.IsApprovedAsync(userId))
            return Forbid();

        ViewBag.Categories = await mediator.Send(new GetAllCategoriesQuery());
        ViewBag.Departments = await mediator.Send(new GetAllDepartmentsQuery());
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateBlogPostCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await authChecker.IsApprovedAsync(userId))
            return Forbid();

        await mediator.Send(command);
        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await mediator.Send(new GetBlogPostByIdQuery(id));
        if (post is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await authChecker.CanEditContentAsync(userId, post.AuthorId))
            return Forbid();

        ViewBag.Categories = await mediator.Send(new GetAllCategoriesQuery());
        ViewBag.Departments = await mediator.Send(new GetAllDepartmentsQuery());
        ViewBag.ExistingTags = JsonSerializer.Serialize(post.Tags.Select(t => new { value = t }));
        return View(post);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, string title, string content, int categoryId, string? tagsInput, int? departmentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var post = await mediator.Send(new GetBlogPostByIdQuery(id));
        if (post is null) return NotFound();
        if (!await authChecker.CanEditContentAsync(userId, post.AuthorId))
            return Forbid();

        await mediator.Send(new UpdateBlogPostCommand(id, title, content, categoryId, userId, tagsInput, departmentId));
        return RedirectToAction("Details", new { id });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var post = await mediator.Send(new GetBlogPostByIdQuery(id));
        if (post is null) return NotFound();
        if (!await authChecker.CanDeleteContentAsync(userId, post.AuthorId))
            return Forbid();

        await mediator.Send(new DeleteBlogPostCommand(id));
        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SubmitForReview(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await mediator.Send(new SubmitForReviewCommand(id, ContentType.BlogPost, userId));
        TempData["Success"] = "Blog post submitted for review.";
        return RedirectToAction("Details", new { id });
    }
}
