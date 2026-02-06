namespace CorporateKnowledgeBase.Application.Features.Documents;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllDocumentsQuery(string? Tag = null, string? UserId = null, bool IncludeAll = false, int? UserDepartmentId = null) : IRequest<List<DocumentDto>>;

public record DocumentDto(
    int Id,
    string Title,
    string AuthorId,
    string CategoryName,
    int ViewCount,
    DateTime CreatedAt,
    ContentStatus Status,
    List<string> Tags);

public class GetAllDocumentsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllDocumentsQuery, List<DocumentDto>>
{
    public async Task<List<DocumentDto>> Handle(
        GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Documents
            .Include(d => d.Category)
            .Include(d => d.Tags)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Tag))
            query = query.Where(d => d.Tags.Any(t => t.Name == request.Tag));

        // Filter by status: show Published to everyone, show own drafts to author
        if (!request.IncludeAll)
        {
            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(d =>
                    d.Status == ContentStatus.Published ||
                    d.AuthorId == request.UserId);
            }
            else
            {
                query = query.Where(d => d.Status == ContentStatus.Published);
            }
        }

        // Department filter: show content with no department restriction, or matching user's department
        if (!request.IncludeAll && request.UserDepartmentId.HasValue)
        {
            query = query.Where(d =>
                d.DepartmentId == null ||
                d.DepartmentId == request.UserDepartmentId);
        }

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DocumentDto(
                d.Id,
                d.Title,
                d.AuthorId,
                d.Category.Name,
                d.ViewCount,
                d.CreatedAt,
                d.Status,
                d.Tags.Select(t => t.Name).ToList()))
            .ToListAsync(cancellationToken);
    }
}
