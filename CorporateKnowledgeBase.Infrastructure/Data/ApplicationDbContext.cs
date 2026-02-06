namespace CorporateKnowledgeBase.Infrastructure.Data;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using CorporateKnowledgeBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IApplicationDbContext
{
    public DbSet<TechnicalDocument> Documents => Set<TechnicalDocument>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<UserLike> UserLikes => Set<UserLike>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ContentReport> ContentReports => Set<ContentReport>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();
    public DbSet<Department> Departments => Set<Department>();

    // AI Chat
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<UserAIProfile> UserAIProfiles => Set<UserAIProfile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ChatSession - ChatMessage relationship
        builder.Entity<ChatMessage>()
            .HasOne(m => m.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ChatMessage - Document/BlogPost optional relationships (NoAction: SQL Server cascade path issue is prevented)
        builder.Entity<ChatMessage>()
            .HasOne(m => m.ReferencedDocument)
            .WithMany()
            .HasForeignKey(m => m.ReferencedDocumentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ChatMessage>()
            .HasOne(m => m.ReferencedBlogPost)
            .WithMany()
            .HasForeignKey(m => m.ReferencedBlogPostId)
            .OnDelete(DeleteBehavior.NoAction);

        // UserAIProfile - unique UserId index
        builder.Entity<UserAIProfile>()
            .HasIndex(p => p.UserId)
            .IsUnique();
    }
}
