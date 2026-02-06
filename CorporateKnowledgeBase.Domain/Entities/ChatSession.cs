namespace CorporateKnowledgeBase.Domain.Entities;

using CorporateKnowledgeBase.Domain.Common;

/// <summary>
/// AI chat session - multiple sessions per user
/// </summary>
public class ChatSession : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = "Yeni Sohbet";
    public DateTime? LastMessageAt { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
