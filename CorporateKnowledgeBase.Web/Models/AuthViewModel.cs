namespace CorporateKnowledgeBase.Web.Models;

using CorporateKnowledgeBase.Application.Features.Auth;

public class AuthViewModel
{
    public LoginCommand Login { get; set; } = new("", "", false);
    public RegisterCommand Register { get; set; } = new("", "", "", null);
    public bool ShowRegister { get; set; }
}
