using Microsoft.AspNetCore.SignalR;
using ProductPriceTracker.Hubs;
using ProductPriceTracker.Models;
namespace ProductPriceTracker.Services
{
    public class CustomCircuitBreaker
    {
        private readonly Guid _instanceId = Guid.NewGuid();
        private State _state = State.Closed;
        private int _failureCount = 0;
        private readonly int _threshold;
        private readonly TimeSpan _timeout;
        private DateTime _lastFailureTime;
        private readonly ILogger<CustomCircuitBreaker> _logger;
        private readonly IHubContext<CircuitBreakerHub> _hubContext;

        public CustomCircuitBreaker(int threshold, TimeSpan timeout, ILogger<CustomCircuitBreaker> logger, IHubContext<CircuitBreakerHub> hubContext)
        {
            _threshold = threshold;
            _timeout = timeout;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            _logger.LogInformation("Attempting to execute action.");
            await SendLogToFrontend($"Attempting to execute action. Instance Id: {this._instanceId}");
            if (_state == State.Open)
            {
                if(DateTime.UtcNow - _lastFailureTime > _timeout)
                {
                    _state = State.HalfOpen;
                    _logger.LogInformation("Circuit transitioned to HalfOpen. Testing service availablity.");
                    await SendLogToFrontend("Circuit transitioned to HalfOpen. Testing service availability.");

                }
                else
                {
                    _logger.LogInformation("Circuit is open. Returning default response");
                    await SendLogToFrontend("Circuit is open. Returning default response.");
                    return default(T);
                }
            }
            try
            {
                T result = await action();

                if(_state == State.HalfOpen)
                {
                    await SendLogToFrontend("Circuit returned to Closed after successful request.");
                    _logger.LogInformation("Circuit returned to Closed state after successful response.");
                    Reset();
                }
                else {
                    await SendLogToFrontend("Request executed successfully.");
                    _logger.LogInformation("Request executed successfully.");
                }
                return result;
            }
            catch (Exception ex)
            {
                await SendLogToFrontend("Exception occurred while executing the request.");
                _logger.LogInformation(ex.ToString(),"Exception occured while execting the request.");
                HandleFailure();
                throw;
            }
        }
        private async void HandleFailure()
        {
            _failureCount++;
            await SendLogToFrontend($"Failure count: {_failureCount}. Increasing failure count.");
            _logger.LogInformation($"Failure count: {_failureCount}. Increasing failure count.");

            if(_failureCount >= _threshold)
            {
                _state = State.Open;
                _lastFailureTime = DateTime.UtcNow;
                await SendLogToFrontend("Threshold exceeede. Circuit is now Open.");
                _logger.LogInformation("Threshold exceeede. Circuit is now Open.");
            }
        }

        private async void Reset()
        {
            _failureCount = 0;
            _state = State.Closed;
            await SendLogToFrontend("Circuit is now closed");
            _logger.LogInformation("Circuit is now closed");

        }

        public string GetState()
        {
            return _state.ToString();
        }
        private async Task SendLogToFrontend(string message)
        {
            _logger.LogInformation(message);
            await _hubContext.Clients.All.SendAsync("ReceiveLog", message); // Send log to frontend
        }
    }
}
