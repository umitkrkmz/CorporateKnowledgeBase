namespace CorporateKnowledgeBase.Domain.Entities;

using CorporateKnowledgeBase.Domain.Enums;

public class UserLike
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? DocumentId { get; set; }
    public int? BlogPostId { get; set; }
    public ContentType ContentType { get; set; }
    public DateTime CreatedAt { get; set; }

    public TechnicalDocument? Document { get; set; }
    public BlogPost? BlogPost { get; set; }
}
