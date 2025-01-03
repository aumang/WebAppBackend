using System;
using System.Threading.Tasks;
using Nest;
using ProductPriceTracker.Models;

namespace ProductPriceTracker.Services
{
    public class ElasticsearchService
    {
        private readonly ElasticClient _client;

        public ElasticsearchService()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("products");
            _client = new ElasticClient(settings);
        }

        public async Task<bool> AddOrUpdateProductAsync(Product product)
        {
            var response = await _client.IndexDocumentAsync(product);
            return response.IsValid;
        }

        public async Task<Product> GetProductByIdAsync(string id)
        {
            var response = await _client.GetAsync<Product>(id);
            return response.Source;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Query(q => q.MatchAll()));
            return response.Documents.ToList();
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
    }
}
