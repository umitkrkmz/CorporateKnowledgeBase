namespace CorporateKnowledgeBase.Domain.Entities;

using CorporateKnowledgeBase.Domain.Common;

public class FaqItem : BaseEntity
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}
