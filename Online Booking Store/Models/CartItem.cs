namespace Online_Booking_Store.Models
{
    public class CartItem
    {
        public int CartId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Product? Product { get; set; }
    }
}
