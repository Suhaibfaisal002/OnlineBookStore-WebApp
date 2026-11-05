using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Online_Booking_Store.ViewModels;

public class ProductViewModel
{
    public int ProductId { get; set; }

    public string? ExistingImagePath { get; set; }

    [StringLength(7)]
    [Display(Name = "Product Code")]
    public string? ProductCode { get; set; }

    [Required, StringLength(200)]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Display(Name = "Sub Category")]
    public int? SubCategoryId { get; set; }   

    [Required]
    [Display(Name = "Manufacturer")]
    public int ManufacturerId { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "999999")]
    public decimal Price { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Author Name")]
    public string? AuthorName { get; set; }

    [Display(Name = "Publisher Name")]
    public string? PublisherName { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Release Date")]
    public DateOnly? ReleaseDate { get; set; }

    public string? Version { get; set; }

    [Required]
    [Display(Name = "Product Type")]
    public string ProductType { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }

    [Display(Name = "Image")]
    public IFormFile? ImageFile { get; set; }

    public bool IsActive { get; set; } = true;

    public IEnumerable<SelectListItem> Categories { get; set; } = [];
    public IEnumerable<SelectListItem> SubCategories { get; set; } = [];
    public IEnumerable<SelectListItem> Manufacturers { get; set; } = [];
}
