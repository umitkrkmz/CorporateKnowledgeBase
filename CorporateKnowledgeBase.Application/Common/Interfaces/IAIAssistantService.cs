namespace CorporateKnowledgeBase.Application.Common.Interfaces;

using CorporateKnowledgeBase.Domain.Entities;

public interface IAIAssistantService
{
    // Embedding Operations
    Task UpdateDocumentEmbeddingAsync(int documentId);
    Task UpdateBlogPostEmbeddingAsync(int blogPostId);

    // Chat Operations
    Task<AIChatResponse> AskAsync(string question, string userId, int? sessionId = null);
    Task<string> AskSimpleAsync(string question); // Backward compatibility

    // Session Management
    Task<ChatSession> CreateSessionAsync(string userId, string? title = null);
    Task<List<ChatSession>> GetUserSessionsAsync(string userId, bool includeArchived = false);
    Task<ChatSession?> GetSessionWithMessagesAsync(int sessionId, string userId);
    Task ArchiveSessionAsync(int sessionId, string userId);
    Task DeleteSessionAsync(int sessionId, string userId);

    // User Profile
    Task<UserAIProfile> GetOrCreateProfileAsync(string userId);
    Task UpdateProfileFromQuestionAsync(string userId, string question, string? matchedCategory);
}

/// <summary>
/// AI response result - includes both the answer and metadata.
/// </summary>
public class AIChatResponse
{
    public string Content { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public int MessageId { get; set; }
    public bool IsDocumentBased { get; set; }
    public int? ReferencedDocumentId { get; set; }
    public int? ReferencedBlogPostId { get; set; }
    public string? ReferencedTitle { get; set; }
    public float? MatchScore { get; set; }
    public string? Error { get; set; }
}
