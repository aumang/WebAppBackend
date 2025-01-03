using Microsoft.AspNetCore.SignalR;
using ProductPriceTracker.Models;
using ProductPriceTracker.Services;

namespace ProductPriceTracker.Hubs
{
    public class ProductPriceHub : Hub
    {
        public async Task BroadcastPriceUpdate(ProductPrice productPrice)
        {
            await Clients.All.SendAsync("ReceivePriceUpdate", productPrice);
        }

        public async Task BroadcastPriceHistory(PriceHistory priceHistory)
        {
            await Clients.Caller.SendAsync("ReceivePriceHistory", new PriceHistory
            {
                ProductId = priceHistory.ProductId,
                Price = priceHistory.Price,
                Location = priceHistory.Location,
                UpdatedAt = priceHistory.UpdatedAt
            });
        }

        // Notify when a new product is added (optional)
        public async Task NotifyNewProduct(ProductPrice productPrice)
        {
            await Clients.All.SendAsync("NewProductAdded", productPrice);
        }

        // Acknowledge client connection
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ConnectionAcknowledged", "You are now connected to the ProductHub!");
            await base.OnConnectedAsync();
        }

        // Handle client disconnection
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync("ClientDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
