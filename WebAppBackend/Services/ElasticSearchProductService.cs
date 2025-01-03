using Nest;
using ProductPriceTracker.Models;
namespace ProductPriceTracker.Services
{
    public class ElasticSearchProductService
    {
        private readonly ElasticClient _client;
        public ElasticSearchProductService()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("product_prices");
            _client = new ElasticClient(settings);
        }

        public async Task<bool> AddOrUpdateProductAsync(ProductPrice productPrice)
        {
            //var response = await _client.IndexDocumentAsync(productPrice);
            // Use a consistent ID, for example, based on ProductId or a unique field
            var documentId = productPrice.ProductId.ToString(); // Replace with your unique identifier field

            // Index the document with the specific ID
            var response = await _client.IndexAsync(productPrice, i => i.Id(documentId));
            return response.IsValid;
        }

        public async Task<List<ProductPrice>> GetProductByLocationAsync(string location)
        {
            var response = await _client.SearchAsync<ProductPrice>(s => s
                .Query(q=>q
                    .Bool(b=>b
                        .Must(
                            //m => m.Term(t => t.Field(f=>f.ProductId).Value(productId)),
                            m => m.Term(t => t.Field(f=>f.Location).Value(location))
                            
                            )
                        )
                    )
                );
            return response.Documents.ToList<ProductPrice>();
        }

        public async Task SavePriceHistoryAsync(PriceHistory priceHistory)
        {
            await _client.IndexAsync(priceHistory, i => i.Index("price_history"));
        }

        public async Task<IEnumerable<PriceHistory>> GetPriceHistoryAsync(string location)
        {
            var response = await _client.SearchAsync<PriceHistory>(s => s
            .Query(q => q
                .Bool(b => b
                    .Must(
                        //m => m.Term(t => t.Field(f => f.ProductId).Value(productId)),
                        m => m.Term(t => t.Field(f => f.Location).Value(location)))
                    )
                )
            );
            return response.Documents;
        }
    }
}
