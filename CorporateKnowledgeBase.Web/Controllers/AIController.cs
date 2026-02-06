namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class AIController(IAIAssistantService aiService, IApplicationDbContext context) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

    [HttpGet]
    public async Task<IActionResult> Index(int? sessionId = null)
    {
        // Get all user sessions
        var sessions = await aiService.GetUserSessionsAsync(UserId);
        ViewBag.Sessions = sessions;

        // If a specific session was requested, it will also receive the messages.
        if (sessionId.HasValue)
        {
            var session = await aiService.GetSessionWithMessagesAsync(sessionId.Value, UserId);
            if (session != null)
            {
                ViewBag.CurrentSession = session;
                ViewBag.Messages = session.Messages.ToList();
            }
        }

        // Get user profile
        var profile = await aiService.GetOrCreateProfileAsync(UserId);
        ViewBag.Profile = profile;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Ask(string question, int? sessionId)
    {
        if (string.IsNullOrWhiteSpace(question))
            return Json(new { success = false, error = "Lütfen bir soru girin." });

        var response = await aiService.AskAsync(question, UserId, sessionId);

        return Json(new
        {
            success = string.IsNullOrEmpty(response.Error),
            answer = response.Content,
            sessionId = response.SessionId,
            messageId = response.MessageId,
            isDocumentBased = response.IsDocumentBased,
            referencedTitle = response.ReferencedTitle,
            matchScore = response.MatchScore,
            error = response.Error
        });
    }

    [HttpPost]
    public async Task<IActionResult> NewSession()
    {
        var session = await aiService.CreateSessionAsync(UserId);
        return Json(new { success = true, sessionId = session.Id, title = session.Title });
    }

    [HttpPost]
    public async Task<IActionResult> ArchiveSession(int sessionId)
    {
        await aiService.ArchiveSessionAsync(sessionId, UserId);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSession(int sessionId)
    {
        await aiService.DeleteSessionAsync(sessionId, UserId);
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetSession(int sessionId)
    {
        var session = await aiService.GetSessionWithMessagesAsync(sessionId, UserId);
        if (session == null)
            return Json(new { success = false, error = "Oturum bulunamadı." });

        var messages = session.Messages.Select(m => new
        {
            id = m.Id,
            role = m.Role.ToString().ToLower(),
            content = m.Content,
            referencedDocumentId = m.ReferencedDocumentId,
            referencedBlogPostId = m.ReferencedBlogPostId,
            matchScore = m.MatchScore,
            createdAt = m.CreatedAt.ToString("HH:mm")
        }).ToList();

        return Json(new { success = true, session = new { session.Id, session.Title }, messages });
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await aiService.GetUserSessionsAsync(UserId);
        var result = sessions.Select(s => new
        {
            id = s.Id,
            title = s.Title,
            lastMessageAt = s.LastMessageAt?.ToString("dd MMM HH:mm") ?? s.CreatedAt.ToString("dd MMM HH:mm"),
            isArchived = s.IsArchived
        }).ToList();

        return Json(new { success = true, sessions = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> RebuildEmbeddings()
    {
        var docIds = await context.Documents.Select(d => d.Id).ToListAsync();
        var blogIds = await context.BlogPosts.Select(b => b.Id).ToListAsync();

        foreach (var id in docIds)
            await aiService.UpdateDocumentEmbeddingAsync(id);
        foreach (var id in blogIds)
            await aiService.UpdateBlogPostEmbeddingAsync(id);

        TempData["Message"] = $"Embedding'ler güncellendi: {docIds.Count} doküman, {blogIds.Count} blog.";
        return RedirectToAction("Index");
    }
}
