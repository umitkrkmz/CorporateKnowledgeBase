namespace CorporateKnowledgeBase.Domain.Entities;

public class DocumentVersion
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public TechnicalDocument Document { get; set; } = null!;
}
