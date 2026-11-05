namespace Online_Booking_Store.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalProductAmount { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Distance { get; set; }
        public string DeliveryType { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime? CancelledDate { get; set; }
        public decimal CancelCharge { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}