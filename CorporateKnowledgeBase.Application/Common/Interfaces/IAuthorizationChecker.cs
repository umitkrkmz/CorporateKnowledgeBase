namespace CorporateKnowledgeBase.Application.Common.Interfaces;

public interface IAuthorizationChecker
{
    /// <summary>Author + Editor + Admin can edit</summary>
    Task<bool> CanEditContentAsync(string userId, string authorId);

    /// <summary>Author + Admin can delete</summary>
    Task<bool> CanDeleteContentAsync(string userId, string authorId);

    /// <summary>Check if user is approved</summary>
    Task<bool> IsApprovedAsync(string userId);
}
