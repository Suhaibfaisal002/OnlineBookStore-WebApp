using Online_Booking_Store.Models;
namespace Online_Booking_Store.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}