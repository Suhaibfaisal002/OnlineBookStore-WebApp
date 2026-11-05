namespace Online_Booking_Store.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public int? UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
        public string? AdminReply { get; set; }
        public DateTime? RepliedOn { get; set; }
        public bool IsResolved { get; set; }

        public User? User { get; set; }
    }
}