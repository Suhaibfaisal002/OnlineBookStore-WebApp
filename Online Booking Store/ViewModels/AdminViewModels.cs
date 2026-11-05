using System.ComponentModel.DataAnnotations;
using Online_Booking_Store.Models;

namespace Online_Booking_Store.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int DispatchedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int UnresolvedFeedbacks { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
}

public class CategoryFormViewModel
{
    public int CategoryId { get; set; }

    [Required, StringLength(2)]
    public string CategoryCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class SubCategoryFormViewModel
{
    public int SubCategoryId { get; set; }

    [Required, StringLength(5)]
    public string SubCategoryCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string SubCategoryName { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ManufacturerFormViewModel
{
    public int ManufacturerId { get; set; }

    [Required, StringLength(200)]
    public string ManufacturerName { get; set; } = string.Empty;

    [StringLength(300)]
    public string? ContactInfo { get; set; }

    public bool IsActive { get; set; } = true;
}

public class FaqViewModel
{
    public int FaqId { get; set; }

    [Required]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ReplyFeedbackViewModel
{
    public int FeedbackId { get; set; }

    [Required]
    public string Reply { get; set; } = string.Empty;
}

public class AdminProductsPageViewModel
{
    public List<Product> Products { get; set; } = new();
    public ProductViewModel CreateModel { get; set; } = new();
}

public class AdminOrdersPageViewModel
{
    public List<Order> Orders { get; set; } = new();
    public string? StatusFilter { get; set; }
}

public class AdminCategoriesPageViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<SubCategory> SubCategories { get; set; } = new();
    public CategoryFormViewModel CategoryForm { get; set; } = new();
    public SubCategoryFormViewModel SubCategoryForm { get; set; } = new();
}

public class AdminManufacturersPageViewModel
{
    public List<Manufacturer> Manufacturers { get; set; } = new();
    public ManufacturerFormViewModel Form { get; set; } = new();
}

public class AdminFaqsPageViewModel
{
    public List<Faq> Faqs { get; set; } = new();
    public FaqViewModel Form { get; set; } = new();
}

public class AdminFeedbacksPageViewModel
{
    public List<Feedback> Feedbacks { get; set; } = new();
    public ReplyFeedbackViewModel ReplyForm { get; set; } = new();
}



public class ContactMessageViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string FormattedDate => CreatedAt.ToString("dd MMM yyyy, hh:mm tt");
    public string FormattedPhone => !string.IsNullOrEmpty(Phone) ? Phone : "Not provided";
}

public class AdminContactMessagesPageViewModel
{
    public List<ContactMessageViewModel> ContactMessages { get; set; } = new();
    public int TotalMessages { get; set; }
    public int UnreadMessages { get; set; }
}
public class AdminCustomersPageViewModel
{
    public List<User> Customers { get; set; } = new();
    public string? SearchTerm { get; set; }
}