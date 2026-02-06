namespace CorporateKnowledgeBase.Domain.Entities;

using CorporateKnowledgeBase.Domain.Common;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<TechnicalDocument> Documents { get; set; } = [];
    public ICollection<BlogPost> BlogPosts { get; set; } = [];
}
