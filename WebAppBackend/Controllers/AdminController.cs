using Microsoft.AspNetCore.Mvc;
using ProductPriceApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using ProductPriceTracker.Models;
using ProductPriceTracker.Services;
using GoldPriceTracker.Services;

namespace ProductPriceApp.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ElasticsearchService _elasticsearchService;
        private readonly GoldPriceService _goldPriceService;
        private readonly IHubContext<ProductHub> _hubContext;

        public AdminController(ElasticsearchService elasticsearchService, IHubContext<ProductHub> hubContext, GoldPriceService goldPriceService)
        {
            _elasticsearchService = elasticsearchService;
            _hubContext = hubContext;
            _goldPriceService = goldPriceService;
        }

        [HttpPost("update-price")]
        public async Task<IActionResult> UpdatePrice([FromBody] Product product)
        {
            product.LastUpdated = DateTime.UtcNow;

            var isUpdated = await _elasticsearchService.AddOrUpdateProductAsync(product);
            if (isUpdated)
            {
                // Notify all clients about the price update
                Console.WriteLine($"Broadcasting price update: {product.Name} - {product.Price}");
                await _hubContext.Clients.All.SendAsync("ReceivePriceUpdate", product);
                return Ok("Price updated successfully!");
            }

            return StatusCode(500, "Failed to update price.");
        }

        [HttpGet("get-product")]
        public async Task<IActionResult> GetProduct(string productId)
        {
            var product = await _elasticsearchService.GetProductByIdAsync(productId);
            if (product != null)
            {
                return Ok(product);
            }
            return StatusCode(500, "Product not found.");
        }

        [HttpPost("update-gold-price")]
        public async Task<IActionResult> UpdateGoldPrice([FromBody] GoldPrice goldPrice)
        {
            goldPrice.Timestamp = DateTime.UtcNow;
            await _goldPriceService.FetchAndBroadcastGoldPrice();
            return Ok("Gold price updated successfully!");
        }

        [HttpGet("gold-prices/history")]
        public async Task<IActionResult> GetGoldPriceHistory()
        {
            var goldPrices = await _elasticsearchService.GetAllGoldPricesAsync("gold");
            return Ok(goldPrices.OrderBy(g => g.Timestamp));
        }

        // Endpoint to fetch the gold price
        [HttpGet("latest")]
        public IActionResult GetGoldPrice()
        {
            var price = _goldPriceService.GetRandomGoldPrice();
            var response = new
            {
                rates = new
                {
                    XAU = price
                },
                baseCurrency = "USD",
                price = price,
                date = DateTime.UtcNow.ToString("yyyy-MM-dd")
            };

            return Ok(response);
        }
    }
}
