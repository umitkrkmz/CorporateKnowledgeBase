namespace CorporateKnowledgeBase.Web.Controllers;

using CorporateKnowledgeBase.Application.Features.Likes;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class LikesController(IMediator mediator) : ControllerBase
{
    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle([FromBody] LikeRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await mediator.Send(new ToggleLikeCommand(
            userId, request.ContentId, request.ContentType));
        return Ok(result);
    }
}

public record LikeRequest(int ContentId, ContentType ContentType);
