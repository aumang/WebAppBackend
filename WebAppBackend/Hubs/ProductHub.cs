using Microsoft.AspNetCore.SignalR;
using ProductPriceTracker.Models;
using ProductPriceTracker.Services;

namespace ProductPriceApp.Hubs
{
    public class ProductHub : Hub
    {
        private readonly ElasticsearchService _elasticsearchService;

        public ProductHub(ElasticsearchService elasticsearchService)
        {
            _elasticsearchService = elasticsearchService;
        }

        // Broadcast price updates to all connected clients
        public async Task BroadcastPriceUpdate(Product product)
        {
            Console.WriteLine($"Broadcasting price update: {product.Name} - {product.Price}");
            await Clients.All.SendAsync("ReceivePriceUpdate", product);
        }

        // Allow clients to fetch product details
        public async Task GetProductById(string id)
        {
            var product = await _elasticsearchService.GetProductByIdAsync(id);
            await Clients.Caller.SendAsync("ReceiveProductDetails", product);
        }

        public async Task BroadcastGoldPrice(GoldPrice goldPrice)
        {
            await Clients.All.SendAsync("ReceiveGoldPriceUpdate", goldPrice);
        }
    }
}
