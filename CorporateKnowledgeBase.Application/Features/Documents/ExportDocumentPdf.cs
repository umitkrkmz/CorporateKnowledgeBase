namespace CorporateKnowledgeBase.Application.Features.Documents;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record ExportDocumentPdfQuery(int Id) : IRequest<PdfExportResult?>;

public record PdfExportResult(byte[] FileBytes, string FileName);

public class ExportDocumentPdfHandler(
    IApplicationDbContext context,
    IPdfExportService pdfService,
    IUserNameResolver userNameResolver)
    : IRequestHandler<ExportDocumentPdfQuery, PdfExportResult?>
{
    public async Task<PdfExportResult?> Handle(
        ExportDocumentPdfQuery request, CancellationToken cancellationToken)
    {
        var doc = await context.Documents
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (doc is null) return null;

        var authorName = await userNameResolver.GetFullNameAsync(doc.AuthorId);

        var pdfBytes = pdfService.GenerateDocumentPdf(
            doc.Title, doc.Content, authorName,
            doc.Category.Name, doc.CreatedAt);

        var fileName = $"{doc.Title.Replace(" ", "_")}.pdf";

        return new PdfExportResult(pdfBytes, fileName);
    }
}
