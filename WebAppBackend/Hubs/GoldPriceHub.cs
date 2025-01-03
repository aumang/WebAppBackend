using Microsoft.AspNetCore.SignalR;
using ProductPriceTracker.Models;
using ProductPriceTracker.Services;

namespace ProductPriceTracker.Hubs
{
    public class GoldPriceHub : Hub
    {
        private readonly ElasticsearchService _elasticsearchService;

        public GoldPriceHub(ElasticsearchService elasticsearchService)
        {
            _elasticsearchService = elasticsearchService;
        }

        public async Task BroadcastGoldPrice(GoldPrice goldPrice)
        {
            await Clients.All.SendAsync("ReceiveGoldPriceUpdate", goldPrice);
        }
    }
}
