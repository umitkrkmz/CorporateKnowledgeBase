namespace CorporateKnowledgeBase.Web.Controllers.Api;

using CorporateKnowledgeBase.Application.Features.Reports;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsApiController(IMediator mediator) : ControllerBase
{
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitReportRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { success = false, message = "Please select a reason." });

        if (request.Reason == "Other" && string.IsNullOrWhiteSpace(request.Description))
            return BadRequest(new { success = false, message = "Please provide a description for 'Other'." });

        var reason = string.IsNullOrWhiteSpace(request.Description)
            ? request.Reason
            : $"{request.Reason}: {request.Description}";

        await mediator.Send(new CreateContentReportCommand(
            userId, request.ContentId, request.ContentType, reason));

        return Ok(new { success = true, message = "Report submitted successfully. Administrators will review it." });
    }
}

public record SubmitReportRequest(
    int ContentId,
    ContentType ContentType,
    string Reason,
    string? Description);
