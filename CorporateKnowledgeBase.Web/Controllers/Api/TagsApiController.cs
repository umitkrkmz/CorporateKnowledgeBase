namespace CorporateKnowledgeBase.Web.Controllers.Api;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class TagsApiController(IApplicationDbContext context) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term = "")
    {
        var tags = await context.Tags
            .Where(t => string.IsNullOrEmpty(term) || t.Name.Contains(term))
            .OrderBy(t => t.Name)
            .Take(20)
            .Select(t => new { value = t.Name })
            .ToListAsync();

        return Ok(tags);
    }
}
