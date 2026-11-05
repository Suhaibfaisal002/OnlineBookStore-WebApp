using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Data;
using Online_Booking_Store.Infrastructure;
using Online_Booking_Store.Models;
using Online_Booking_Store.Services;
using Online_Booking_Store.ViewModels;

namespace Online_Booking_Store.Controllers;

public class ProductController(
    ShradhaBookStoreContext context,
    ICartService cartService) : AppController
{
    public async Task<IActionResult> Index(
        string? query,
        List<int>? categoryIds,
        List<int>? subCategoryIds,
        List<int>? manufacturerIds,
        List<string>? productTypes,
        decimal? minPrice,
        decimal? maxPrice,
        bool inStockOnly,
        string? sort,
        int page = 1)
    {
        const int pageSize = 12;
        page = Math.Max(page, 1);
        var productQuery = context.Products
            .Include(x => x.Category)
            .Include(x => x.Manufacturer)
            .Include(x => x.SubCategory)
            .Where(x => x.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            productQuery = productQuery.Where(x =>
                x.ProductName.Contains(query) ||
                x.ProductCode.Contains(query) ||
                x.Description.Contains(query));
        }

        if (categoryIds is { Count: > 0 })
        {
            productQuery = productQuery.Where(x => categoryIds.Contains(x.CategoryId));
        }

        if (subCategoryIds is { Count: > 0 })
        {
            var subCategoryIdsNullable = subCategoryIds.Select(id => (int?)id).ToList();

            productQuery = productQuery
                .Where(x => subCategoryIdsNullable.Contains(x.SubCategoryId));
        }

        if (manufacturerIds is { Count: > 0 })
        {
            productQuery = productQuery.Where(x => manufacturerIds.Contains(x.ManufacturerId));
        }

        var sanitizedTypes = productTypes?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? [];
        if (sanitizedTypes.Count > 0)
        {
            productQuery = productQuery.Where(x => sanitizedTypes.Contains(x.ProductType));
        }

        if (minPrice.HasValue)
        {
            productQuery = productQuery.Where(x => x.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            productQuery = productQuery.Where(x => x.Price <= maxPrice.Value);
        }

        if (inStockOnly)
        {
            productQuery = productQuery.Where(x => x.StockQuantity > 0);
        }

        productQuery = sort switch
        {
            "price_asc" => productQuery.OrderBy(x => x.Price),
            "price_desc" => productQuery.OrderByDescending(x => x.Price),
            "name" => productQuery.OrderBy(x => x.ProductName),
            _ => productQuery.OrderByDescending(x => x.CreatedDate)
        };

        var totalCount = await productQuery.CountAsync();
        var products = await productQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var ratingLookup = new Dictionary<int, ProductRatingSummary>();
        if (products.Count > 0)
        {
            var productIds = products.Select(x => x.ProductId).ToList();
            var ratings = await context.ProductReviews
                .Where(x => productIds.Contains(x.ProductId))
                .GroupBy(x => x.ProductId)
                .Select(x => new
                {
                    ProductId = x.Key,
                    Average = x.Average(r => r.Rating),
                    Count = x.Count()
                })
                .ToListAsync();

            ratingLookup = ratings.ToDictionary(
                x => x.ProductId,
                x => new ProductRatingSummary
                {
                    AverageRating = Math.Round((decimal)x.Average, 1),
                    ReviewCount = x.Count
                });
        }

        var filterParams = new List<KeyValuePair<string, string?>>();
        if (!string.IsNullOrWhiteSpace(query))
        {
            filterParams.Add(new KeyValuePair<string, string>("query", query));
        }

        foreach (var id in categoryIds ?? [])
        {
            filterParams.Add(new KeyValuePair<string, string>("categoryIds", id.ToString(CultureInfo.InvariantCulture)));
        }

        foreach (var id in subCategoryIds ?? [])
        {
            filterParams.Add(new KeyValuePair<string, string>("subCategoryIds", id.ToString(CultureInfo.InvariantCulture)));
        }

        foreach (var id in manufacturerIds ?? [])
        {
            filterParams.Add(new KeyValuePair<string, string>("manufacturerIds", id.ToString(CultureInfo.InvariantCulture)));
        }

        foreach (var type in sanitizedTypes)
        {
            filterParams.Add(new KeyValuePair<string, string>("productTypes", type));
        }

        if (minPrice.HasValue)
        {
            filterParams.Add(new KeyValuePair<string, string>("minPrice", minPrice.Value.ToString(CultureInfo.InvariantCulture)));
        }

        if (maxPrice.HasValue)
        {
            filterParams.Add(new KeyValuePair<string, string>("maxPrice", maxPrice.Value.ToString(CultureInfo.InvariantCulture)));
        }

        if (inStockOnly)
        {
            filterParams.Add(new KeyValuePair<string, string>("inStockOnly", "true"));
        }

        if (!string.IsNullOrWhiteSpace(sort))
        {
            filterParams.Add(new KeyValuePair<string, string>("sort", sort));
        }

        var model = new ProductListViewModel
        {
            SearchQuery = query,
            SelectedCategoryIds = categoryIds ?? [],
            SelectedSubCategoryIds = subCategoryIds ?? [],
            SelectedManufacturerIds = manufacturerIds ?? [],
            SelectedProductTypes = sanitizedTypes,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            InStockOnly = inStockOnly,
            Sort = sort,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Products = products,
            Categories = await context.Categories.Where(x => x.IsActive).OrderBy(x => x.CategoryName).ToListAsync(),
            SubCategories = await context.SubCategories
                .Where(x => x.IsActive && (categoryIds == null || categoryIds.Count == 0 || categoryIds.Contains(x.CategoryId)))
                .OrderBy(x => x.SubCategoryName)
                .ToListAsync(),
            Manufacturers = await context.Manufacturers.Where(x => x.IsActive).OrderBy(x => x.ManufacturerName).ToListAsync(),
            ProductTypes = await context.Products
                .Where(x => x.IsActive)
                .Select(x => x.ProductType)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(),
            FilterQueryString = QueryString.Create(filterParams).ToString(),
            RatingLookup = ratingLookup
        };

        return View(model);
    }

    public IActionResult Category(int categoryId) => RedirectToAction(nameof(Index), new { categoryIds = new[] { categoryId } });

    public async Task<IActionResult> Details(string productCode)
    {
        var product = await context.Products
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Manufacturer)
            .FirstOrDefaultAsync(x => x.ProductCode == productCode && x.IsActive);

        if (product is null)
        {
            return NotFound();
        }

        var model = await BuildProductDetailsViewModelAsync(product);
        return View(model);
    }

    [SessionAuthorize(AppRoles.Customer)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(string productCode, [Bind(Prefix = "ReviewForm")] ProductReviewInputViewModel review)
    {
        var product = await context.Products.FirstOrDefaultAsync(x => x.ProductCode == productCode && x.IsActive);
        if (product is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var model = await BuildProductDetailsViewModelAsync(product, review);
            return View("Details", model);
        }

        var userId = CurrentUserId!.Value;
        var isVerified = await context.OrderItems
            .AnyAsync(x => x.ProductId == product.ProductId && x.Order != null && x.Order.UserId == userId);

        context.ProductReviews.Add(new ProductReview
        {
            ProductId = product.ProductId,
            UserId = userId,
            Rating = review.Rating,
            Title = review.Title.Trim(),
            Comment = review.Comment.Trim(),
            IsVerifiedPurchase = isVerified,
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Thanks for sharing your review!";
        return RedirectToAction(nameof(Details), new { productCode });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? returnUrl = null)
    {
        var product = await context.Products.FirstOrDefaultAsync(x => x.ProductId == productId && x.IsActive);
        if (product is null)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToAction(nameof(Index));
        }

        if (product.StockQuantity <= 0)
        {
            TempData["ErrorMessage"] = "This product is currently out of stock.";
            return RedirectToAction(nameof(Details), new { productCode = product.ProductCode });
        }

        var safeQuantity = Math.Clamp(quantity, 1, product.StockQuantity);
        await cartService.AddToCartAsync(GuestSessionId, CurrentUserId, productId, safeQuantity);

        TempData["SuccessMessage"] = $"{product.ProductName} added to your cart.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Details), new { productCode = product.ProductCode });
    }

    private async Task<ProductDetailsViewModel> BuildProductDetailsViewModelAsync(Product product, ProductReviewInputViewModel? reviewForm = null)
    {
        var reviews = await context.ProductReviews
            .Include(x => x.User)
            .Where(x => x.ProductId == product.ProductId)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync();

        var ratingSummary = new ProductRatingSummary
        {
            ReviewCount = reviews.Count,
            AverageRating = reviews.Count == 0 ? 0 : Math.Round((decimal)reviews.Average(x => x.Rating), 1)
        };

        var ratingBreakdown = Enumerable.Range(1, 5)
            .ToDictionary(x => x, _ => 0);

        foreach (var group in reviews.GroupBy(x => x.Rating))
        {
            ratingBreakdown[group.Key] = group.Count();
        }

        var existingReview = CurrentUserId.HasValue
            ? await context.ProductReviews.AnyAsync(x => x.ProductId == product.ProductId && x.UserId == CurrentUserId.Value)
            : false;

        var formModel = reviewForm ?? new ProductReviewInputViewModel();

        return new ProductDetailsViewModel
        {
            Product = product,
            RatingSummary = ratingSummary,
            RatingBreakdown = ratingBreakdown,
            Reviews = reviews.Select(x => new ProductReviewDisplayViewModel
            {
                ReviewerName = x.User?.FullName ?? "Customer",
                ReviewerImagePath = x.User?.ProfileImagePath,
                Rating = x.Rating,
                Title = x.Title,
                Comment = x.Comment,
                IsVerifiedPurchase = x.IsVerifiedPurchase,
                CreatedOn = x.CreatedOn
            }).ToList(),
            ReviewForm = formModel,
            CanReview = CurrentUserId.HasValue && IsCustomer,
            HasReviewed = existingReview
        };
    }
}
