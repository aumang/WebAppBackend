using Microsoft.AspNetCore.SignalR;
using ProductPriceTracker.Models;
using WebAppBackend.DTO;

namespace WebAppBackend.Hubs
{
    public class StockMarketHub : Hub
    {
        public async Task BroadcastGoldPrice(StockValueDto stockValue)
        {
            await Clients.All.SendAsync("ReceiveStockPriceUpdate", stockValue);
        }
    }
}
