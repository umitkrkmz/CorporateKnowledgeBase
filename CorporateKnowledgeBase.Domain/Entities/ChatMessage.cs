namespace CorporateKnowledgeBase.Domain.Entities;

using CorporateKnowledgeBase.Domain.Common;
using CorporateKnowledgeBase.Domain.Enums;

/// <summary>
/// AI chat messaging - includes both user and assistant messages.
/// </summary>
public class ChatMessage : BaseEntity
{
    public int SessionId { get; set; }
    public ChatRole Role { get; set; }
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// If the RAG system referenced a document
    /// </summary>
    public int? ReferencedDocumentId { get; set; }
    public int? ReferencedBlogPostId { get; set; }

    /// <summary>
    /// Semantic similarity score (0-1)
    /// </summary>
    public float? MatchScore { get; set; }

    public ChatSession Session { get; set; } = null!;
    public TechnicalDocument? ReferencedDocument { get; set; }
    public BlogPost? ReferencedBlogPost { get; set; }
}
