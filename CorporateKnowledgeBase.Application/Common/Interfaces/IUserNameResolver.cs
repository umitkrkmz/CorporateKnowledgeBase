namespace CorporateKnowledgeBase.Application.Common.Interfaces;

public interface IUserNameResolver
{
    Task<string> GetFullNameAsync(string userId);
    Task<string?> GetProfileImagePathAsync(string userId);
}
