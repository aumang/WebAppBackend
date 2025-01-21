using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAppBackend.Services;

namespace WebAppBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockMarketController : ControllerBase
    {
        private readonly StockMarketService _stockMarketService;

        public StockMarketController(StockMarketService stockMarketService)
        {
            _stockMarketService = stockMarketService;
        }
        [HttpGet("start")]
        public async Task<IActionResult> StartListening()
        {
            await _stockMarketService.StartTradeValue("wss://testnet.binance.vision/ws/btcusdt@trade");
            return Ok("WebSocket listener started.");
        }

        [HttpGet("stop")]
        public async Task<IActionResult> StopListening()
        {
            await _stockMarketService.StopTradeValue();
            return Ok("WebSocket listener stopped.");
        }
    }
}
