using Microsoft.AspNetCore.SignalR;
using ProductPriceApp.Hubs;
using ProductPriceTracker.Models;
using ProductPriceTracker.Services;
using System;

namespace GoldPriceTracker.Services
{
    public class GoldPriceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ElasticsearchService _elasticsearchService;
        private readonly IHubContext<ProductHub> _hubContext;

        public GoldPriceService(IHttpClientFactory httpClientFactory, ElasticsearchService elasticsearchService, IHubContext<ProductHub> hubContext)
        {
            _httpClientFactory = httpClientFactory;
            _elasticsearchService = elasticsearchService;
            _hubContext = hubContext;
        }

        public async Task FetchAndBroadcastGoldPrice()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync("http://localhost:5156/api/admin/latest");  // Replace with actual API
            if (response.IsSuccessStatusCode)
            {
                var goldPrice = await response.Content.ReadFromJsonAsync<GoldPrice>();
                goldPrice.Timestamp = DateTime.UtcNow;

                // Save to Elasticsearch
                await _elasticsearchService.AddOrUpdateGoldPriceAsync(goldPrice, "gold");

                // Broadcast to SignalR clients
                await _hubContext.Clients.All.SendAsync("ReceiveGoldPriceUpdate", goldPrice);
            }
        }

        public decimal GetRandomGoldPrice()
        {
            Random _random = new Random();
            // Simulate realistic gold price fluctuation (e.g., $1700 - $2000)
            decimal minPrice = 1700;
            decimal maxPrice = 2000;
            decimal randomPrice = (decimal)(_random.NextDouble() * (double)(maxPrice - minPrice) + (double)minPrice);
            return Math.Round(randomPrice, 2);  // Round to 2 decimal places
        }
    }
}
