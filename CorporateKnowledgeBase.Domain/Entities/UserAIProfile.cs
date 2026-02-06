namespace CorporateKnowledgeBase.Domain.Entities;

using CorporateKnowledgeBase.Domain.Common;

/// <summary>
/// User's interaction profile with the AI ​​assistant
/// Interests, preferences, and frequency of topics
/// </summary>
public class UserAIProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Automatically extracted fields of interest (JSON array: ["backend", "docker", "ef-core"])
    /// </summary>
    public string? Interests { get; set; }

    /// <summary>
    /// Frequently asked question categories and their frequencies (JSON: {"Backend": 15, "DevOps": 8})
    /// </summary>
    public string? TopicFrequency { get; set; }

    /// <summary>
    /// Preferred answer style: "detailed", "concise", "code-heavy"
    /// </summary>
    public string ResponseStyle { get; set; } = "detailed";

    /// <summary>
    /// Preferred languages: "tr", "en"
    /// </summary>
    public string PreferredLanguage { get; set; } = "tr";

    /// <summary>
    /// Total number of questions
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Last update time (profile analysis)
    /// </summary>
    public DateTime? LastAnalyzedAt { get; set; }
}
