namespace Online_Booking_Store.Models
{
    public class SubCategory
    {
        public int SubCategoryId { get; set; }

        public string SubCategoryCode { get; set; } = string.Empty;
        public string SubCategoryName { get; set; } = string.Empty;

        // 🔥 REQUIRED
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}