namespace CorporateKnowledgeBase.Application.Features.Reports;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record ReviewReportCommand(
    int Id,
    ReportStatus Status,
    string? AdminNotes) : IRequest<bool>;

public class ReviewReportHandler(IApplicationDbContext context)
    : IRequestHandler<ReviewReportCommand, bool>
{
    public async Task<bool> Handle(
        ReviewReportCommand request, CancellationToken cancellationToken)
    {
        var report = await context.ContentReports
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (report is null) return false;

        report.Status = request.Status;
        report.AdminNotes = request.AdminNotes;
        report.ReviewedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
