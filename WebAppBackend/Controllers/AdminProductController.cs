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
    public class AdminProductController : ControllerBase
    {
        private readonly ElasticSearchProductService _elasticsearchProductService;
        private readonly IHubContext<ProductPriceHub> _hubContext;
        private readonly IHubContext<CircuitBreakerHub> _circuitBreakerHubContext;
        private readonly ILogger<AdminProductController> _logger;
        private readonly ILogger<CustomCircuitBreaker> _customCircuitBreakerLogger;
        private readonly CustomCircuitBreaker _customCircuitBreaker;


        public AdminProductController(ElasticSearchProductService elasticsearchProductService, IHubContext<ProductPriceHub> hubContext, IHubContext<CircuitBreakerHub> circuitBreakerHubContext, ILogger<AdminProductController> logger, ILogger<CustomCircuitBreaker> customCircuitBreakerLogger, CustomCircuitBreaker customCircuitBreaker)
        {
            _elasticsearchProductService = elasticsearchProductService;
            _hubContext = hubContext;
            _circuitBreakerHubContext = circuitBreakerHubContext;
            _logger = logger;
            _customCircuitBreakerLogger = customCircuitBreakerLogger;
            _customCircuitBreaker = customCircuitBreaker;
        }

        [HttpPost("update-price")]
        public async Task<IActionResult> UpdatePrice([FromBody] ProductPrice productPrice)
        {
            productPrice.LastUpdated = DateTime.UtcNow;

            var isUpdated = await _elasticsearchProductService.AddOrUpdateProductAsync(productPrice);
            if (isUpdated)
            {
                await _hubContext.Clients.All.SendAsync("ReceivePriceUpdate", productPrice);

                var history = new PriceHistory
                {
                    ProductId = productPrice.ProductId,
                    Location = productPrice.Location,
                    Price = productPrice.Price,
                    UpdatedAt = DateTime.UtcNow
                };
                await _elasticsearchProductService.SavePriceHistoryAsync(history);

                return Ok("Price updated successfully!");
            }
            return StatusCode(500, "Failed to update price.");
        }

        [HttpGet("get-price")]
        public async Task<IActionResult> GetPrice(string location)
        {
            var product = await _elasticsearchProductService.GetProductByLocationAsync(location);
            return product != null ? Ok(product) : NotFound("Product not found.");
        }

        [HttpGet("price-history")]
        public async Task<IActionResult> GetPriceHistory(string location)
        {
            var history = await _elasticsearchProductService.GetPriceHistoryAsync(location);
            return Ok(history);
        }

        //[HttpGet("GetProduct")]
        //public async Task<IActionResult> GetProduct()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Product request started.");

        //        // Execute the action with the CircuitBreaker in place
        //        var result = await _customCircuitBreaker.ExecuteAsync(async () =>
        //        {
        //            // Simulating a success or failure
        //            Random random = new Random();
        //            if (random.Next(0, 2) == 0)
        //            {
        //                _logger.LogInformation("Simulated successful request.");
        //                return "Product details fetched successfully.";
        //            }
        //            else
        //            {
        //                _logger.LogError("Simulated failure occurred.");
        //                throw new Exception("Simulated failure");
        //            }
        //        });

        //        // Notify frontend about the circuit state
        //        await _hubContext.Clients.All.SendAsync("ReceiveCircuitState", _customCircuitBreaker.GetState());

        //        return Ok(new { Message = result, CircuitState = _customCircuitBreaker.GetState() });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to execute product request.");
        //        await _hubContext.Clients.All.SendAsync("ReceiveCircuitState", _customCircuitBreaker.GetState());
        //        return BadRequest(new { Error = ex.Message, CircuitState = _customCircuitBreaker.GetState() });
        //    }
        [HttpGet("GetProduct")]
        public async Task<IActionResult> GetProduct()
        {
            try
            {
                _logger.LogInformation("Product request started.");

                // Execute the action with the CircuitBreaker in place
                var result = await _customCircuitBreaker.ExecuteAsync(async () =>
                {
                    // Simulating a success or failure
                    Random random = new Random();
                    if (random.Next(0, 2) == 0)
                    {
                        _logger.LogInformation("Simulated successful request.");
                        return "Product details fetched successfully.";
                    }
                    else
                    {
                        _logger.LogError("Simulated failure occurred.");
                        throw new Exception("Simulated failure");
                    }
                });

                // Notify frontend about the circuit state
                await _hubContext.Clients.All.SendAsync("ReceiveLog", $"Circuit State: {_customCircuitBreaker.GetState()}");

                return Ok(new { Message = result, CircuitState = _customCircuitBreaker.GetState() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute product request.");
                await _hubContext.Clients.All.SendAsync("ReceiveLog", $"Circuit State: {_customCircuitBreaker.GetState()}");
                return BadRequest(new { Error = ex.Message, CircuitState = _customCircuitBreaker.GetState() });
            }
        }
    }
}
