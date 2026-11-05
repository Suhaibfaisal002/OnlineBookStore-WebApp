namespace Online_Booking_Store.Models
{
    public class ProductReview
    {
        public int ProductReviewId { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public Product? Product { get; set; }
        public User? User { get; set; }
    }
}