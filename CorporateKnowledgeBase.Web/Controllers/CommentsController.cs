namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Features.Comments;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
public class CommentsController(IMediator mediator, IUserNameResolver userNameResolver) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Create(string content, int? documentId, int? blogPostId, int? parentCommentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        ContentType type;
        if (documentId.HasValue)
            type = ContentType.Document;
        else if (blogPostId.HasValue)
            type = ContentType.BlogPost;
        else
            return BadRequest();

        var commentId = await mediator.Send(new CreateCommentCommand(
            content, userId, documentId, blogPostId, type, parentCommentId));

        var authorName = await userNameResolver.GetFullNameAsync(userId);
        var authorImage = await userNameResolver.GetProfileImagePathAsync(userId);

        return Json(new
        {
            id = commentId,
            authorName,
            authorImagePath = authorImage,
            content,
            parentCommentId,
            createdAt = DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm")
        });
    }
}
