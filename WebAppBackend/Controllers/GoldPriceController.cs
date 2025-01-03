using GoldPriceTracker.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ProductPriceApp.Hubs;
using ProductPriceTracker.Hubs;
using ProductPriceTracker.Models;
using ProductPriceTracker.Services;

namespace ProductPriceTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoldPriceController : ControllerBase
    {
        private readonly GoldPricesService _goldPricesService;
        private readonly IHubContext<GoldPriceHub> _hubContext;

        public GoldPriceController(IHubContext<GoldPriceHub> hubContext, GoldPricesService goldPricesService)
        {
            _hubContext = hubContext;
            _goldPricesService = goldPricesService;
        }

        [HttpPost("update-gold-price")]
        public async Task<IActionResult> UpdateGoldPrice([FromBody] GoldPrice goldPrice)
        {
            goldPrice.Timestamp = DateTime.UtcNow;
            await _goldPricesService.AddOrUpdateGoldPriceAsync(goldPrice, "gold");
            // Broadcast to SignalR clients
            await _hubContext.Clients.All.SendAsync("ReceiveGoldPriceUpdate", goldPrice);
            return Ok("Gold price updated successfully!");
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetGoldPriceHistory()
        {
            var goldPrices = await _goldPricesService.GetAllGoldPricesAsync("gold");
            return Ok(goldPrices.OrderBy(g => g.Timestamp));
        }

        [HttpGet("latest-gold-price")]
        public async Task<IActionResult> GetLatestGoldPrice()
        {
            var latestGoldPrice = await _goldPricesService.GetLatestGoldPriceAsync("gold");
            return Ok(latestGoldPrice);
        }
    }
}
