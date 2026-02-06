namespace CorporateKnowledgeBase.Infrastructure.Services;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class PdfExportService : IPdfExportService
{
    public byte[] GenerateDocumentPdf(
        string title, string content, string author, string category, DateTime createdAt)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("Corporate Knowledge Base")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(10).Text(title)
                        .FontSize(22).Bold();
                    col.Item().PaddingTop(5).Row(row =>
                    {
                        row.AutoItem().Text($"Yazar: {author}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        row.AutoItem().PaddingLeft(20).Text($"Kategori: {category}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        row.AutoItem().PaddingLeft(20).Text($"Tarih: {createdAt:dd.MM.yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(15).Text(content)
                    .FontSize(11).LineHeight(1.5f);

                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Sayfa ").FontSize(9);
                        x.CurrentPageNumber().FontSize(9);
                        x.Span(" / ").FontSize(9);
                        x.TotalPages().FontSize(9);
                    });
            });
        });

        return document.GeneratePdf();
    }
}
