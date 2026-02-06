namespace CorporateKnowledgeBase.Application.Features.Reports;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;

public record CreateContentReportCommand(
    string ReporterId,
    int ContentId,
    ContentType ContentType,
    string Reason) : IRequest<int>;

public class CreateContentReportHandler(IApplicationDbContext context)
    : IRequestHandler<CreateContentReportCommand, int>
{
    public async Task<int> Handle(
        CreateContentReportCommand request, CancellationToken cancellationToken)
    {
        var report = new ContentReport
        {
            ReporterId = request.ReporterId,
            ContentId = request.ContentId,
            ContentType = request.ContentType,
            Reason = request.Reason,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        context.ContentReports.Add(report);
        await context.SaveChangesAsync(cancellationToken);

        return report.Id;
    }
}
