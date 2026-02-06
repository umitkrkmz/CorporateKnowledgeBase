namespace CorporateKnowledgeBase.Application.Common.Interfaces;

public interface IPdfExportService
{
    byte[] GenerateDocumentPdf(string title, string content, string author, string category, DateTime createdAt);
}
