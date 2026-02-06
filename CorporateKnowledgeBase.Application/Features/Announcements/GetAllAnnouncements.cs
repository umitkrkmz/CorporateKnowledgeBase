namespace CorporateKnowledgeBase.Application.Features.Announcements;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllAnnouncementsQuery : IRequest<List<AnnouncementDto>>;

public record AnnouncementDto(
    int Id,
    string Title,
    string Content,
    string AuthorId,
    DateTime CreatedAt);

public class GetAllAnnouncementsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllAnnouncementsQuery, List<AnnouncementDto>>
{
    public async Task<List<AnnouncementDto>> Handle(
        GetAllAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        return await context.Announcements
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AnnouncementDto(
                a.Id, a.Title, a.Content, a.AuthorId, a.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
