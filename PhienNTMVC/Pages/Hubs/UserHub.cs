using Microsoft.AspNetCore.SignalR;

namespace PhienNTMVC.Pages.Hubs
{
    public class UserHub : Hub
    {
        public async Task SendUserDeleted(int accountId, string accountName)
        {
            await Clients.All.SendAsync("ReceiveUserDeleted", accountId, accountName);
        }
    }
}
