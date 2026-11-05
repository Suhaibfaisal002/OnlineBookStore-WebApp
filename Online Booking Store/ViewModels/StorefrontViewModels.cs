using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Online_Booking_Store.Models;

namespace Online_Booking_Store.ViewModels;

public class HomeViewModel
{
    public List<Product> FeaturedProducts { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public string? SearchQuery { get; set; }
}

public class ProductRatingSummary
{
    public int ReviewCount { get; set; }
    public decimal AverageRating { get; set; }
}

public class ProductListViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<SubCategory> SubCategories { get; set; } = new();
    public List<Manufacturer> Manufacturers { get; set; } = new();
    public List<string> ProductTypes { get; set; } = new();
    public List<int> SelectedCategoryIds { get; set; } = new();
    public List<int> SelectedSubCategoryIds { get; set; } = new();
    public List<int> SelectedManufacturerIds { get; set; } = new();
    public List<string> SelectedProductTypes { get; set; } = new();
    public string? SearchQuery { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool InStockOnly { get; set; }
    public string? Sort { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string FilterQueryString { get; set; } = string.Empty;
    public Dictionary<int, ProductRatingSummary> RatingLookup { get; set; } = new();
}

public class ProductReviewDisplayViewModel
{
    public string ReviewerName { get; set; } = string.Empty;
    public string? ReviewerImagePath { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class ProductReviewInputViewModel
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Comment { get; set; } = string.Empty;
}

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = new();
    public int Quantity { get; set; } = 1;
    public ProductRatingSummary RatingSummary { get; set; } = new();
    public Dictionary<int, int> RatingBreakdown { get; set; } = new();
    public List<ProductReviewDisplayViewModel> Reviews { get; set; } = new();
    public ProductReviewInputViewModel ReviewForm { get; set; } = new();
    public bool CanReview { get; set; }
    public bool HasReviewed { get; set; }
}

public class CartItemViewModel
{
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? ImagePath { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal { get; set; }
}

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalQuantity { get; set; }
}

public class OrderSummaryItemViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}

public class OrderViewModel
{
    [Required, StringLength(500)]
    [Display(Name = "Delivery Address")]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0", "99999")]
    [Display(Name = "Distance (KM)")]
    public decimal Distance { get; set; }

    [Required]
    [Display(Name = "Delivery Type")]
    public string DeliveryType { get; set; } = "Standard";

    [Required]
    [Display(Name = "Payment Type")]
    public string PaymentType { get; set; } = "AfterDelivery";

    [Display(Name = "Payment Mode")]
    public string PaymentMode { get; set; } = "Cash";

    [StringLength(500)]
    public string? Remarks { get; set; }

    public List<OrderSummaryItemViewModel> Items { get; set; } = new();
    public decimal TotalProductAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DeliveryCharge { get; set; }
    public decimal GrandTotal { get; set; }
    public int TotalQuantity { get; set; }
    public IEnumerable<SelectListItem> DeliveryTypes { get; set; } = [];
}

public class TrackOrderViewModel
{
    [Required, StringLength(8)]
    [Display(Name = "Order Number")]
    public string OrderNumber { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
}

public class UpdateOrderViewModel
{
    [Required]
    public string OrderNumber { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0", "99999")]
    public decimal Distance { get; set; }

    [Required]
    public string DeliveryType { get; set; } = "Standard";

    [Required]
    public string PaymentType { get; set; } = "AfterDelivery";

    public string PaymentMode { get; set; } = "Cash";
    public string? Remarks { get; set; }
}

public class FeedbackViewModel
{
    [Required, StringLength(200)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;
}

public class FeedbackEntryViewModel
{
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedOn { get; set; }
    public bool IsResolved { get; set; }
    public string? AdminReply { get; set; }
    public DateTime? RepliedOn { get; set; }
}

public class FeedbackPageViewModel
{
    public FeedbackViewModel Form { get; set; } = new();
    public List<FeedbackEntryViewModel> Entries { get; set; } = new();
    public bool IsSignedIn { get; set; }
}
