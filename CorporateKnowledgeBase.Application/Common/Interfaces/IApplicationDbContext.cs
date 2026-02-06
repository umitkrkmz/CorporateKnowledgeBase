namespace CorporateKnowledgeBase.Application.Common.Interfaces;

using CorporateKnowledgeBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IApplicationDbContext
{
    DbSet<TechnicalDocument> Documents { get; }
    DbSet<BlogPost> BlogPosts { get; }
    DbSet<Announcement> Announcements { get; }
    DbSet<Category> Categories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Comment> Comments { get; }
    DbSet<UserLike> UserLikes { get; }
    DbSet<DocumentVersion> DocumentVersions { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<ContentReport> ContentReports { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<FaqItem> FaqItems { get; }
    DbSet<Department> Departments { get; }

    // AI Chat
    DbSet<ChatSession> ChatSessions { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<UserAIProfile> UserAIProfiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
