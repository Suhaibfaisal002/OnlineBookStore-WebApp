namespace Online_Booking_Store.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        // 🔥 REQUIRED
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // 🔥 OPTIONAL FK
        public int? SubCategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }

        // 🔥 REQUIRED
        public int ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; } = null!;

        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;

        // 🔥 BOOK FIELDS
        public string? AuthorName { get; set; }
        public string? PublisherName { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string? Version { get; set; }

        public string ProductType { get; set; } = string.Empty;

        public int StockQuantity { get; set; }
        public string? ImagePath { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    }
}