using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PhienNTMVC.Pages.Hubs
{
    public class UserHub : Hub
    {
        public async Task NotifyUserDeleted(int userId, string userName)
        {
            // Broadcast to all connected clients that a user has been deleted
            await Clients.All.SendAsync("UserDeleted", userId, userName);
        }
    }
}