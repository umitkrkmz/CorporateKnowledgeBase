namespace CorporateKnowledgeBase.Application.Features.Reports;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteReportedContentCommand(int ReportId) : IRequest<bool>;

public class DeleteReportedContentHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteReportedContentCommand, bool>
{
    public async Task<bool> Handle(
        DeleteReportedContentCommand request, CancellationToken cancellationToken)
    {
        var report = await context.ContentReports
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken);

        if (report is null) return false;

        // Delete the reported content
        switch (report.ContentType)
        {
            case ContentType.Document:
                var doc = await context.Documents
                    .FirstOrDefaultAsync(d => d.Id == report.ContentId, cancellationToken);
                if (doc is not null) context.Documents.Remove(doc);
                break;

            case ContentType.BlogPost:
                var post = await context.BlogPosts
                    .FirstOrDefaultAsync(b => b.Id == report.ContentId, cancellationToken);
                if (post is not null) context.BlogPosts.Remove(post);
                break;

            case ContentType.Announcement:
                var ann = await context.Announcements
                    .FirstOrDefaultAsync(a => a.Id == report.ContentId, cancellationToken);
                if (ann is not null) context.Announcements.Remove(ann);
                break;
        }

        // Auto-resolve the report
        report.Status = ReportStatus.Reviewed;
        report.AdminNotes = "Content deleted due to violation.";
        report.ReviewedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
