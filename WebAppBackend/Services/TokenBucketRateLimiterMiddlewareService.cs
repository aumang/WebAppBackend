using Microsoft.AspNetCore.SignalR;
using ProductPriceTracker.Hubs;
using ProductPriceTracker.Services;
using WebAppBackend.Hubs;

public class TokenBucketRateLimiterMiddlewareService
{
    private readonly RequestDelegate _next;
    private readonly int _maxTokens;
    private int _tokens;
    private DateTime _lastRefillTime;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Asynchronous locking mechanism
    private readonly ILogger<TokenBucketRateLimiterMiddlewareService> _logger;
    private readonly IHubContext<RateLimiterHub> _hubContext;

    public TokenBucketRateLimiterMiddlewareService(RequestDelegate next, int maxTokens, int refillRate, ILogger<TokenBucketRateLimiterMiddlewareService> logger, IHubContext<RateLimiterHub> hubContext)
    {
        _next = next;
        _maxTokens = maxTokens;
        _tokens = maxTokens;
        _lastRefillTime = DateTime.UtcNow;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task Invoke(HttpContext context)
    {
        await SendLogToFrontend("Your request has been received! Verifying if it meets the allowed rate limit...");
        await SendLogToFrontend("Rate Limit Rule: You can make up to 5 requests every 10 seconds. Please wait if you exceed this limit.");

        await _semaphore.WaitAsync(); // Asynchronous lock
        try
        {
            // Refill tokens if 1 second has passed
            var now = DateTime.UtcNow;
            var elapsedSeconds = (now - _lastRefillTime).TotalSeconds;

            if (elapsedSeconds >= 10)
            {
                var tokensToAdd = (int)(now - _lastRefillTime).TotalSeconds;
                _tokens = Math.Min(_tokens + tokensToAdd, _maxTokens);
                _lastRefillTime = now;
                await SendLogToFrontend($"Tokens refilled after {elapsedSeconds:F1} seconds. {tokensToAdd} token(s) added. Current tokens available: {_tokens}.");

            }

            // Check if tokens are available
            if (_tokens <= 0)
            {
                await SendLogToFrontend("Rate limit exceeded! No tokens available. Request rejected with status 429 (Too Many Requests).");
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "text/plain";
                context.Response.Headers.Add("Retry-After", "1");
                await context.Response.WriteAsync("Too many requests. Please try again later.");
                return;
            }

            _tokens--; // Consume a token
            await SendLogToFrontend($"Token consumed for the request. Remaining tokens: {_tokens}.");
        }
        finally
        {
            _semaphore.Release(); // Release the semaphore
        }
        // Send a summary log with variable data
        await SendSummaryToFrontend();

        await SendLogToFrontend("Request passed rate limiting check. Processing request...");
        await _next(context); // Continue with the next middleware
    }
    private async Task SendLogToFrontend(string message)
    {
        _logger.LogInformation(message);
        await _hubContext.Clients.All.SendAsync("ReceiveLog", message); // Send log to frontend
    }

    private async Task SendSummaryToFrontend()
    {
        string summaryMessage = $"Rate Limiting Summary:\n" +
                                $"- Max Tokens Allowed: {_maxTokens}\n" +
                                $"- Tokens Remaining: {_tokens}\n" +
                                $"- Last Refill Time: {_lastRefillTime:yyyy-MM-dd HH:mm:ss UTC}\n" +
                                $"- Refill Interval: Every 10 seconds";

        await SendLogToFrontend(summaryMessage);
    }
}
