namespace Online_Booking_Store.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? ProfileImagePath { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    }
}