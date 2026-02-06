namespace CorporateKnowledgeBase.Domain.Entities;

using CorporateKnowledgeBase.Domain.Enums;

public class ContentReport
{
    public int Id { get; set; }
    public string ReporterId { get; set; } = string.Empty;
    public int ContentId { get; set; }
    public ContentType ContentType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ReportStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
