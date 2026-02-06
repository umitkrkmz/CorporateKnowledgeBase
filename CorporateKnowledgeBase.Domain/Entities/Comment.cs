namespace CorporateKnowledgeBase.Domain.Entities;

using CorporateKnowledgeBase.Domain.Common;
using CorporateKnowledgeBase.Domain.Enums;

public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public int? DocumentId { get; set; }
    public int? BlogPostId { get; set; }
    public ContentType ContentType { get; set; }
    public int? ParentCommentId { get; set; }

    public TechnicalDocument? Document { get; set; }
    public BlogPost? BlogPost { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
