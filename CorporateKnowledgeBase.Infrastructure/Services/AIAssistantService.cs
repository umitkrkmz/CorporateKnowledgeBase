namespace CorporateKnowledgeBase.Infrastructure.Services;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using CorporateKnowledgeBase.Domain.Enums;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Numerics.Tensors;
using System.Text.Json;
using System.Text.RegularExpressions;

public class AIAssistantService(
    IApplicationDbContext context,
    IChatClient chatClient,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    : IAIAssistantService
{
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    private const float DOCUMENT_THRESHOLD = 0.50f;
    private const int MAX_CONTEXT_MESSAGES = 10;

    #region Embedding Operations

    public async Task UpdateDocumentEmbeddingAsync(int documentId)
    {
        var doc = await context.Documents
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc is null) return;

        var tagString = string.Join(" ", doc.Tags.Select(t => t.Name));
        var rawText = $"Title: {doc.Title}\nTags: {tagString}\nContent: {doc.Content}";
        var vectorJson = await GenerateEmbeddingJsonAsync(rawText);

        if (vectorJson is not null)
        {
            doc.EmbeddingVector = vectorJson;
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }

    public async Task UpdateBlogPostEmbeddingAsync(int blogPostId)
    {
        var blog = await context.BlogPosts
            .Include(b => b.Tags)
            .FirstOrDefaultAsync(b => b.Id == blogPostId);
        if (blog is null) return;

        var tagString = string.Join(" ", blog.Tags.Select(t => t.Name));
        var rawText = $"Title: {blog.Title}\nTags: {tagString}\nContent: {blog.Content}";
        var vectorJson = await GenerateEmbeddingJsonAsync(rawText);

        if (vectorJson is not null)
        {
            blog.EmbeddingVector = vectorJson;
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }

    #endregion

    #region Chat Operations

    /// <summary>
    /// Simple Q&A for backward compatibility.
    /// </summary>
    public async Task<string> AskSimpleAsync(string question)
    {
        var response = await AskAsync(question, "anonymous", null);
        return response.Content;
    }

    /// <summary>
    /// Fully featured chat - session, history, profile supported.
    /// </summary>
    public async Task<AIChatResponse> AskAsync(string question, string userId, int? sessionId = null)
    {
        var response = new AIChatResponse();

        try
        {
            // 1. Create or accept a session.
            ChatSession session;
            if (sessionId.HasValue)
            {
                session = await context.ChatSessions
                    .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
                    ?? await CreateSessionAsync(userId, "Yeni Sohbet");
            }
            else
            {
                session = await CreateSessionAsync(userId, "Yeni Sohbet");
            }
            response.SessionId = session.Id;

            // If the session title is "New Chat", update from the first message.
            if (session.Title == "Yeni Sohbet")
            {
                session.Title = ExtractTitle(question);
            }

            // 2. Save user message
            var userMessage = new Domain.Entities.ChatMessage
            {
                SessionId = session.Id,
                Role = Domain.Enums.ChatRole.User,
                Content = question,
                CreatedAt = DateTime.UtcNow
            };
            context.ChatMessages.Add(userMessage);
            await context.SaveChangesAsync(CancellationToken.None);

            // 3. Intent detection: Document or general chat?
            var (isDocumentQuery, bestMatch) = await DetectIntentAndSearchAsync(question);

            // 4. Get user profile
            var profile = await GetOrCreateProfileAsync(userId);
            var systemPrompt = BuildSystemPrompt(profile, isDocumentQuery);

            // 5. Get chat history (last N messages)
            var history = await GetChatHistoryAsync(session.Id);

            // 6. LLM call
            var messages = new List<Microsoft.Extensions.AI.ChatMessage>();
            messages.Add(new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.System, systemPrompt));

            // Add past messages
            foreach (var msg in history)
            {
                var role = msg.Role == Domain.Enums.ChatRole.User
                    ? Microsoft.Extensions.AI.ChatRole.User
                    : Microsoft.Extensions.AI.ChatRole.Assistant;
                messages.Add(new Microsoft.Extensions.AI.ChatMessage(role, msg.Content));
            }

            // Add document content if available.
            string userPrompt;
            if (isDocumentQuery && bestMatch != null)
            {
                userPrompt = $"CONTENTS ({bestMatch.Type}): {bestMatch.Title}\n{bestMatch.Content}\n\nQUESTION: {question}";
                response.IsDocumentBased = true;
                response.ReferencedTitle = bestMatch.Title;
                response.MatchScore = bestMatch.Score;

                if (bestMatch.Type == "Document")
                    response.ReferencedDocumentId = bestMatch.Id;
                else
                    response.ReferencedBlogPostId = bestMatch.Id;
            }
            else
            {
                userPrompt = question;
                response.IsDocumentBased = false;
            }

            messages.Add(new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.User, userPrompt));

            var llmResponse = await chatClient.GetResponseAsync(messages);
            var markdownAnswer = llmResponse.Messages.FirstOrDefault()?.Text ?? llmResponse.ToString();

            // 7. convert to HTML
            var htmlAnswer = Markdown.ToHtml(markdownAnswer ?? "", Pipeline);

            // Add source link
            if (response.IsDocumentBased && bestMatch != null)
            {
                var controllerName = bestMatch.Type == "Document" ? "Documents" : "BlogPosts";
                htmlAnswer += $"<div class='mt-3 pt-2 border-top'><small><strong>Source:</strong> " +
                    $"<a href='/{controllerName}/Details/{bestMatch.Id}' class='text-primary'>{bestMatch.Title}</a> " +
                    $"<span class='text-muted'>({bestMatch.Score * 100:0}% match)</span></small></div>";
            }

            response.Content = htmlAnswer;

            // 8. Save the assistant's message.
            var assistantMessage = new Domain.Entities.ChatMessage
            {
                SessionId = session.Id,
                Role = Domain.Enums.ChatRole.Assistant,
                Content = htmlAnswer,
                ReferencedDocumentId = response.ReferencedDocumentId,
                ReferencedBlogPostId = response.ReferencedBlogPostId,
                MatchScore = response.MatchScore,
                CreatedAt = DateTime.UtcNow
            };
            context.ChatMessages.Add(assistantMessage);
            await context.SaveChangesAsync(CancellationToken.None);
            response.MessageId = assistantMessage.Id;

            // 9. Update session
            session.LastMessageAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(CancellationToken.None);

            // 10. Profile update (async, silently continues in case of error)
            try
            {
                var matchedCategory = bestMatch != null
                    ? await GetCategoryNameAsync(bestMatch.Type, bestMatch.Id)
                    : null;
                await UpdateProfileFromQuestionAsync(userId, question, matchedCategory);
            }
            catch { /* If the profile update fails, proceed silently. */ }

            return response;
        }
        catch (Exception ex)
        {
            response.Error = $"AI Assistant error: {ex.Message}";
            response.Content = $"<p class='text-danger'>{response.Error}</p>";
            return response;
        }
    }

    private async Task<(bool isDocumentQuery, ContentMatch? bestMatch)> DetectIntentAndSearchAsync(string question)
    {
        try
        {
            // Convert the question to a vector.
            var questionEmbedding = await embeddingGenerator.GenerateAsync([question]);
            var questionVector = questionEmbedding[0].Vector.ToArray();

            // Get all embedded content
            var docs = await context.Documents.AsNoTracking()
                .Where(d => d.EmbeddingVector != null)
                .Select(d => new { d.Id, d.Title, d.Content, d.EmbeddingVector, d.CategoryId, Type = "Document" })
                .ToListAsync();

            var blogs = await context.BlogPosts.AsNoTracking()
                .Where(b => b.EmbeddingVector != null)
                .Select(b => new { b.Id, b.Title, b.Content, b.EmbeddingVector, b.CategoryId, Type = "Blog" })
                .ToListAsync();

            var allContent = docs.Select(d => new { d.Id, d.Title, d.Content, d.EmbeddingVector, d.CategoryId, d.Type })
                .Concat(blogs.Select(b => new { b.Id, b.Title, b.Content, b.EmbeddingVector, b.CategoryId, b.Type }))
                .ToList();

            if (!allContent.Any())
                return (false, null);

            // Keyword boosting
            var searchTerms = question.ToLower()
                .Split([' ', '?', '.', ','], StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3)
                .ToList();

            // Calculate the scores.
            var candidates = allContent.Select(item =>
            {
                var vector = JsonSerializer.Deserialize<float[]>(item.EmbeddingVector!);
                float similarity = TensorPrimitives.CosineSimilarity(questionVector, vector!);

                float boost = 0;
                foreach (var term in searchTerms)
                {
                    if (item.Title.ToLower().Contains(term))
                        boost += 0.15f;
                }

                return new ContentMatch
                {
                    Id = item.Id,
                    Title = item.Title,
                    Content = item.Content,
                    CategoryId = item.CategoryId,
                    Type = item.Type,
                    Score = similarity + boost
                };
            })
            .OrderByDescending(x => x.Score)
            .Take(3)
            .ToList();

            var best = candidates.FirstOrDefault();

            // Threshold control
            if (best == null || best.Score < DOCUMENT_THRESHOLD)
                return (false, null);

            return (true, best);
        }
        catch
        {
            return (false, null);
        }
    }

    private string BuildSystemPrompt(UserAIProfile profile, bool isDocumentQuery)
    {
        var basePrompt = "You are a software team assistant. ";

        // Language preference
        basePrompt += profile.PreferredLanguage == "en"
            ? "Respond in English. "
            : "Cevaplarını Türkçe ver. ";

        // Answer style
        basePrompt += profile.ResponseStyle switch
        {
            "concise" => "Give short and concise answers. ",
            "code-heavy" => "Explain with code examples whenever possible. ",
            _ => "Provide detailed and explanatory answers. "
        };

        // Is it document-based?
        if (isDocumentQuery)
        {
            basePrompt += "Answer the question based on the given information. ";
        }
        else
        {
            basePrompt += "Answer the question using your general knowledge. If there is no relevant document in the knowledge base, help with your own information. ";
        }

        // interests
        if (!string.IsNullOrEmpty(profile.Interests))
        {
            try
            {
                var interests = JsonSerializer.Deserialize<List<string>>(profile.Interests);
                if (interests?.Any() == true)
                {
                    basePrompt += $"User interests: {string.Join(", ", interests)}. ";
                }
            }
            catch { }
        }

        basePrompt += "If there are code examples, write them in Markdown format. (```csharp like).";

        return basePrompt;
    }

    private async Task<List<Domain.Entities.ChatMessage>> GetChatHistoryAsync(int sessionId)
    {
        return await context.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(MAX_CONTEXT_MESSAGES)
            .OrderBy(m => m.CreatedAt) // Sort back in chronological order.
            .ToListAsync();
    }

    private async Task<string?> GetCategoryNameAsync(string type, int contentId)
    {
        if (type == "Document")
        {
            var doc = await context.Documents
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == contentId);
            return doc?.Category?.Name;
        }
        else
        {
            var blog = await context.BlogPosts
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == contentId);
            return blog?.Category?.Name;
        }
    }

    #endregion

    #region Session Management

    public async Task<ChatSession> CreateSessionAsync(string userId, string? title = null)
    {
        var session = new ChatSession
        {
            UserId = userId,
            Title = title ?? "Yeni Sohbet",
            CreatedAt = DateTime.UtcNow
        };

        context.ChatSessions.Add(session);
        await context.SaveChangesAsync(CancellationToken.None);

        return session;
    }

    public async Task<List<ChatSession>> GetUserSessionsAsync(string userId, bool includeArchived = false)
    {
        var query = context.ChatSessions
            .Where(s => s.UserId == userId);

        if (!includeArchived)
            query = query.Where(s => !s.IsArchived);

        return await query
            .OrderByDescending(s => s.LastMessageAt ?? s.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatSession?> GetSessionWithMessagesAsync(int sessionId, string userId)
    {
        return await context.ChatSessions
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
    }

    public async Task ArchiveSessionAsync(int sessionId, string userId)
    {
        var session = await context.ChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session != null)
        {
            session.IsArchived = true;
            session.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }

    public async Task DeleteSessionAsync(int sessionId, string userId)
    {
        var session = await context.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session != null)
        {
            context.ChatSessions.Remove(session); // Cascade delete also deletes messages.
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }

    #endregion

    #region User Profile

    public async Task<UserAIProfile> GetOrCreateProfileAsync(string userId)
    {
        var profile = await context.UserAIProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new UserAIProfile
            {
                UserId = userId,
                ResponseStyle = "detailed",
                PreferredLanguage = "tr",
                CreatedAt = DateTime.UtcNow
            };
            context.UserAIProfiles.Add(profile);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        return profile;
    }

    public async Task UpdateProfileFromQuestionAsync(string userId, string question, string? matchedCategory)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        // Increase the number of questions
        profile.TotalQuestions++;

        // Update category frequency
        if (!string.IsNullOrEmpty(matchedCategory))
        {
            var frequency = new Dictionary<string, int>();
            if (!string.IsNullOrEmpty(profile.TopicFrequency))
            {
                try { frequency = JsonSerializer.Deserialize<Dictionary<string, int>>(profile.TopicFrequency) ?? new(); }
                catch { }
            }

            frequency[matchedCategory] = frequency.GetValueOrDefault(matchedCategory, 0) + 1;
            profile.TopicFrequency = JsonSerializer.Serialize(frequency);

            // Update your interests (top 5 categories)
            var topInterests = frequency
                .OrderByDescending(kv => kv.Value)
                .Take(5)
                .Select(kv => kv.Key)
                .ToList();
            profile.Interests = JsonSerializer.Serialize(topInterests);
        }

        profile.LastAnalyzedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(CancellationToken.None);
    }

    #endregion

    #region Private Helpers

    private async Task<string?> GenerateEmbeddingJsonAsync(string rawText)
    {
        try
        {
            var cleanText = RemoveMarkdown(rawText);
            if (cleanText.Length > 1000)
            {
                var lastSpace = cleanText.LastIndexOf(' ', 1000);
                cleanText = lastSpace > 0 ? cleanText[..lastSpace] : cleanText[..1000];
            }

            var result = await embeddingGenerator.GenerateAsync([cleanText]);
            return JsonSerializer.Serialize(result[0].Vector.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Embedding hatası: {ex.Message}");
            return null;
        }
    }

    private static string RemoveMarkdown(string markdownText)
    {
        if (string.IsNullOrEmpty(markdownText)) return "";
        var text = Regex.Replace(markdownText, @"```[\s\S]*?```", "", RegexOptions.Multiline);
        text = Regex.Replace(text, @"`[^`]*`", "");
        text = Regex.Replace(text, @"!\[(.*?)\]\(.*?\)", "$1");
        text = Regex.Replace(text, @"\[(.*?)\]\(.*?\)", "$1");
        text = Regex.Replace(text, @"^#+\s+", "", RegexOptions.Multiline);
        text = Regex.Replace(text, @"(\*\*|__)(.*?)\1", "$2");
        text = Regex.Replace(text, @"(\*|_)(.*?)\1", "$2");
        text = Regex.Replace(text, @"^>\s+", "", RegexOptions.Multiline);
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text;
    }

    private static string ExtractTitle(string question)
    {
        // Use the first 50 characters as the title.
        var title = question.Length > 50 ? question[..47] + "..." : question;
        return title.Replace('\n', ' ').Trim();
    }

    #endregion

    private class ContentMatch
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string Type { get; set; } = string.Empty;
        public float Score { get; set; }
    }
}
