namespace Online_Booking_Store.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? TransactionRef { get; set; }
        public string Status { get; set; } = string.Empty;

        public Order? Order { get; set; }
    }
}