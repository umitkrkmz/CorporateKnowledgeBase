namespace CorporateKnowledgeBase.Infrastructure.Hubs;

using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = Context.UserIdentifier;
            if (userId is not null)
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }
}
