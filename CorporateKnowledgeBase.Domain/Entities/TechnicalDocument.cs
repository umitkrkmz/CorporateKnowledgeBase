namespace CorporateKnowledgeBase.Domain.Entities;

using CorporateKnowledgeBase.Domain.Common;
using CorporateKnowledgeBase.Domain.Enums;
using System.Xml.Linq;

public class TechnicalDocument : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int ViewCount { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public string? RejectionReason { get; set; }
    public int? DepartmentId { get; set; }
    public string? EmbeddingVector { get; set; }

    public Category Category { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<UserLike> Likes { get; set; } = [];
    public ICollection<DocumentVersion> Versions { get; set; } = [];
}
