using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;

namespace ProductPriceApp.Services
{
    public class CircuitBreakerService
    {
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public CircuitBreakerService()
        {
            _circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(action);
        }
    }
}
