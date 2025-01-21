using Microsoft.AspNetCore.SignalR;

namespace WebAppBackend.Hubs
{
    public class RateLimiterHub:Hub
    {
        public async Task SendLog(string logMessage)
        {
            await Clients.All.SendAsync("ReceiveLog", logMessage);
        }
    }
}
