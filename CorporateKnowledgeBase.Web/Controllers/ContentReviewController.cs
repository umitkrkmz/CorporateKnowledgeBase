namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Features.Content;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Editor")]
public class ContentReviewController(IMediator mediator, IApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var pendingDocs = await context.Documents
            .Where(d => d.Status == ContentStatus.PendingReview)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new PendingContentItem
            {
                Id = d.Id,
                Title = d.Title,
                AuthorId = d.AuthorId,
                ContentType = ContentType.Document,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();

        var pendingPosts = await context.BlogPosts
            .Where(b => b.Status == ContentStatus.PendingReview)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new PendingContentItem
            {
                Id = b.Id,
                Title = b.Title,
                AuthorId = b.AuthorId,
                ContentType = ContentType.BlogPost,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();

        var items = pendingDocs.Concat(pendingPosts)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id, ContentType contentType)
    {
        await mediator.Send(new ApproveContentCommand(id, contentType));
        TempData["Success"] = "Content has been approved and published.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id, ContentType contentType, string reason)
    {
        await mediator.Send(new RejectContentCommand(id, contentType, reason));
        TempData["Success"] = "Content has been rejected.";
        return RedirectToAction("Index");
    }
}

public class PendingContentItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public DateTime CreatedAt { get; set; }
}
