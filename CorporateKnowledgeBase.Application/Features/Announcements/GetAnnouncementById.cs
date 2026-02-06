namespace CorporateKnowledgeBase.Application.Features.Announcements;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAnnouncementByIdQuery(int Id) : IRequest<AnnouncementDto?>;

public class GetAnnouncementByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetAnnouncementByIdQuery, AnnouncementDto?>
{
    public async Task<AnnouncementDto?> Handle(
        GetAnnouncementByIdQuery request, CancellationToken cancellationToken)
    {
        return await context.Announcements
            .Where(a => a.Id == request.Id)
            .Select(a => new AnnouncementDto(
                a.Id, a.Title, a.Content, a.AuthorId, a.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
