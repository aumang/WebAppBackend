namespace ProductPriceTracker.Models
{
    public class PriceHistory
    {
        public string ProductId { get; set; } // Unique identifier for the product
        public string Location { get; set; } // Location where the price was updated
        public float Price { get; set; } // Price of the product at the time of update
        public DateTime UpdatedAt { get; set; } // Timestamp of when the price was updated
    }
}
