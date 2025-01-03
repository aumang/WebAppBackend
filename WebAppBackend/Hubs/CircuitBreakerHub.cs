using Microsoft.AspNetCore.SignalR;

namespace ProductPriceTracker.Hubs
{
    public class CircuitBreakerHub:Hub
    {
        public async Task SendCircuitState(string state)
        {
            await Clients.All.SendAsync("ReceiveCircuitState",state);
        }
        public async Task SendLog(string logMessage)
        {
            await Clients.All.SendAsync("ReceiveLog", logMessage);
        }
    }
}
