using Microsoft.AspNetCore.SignalR;
using Nest;
using ProductPriceTracker.Models;
using System.Net.Http;

namespace ProductPriceTracker.Services
{
    public class GoldPricesService
    {
        private readonly ElasticClient _client;
        public GoldPricesService(ElasticClient client)
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("products");
            _client = new ElasticClient(settings);
        }

        // Modify this method to accept the index name as a parameter
        public async Task<bool> AddOrUpdateGoldPriceAsync(GoldPrice goldPrice, string indexName)
        {
            var response = await _client.IndexAsync(goldPrice, idx => idx.Index(indexName));

            return response.IsValid;
        }

        // Modify this method to accept the index name as a parameter
        public async Task<List<GoldPrice>> GetAllGoldPricesAsync(string indexName)
        {
            var response = await _client.SearchAsync<GoldPrice>(s => s
                .Index(indexName) // Specify the index to use dynamically
                .Query(q => q.MatchAll()));
            return response.Documents.ToList();
        }

        public async Task<GoldPrice> GetLatestGoldPriceAsync(string indexName)
        {
            var response = await _client.SearchAsync<GoldPrice>(s => s
                .Index(indexName) // Specify the index to use dynamically
                .Query(q => q.MatchAll()) // Get all documents
                .Sort(so => so.Descending(g => g.Timestamp)) // Sort by the 'Timestamp' field in descending order
                .Size(1) // Limit the results to 1
            );

            return response.Documents.FirstOrDefault(); // Return the first document, or null if none found
        }

    }
}
