using Microsoft.AspNetCore.SignalR;
using ProductPriceTracker.Models;

namespace ProductPriceTracker.Hubs
{
    public class PriceHub : Hub
    {
        public async Task SendPriceUpdate(Product productDetails)
        {
            //await Clients.All.SendAsync("ReceivePriceUpdate", productDetails.productId, productDetails.price);
        }
    }
}
