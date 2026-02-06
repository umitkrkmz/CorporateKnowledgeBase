namespace CorporateKnowledgeBase.Application.Features.Reports;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetReportByIdQuery(int Id) : IRequest<ReportDetailDto?>;

public record ReportDetailDto(
    int Id,
    string ReporterId,
    string ReporterName,
    int ContentId,
    ContentType ContentType,
    string? ContentTitle,
    string Reason,
    ReportStatus Status,
    string? AdminNotes,
    DateTime CreatedAt,
    DateTime? ReviewedAt);

public class GetReportByIdHandler(
    IApplicationDbContext context,
    IUserNameResolver userNameResolver)
    : IRequestHandler<GetReportByIdQuery, ReportDetailDto?>
{
    public async Task<ReportDetailDto?> Handle(
        GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await context.ContentReports
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (report is null) return null;

        var reporterName = await userNameResolver.GetFullNameAsync(report.ReporterId);

        // Resolve content title
        string? contentTitle = null;
        switch (report.ContentType)
        {
            case ContentType.Document:
                contentTitle = await context.Documents
                    .Where(d => d.Id == report.ContentId)
                    .Select(d => d.Title)
                    .FirstOrDefaultAsync(cancellationToken);
                break;
            case ContentType.BlogPost:
                contentTitle = await context.BlogPosts
                    .Where(b => b.Id == report.ContentId)
                    .Select(b => b.Title)
                    .FirstOrDefaultAsync(cancellationToken);
                break;
            case ContentType.Announcement:
                contentTitle = await context.Announcements
                    .Where(a => a.Id == report.ContentId)
                    .Select(a => a.Title)
                    .FirstOrDefaultAsync(cancellationToken);
                break;
        }

        return new ReportDetailDto(
            report.Id, report.ReporterId, reporterName,
            report.ContentId, report.ContentType, contentTitle,
            report.Reason, report.Status, report.AdminNotes,
            report.CreatedAt, report.ReviewedAt);
    }
}
