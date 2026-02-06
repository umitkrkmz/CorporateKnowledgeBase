namespace CorporateKnowledgeBase.Application.Features.Reports;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllReportsQuery : IRequest<List<ReportDto>>;

public record ReportDto(
    int Id,
    string ReporterId,
    string ReporterName,
    int ContentId,
    ContentType ContentType,
    string Reason,
    ReportStatus Status,
    string? AdminNotes,
    DateTime CreatedAt);

public class GetAllReportsHandler(
    IApplicationDbContext context,
    IUserNameResolver userNameResolver)
    : IRequestHandler<GetAllReportsQuery, List<ReportDto>>
{
    public async Task<List<ReportDto>> Handle(
        GetAllReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await context.ContentReports
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<ReportDto>();
        foreach (var r in reports)
        {
            var reporterName = await userNameResolver.GetFullNameAsync(r.ReporterId);
            result.Add(new ReportDto(
                r.Id, r.ReporterId, reporterName, r.ContentId, r.ContentType,
                r.Reason, r.Status, r.AdminNotes, r.CreatedAt));
        }

        return result;
    }
}
