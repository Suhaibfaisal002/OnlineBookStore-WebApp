namespace Online_Booking_Store.Models
{
    public class Faq
    {
        public int FaqId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}