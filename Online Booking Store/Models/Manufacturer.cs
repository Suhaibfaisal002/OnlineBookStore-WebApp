namespace Online_Booking_Store.Models
{
    public class Manufacturer
    {
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; } = string.Empty;
        public string ManufacturerCode { get; set; } = string.Empty;
        public string? ContactInfo { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}