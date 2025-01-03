namespace ProductPriceTracker.Models
{
    public class ProductPrice
    {
        public string ProductId { get; set; } // Unique identifier for the product (e.g., "1")
        public string ProductName { get; set; } // Product name (e.g., "Rice Bag 5kg")
        public string Location { get; set; } // Location (e.g., "Bangalore")
        public float Price { get; set; } // Current price of the product (e.g., 500.0)
        public DateTime LastUpdated { get; set; } // Timestamp of the last price update
    }
}
