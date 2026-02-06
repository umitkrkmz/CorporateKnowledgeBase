namespace CorporateKnowledgeBase.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public int? DepartmentId { get; set; }
    public string? JobTitle { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginDate { get; set; }

    // Notification preferences
    public bool NotifyOnNewComment { get; set; } = true;
    public bool NotifyOnNewBlogPost { get; set; } = true;
    public bool NotifyOnNewDocument { get; set; } = true;
    public bool NotifyOnNewAnnouncement { get; set; } = true;
}
