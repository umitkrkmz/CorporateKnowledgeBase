namespace CorporateKnowledgeBase.Domain.Entities;

using CorporateKnowledgeBase.Domain.Common;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<TechnicalDocument> Documents { get; set; } = [];
    public ICollection<BlogPost> BlogPosts { get; set; } = [];
}
