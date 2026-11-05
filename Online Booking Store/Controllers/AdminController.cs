using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Data;
using Online_Booking_Store.Infrastructure;
using Online_Booking_Store.Models;
using Online_Booking_Store.Services;
using Online_Booking_Store.ViewModels;

namespace Online_Booking_Store.Controllers;

public class AdminController(
    ShradhaBookStoreContext context,
    IOrderService orderService,
    IFileStorageService fileStorageService) : AppController
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        RedirectToAction("Login", "Customer", new { returnUrl = returnUrl ?? Url.Action(nameof(Dashboard), "Admin") });

    public IActionResult Logout()
    {
        SignOutUser();
        TempData["SuccessMessage"] = "Admin session closed successfully.";
        return RedirectToAction("Login", "Customer");
    }

    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> Dashboard()
    {
        var stats = await orderService.GetDashboardStatsAsync();
        return View(new DashboardViewModel
        {
            TotalProducts = stats.TotalProducts,
            TotalOrders = stats.TotalOrders,
            PendingOrders = stats.PendingOrders,
            ProcessingOrders = stats.ProcessingOrders,
            DispatchedOrders = stats.DispatchedOrders,
            DeliveredOrders = stats.DeliveredOrders,
            CancelledOrders = stats.CancelledOrders,
            TotalCustomers = stats.TotalCustomers,
            UnresolvedFeedbacks = stats.UnresolvedFeedbacks,
            TotalRevenue = stats.TotalRevenue,
            AverageOrderValue = stats.TotalOrders == 0 ? 0 : Math.Round(stats.TotalRevenue / stats.TotalOrders, 2),
            RecentOrders = stats.RecentOrders
        });
    }

    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> Profile()
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.UserId == CurrentUserId);
        if (user is null)
        {
            return RedirectToAction("Login", "Customer");
        }

        var model = new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            City = user.City,
            ProfileImagePath = user.ProfileImagePath,
            OrdersCount = await context.Orders.CountAsync(x => x.UserId == user.UserId)
        };

        return View(model);
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.UserId == CurrentUserId);
        if (user is null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!ModelState.IsValid)
        {
            model.ProfileImagePath = user.ProfileImagePath;
            model.OrdersCount = await context.Orders.CountAsync(x => x.UserId == user.UserId);
            return View("Profile", model);
        }

        var normalizedEmail = model.Email.Trim();
        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            await context.Users.AnyAsync(x => x.Email == normalizedEmail && x.UserId != user.UserId))
        {
            ModelState.AddModelError(nameof(ProfileViewModel.Email), "This email is already in use.");
            model.ProfileImagePath = user.ProfileImagePath;
            model.OrdersCount = await context.Orders.CountAsync(x => x.UserId == user.UserId);
            return View("Profile", model);
        }

        user.FullName = model.FullName.Trim();
        user.Email = normalizedEmail;
        user.Phone = model.Phone?.Trim();
        user.Address = model.Address?.Trim();
        user.City = model.City?.Trim();
        user.ProfileImagePath = await fileStorageService.SaveProfileImageAsync(model.ProfileImage, user.ProfileImagePath);

        await context.SaveChangesAsync();
        SignInUser(user);

        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfilePhoto(IFormFile? profileImage, string? returnUrl = null)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.UserId == CurrentUserId);
        if (user is null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (profileImage is null || profileImage.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select an image to upload.";
            return RedirectToAction(nameof(Profile));
        }

        user.ProfileImagePath = await fileStorageService.SaveProfileImageAsync(profileImage, user.ProfileImagePath);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Profile photo updated.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Profile));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBrandLogo(IFormFile? logoFile, string? returnUrl = null)
    {
        if (logoFile is null || logoFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a logo image to upload.";
            return RedirectToAction(nameof(Profile));
        }

        await fileStorageService.SaveBrandLogoAsync(logoFile);
        TempData["SuccessMessage"] = "Store logo updated successfully.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Profile));
    }

    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> Products(string? query, int? categoryId)
    {
        var productQuery = context.Products
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Manufacturer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            productQuery = productQuery.Where(x => x.ProductName.Contains(query) || x.ProductCode.Contains(query));
        }

        if (categoryId.HasValue)
        {
            productQuery = productQuery.Where(x => x.CategoryId == categoryId.Value);
        }

        var model = new AdminProductsPageViewModel
        {
            Products = await productQuery.OrderByDescending(x => x.CreatedDate).ToListAsync(),
            CreateModel = await BuildProductFormAsync(new ProductViewModel())
        };

        ViewBag.Query = query;
        ViewBag.CategoryId = categoryId;
        ViewBag.CategoryFilter = await GetCategorySelectListAsync(categoryId);

        return View(model);
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(ProductViewModel model)
    {
        // 🔥 REMOVE SubCategory validation error manually
        ModelState.Remove(nameof(model.SubCategoryId));

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please fill all required product fields.";
            return RedirectToAction(nameof(Products));
        }

        var category = await context.Categories.FindAsync(model.CategoryId);
        var manufacturer = await context.Manufacturers.FindAsync(model.ManufacturerId);

        if (category is null || manufacturer is null)
        {
            TempData["ErrorMessage"] = "Invalid category or manufacturer selected.";
            return RedirectToAction(nameof(Products));
        }

        // 🔥 HANDLE NULL SUBCATEGORY
        int? subCategoryId = (model.SubCategoryId == null || model.SubCategoryId == 0)
            ? null
            : model.SubCategoryId;

        var serial = await GetNextProductSerialAsync(
            category.CategoryCode,
            manufacturer.ManufacturerName
        );

        var product = new Product
        {
            ProductCode = orderService.GenerateProductCode(
                category.CategoryCode,
                manufacturer.ManufacturerName,
                serial
            ),

            ProductName = model.ProductName.Trim(),
            CategoryId = model.CategoryId,
            SubCategoryId = subCategoryId,

            ManufacturerId = model.ManufacturerId,
            Price = model.Price,
            Description = model.Description?.Trim() ?? "",

            AuthorName = model.AuthorName?.Trim(),
            PublisherName = model.PublisherName?.Trim(),
            ReleaseDate = model.ReleaseDate,

            Version = model.Version,
            ProductType = model.ProductType,

            StockQuantity = model.StockQuantity,
            IsActive = model.IsActive,
            CreatedDate = DateTime.UtcNow,

            ImagePath = await fileStorageService
                .SaveProductImageAsync(model.ImageFile)
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Product added successfully.";
        return RedirectToAction(nameof(Products));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(ProductViewModel model)
    {
        var product = await context.Products.FindAsync(model.ProductId);
        if (product is null)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToAction(nameof(Products));
        }

        product.ProductName = model.ProductName;
        product.CategoryId = model.CategoryId;
        product.SubCategoryId = model.SubCategoryId;
        product.ManufacturerId = model.ManufacturerId;
        product.Price = model.Price;
        product.Description = model.Description;
        product.AuthorName = model.AuthorName;
        product.PublisherName = model.PublisherName;
        product.ReleaseDate = model.ReleaseDate;
        product.Version = model.Version;
        product.ProductType = model.ProductType;
        product.StockQuantity = model.StockQuantity;
        product.IsActive = model.IsActive;
        product.ImagePath = await fileStorageService.SaveProductImageAsync(model.ImageFile, product.ImagePath);

        var category = await context.Categories.FindAsync(model.CategoryId);
        var manufacturer = await context.Manufacturers.FindAsync(model.ManufacturerId);
        if (category is not null && manufacturer is not null)
        {
            var prefix = orderService.GenerateProductCode(category.CategoryCode, manufacturer.ManufacturerName, 0)[..5];
            if (!product.ProductCode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var serial = await GetNextProductSerialAsync(category.CategoryCode, manufacturer.ManufacturerName, product.ProductId);
                product.ProductCode = orderService.GenerateProductCode(category.CategoryCode, manufacturer.ManufacturerName, serial);
            }
        }

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Product updated successfully.";
        return RedirectToAction(nameof(Products));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToAction(nameof(Products));
        }

        fileStorageService.DeleteProductImage(product.ImagePath);
        context.Products.Remove(product);
        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Product deleted successfully.";
        return RedirectToAction(nameof(Products));
    }

    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> Orders(string? status)
    {
        var query = context.Orders
            .Include(x => x.User)
            .Include(x => x.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.OrderStatus == status);
        }

        var model = new AdminOrdersPageViewModel
        {
            Orders = await query.OrderByDescending(x => x.OrderDate).ToListAsync(),
            StatusFilter = status
        };

        return View(model);
    }

    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> OrderDetail(int orderId)
    {
        var order = await context.Orders
            .Include(x => x.User)
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Product)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.OrderId == orderId);

        return order is null ? NotFound() : View(order);
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, string status, string? remarks)
    {
        await orderService.UpdateOrderStatusAsync(orderId, status, remarks);
        TempData["SuccessMessage"] = "Order status updated successfully.";
        return RedirectToAction(nameof(OrderDetail), new { orderId });
    }

    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> Customers()
    {
        var model = new AdminCustomersPageViewModel
        {
            Customers = await context.Users
                .Where(x => x.Role == AppRoles.Customer)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync()
        };

        return View(model);
    }

    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> Feedbacks()
    {
        var model = new AdminFeedbacksPageViewModel
        {
            Feedbacks = await context.FeedbackEntries
                .OrderBy(x => x.IsResolved)
                .ThenByDescending(x => x.SubmittedOn)
                .ToListAsync()
        };

        return View(model);
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReplyFeedback(ReplyFeedbackViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Reply text is required.";
            return RedirectToAction(nameof(Feedbacks));
        }

        var feedback = await context.FeedbackEntries.FindAsync(model.FeedbackId);
        if (feedback is null)
        {
            TempData["ErrorMessage"] = "Feedback record not found.";
            return RedirectToAction(nameof(Feedbacks));
        }

        feedback.AdminReply = model.Reply;
        feedback.RepliedOn = DateTime.UtcNow;
        feedback.IsResolved = true;
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reply saved successfully.";
        return RedirectToAction(nameof(Feedbacks));
    }

    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> FAQs()
    {
        var model = new AdminFaqsPageViewModel
        {
            Faqs = await context.Faqs.OrderBy(x => x.SortOrder).ToListAsync()
        };

        return View(model);
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFAQ(FaqViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please complete the FAQ form.";
            return RedirectToAction(nameof(FAQs));
        }

        context.Faqs.Add(new Faq
        {
            Question = model.Question,
            Answer = model.Answer,
            SortOrder = model.SortOrder,
            IsActive = model.IsActive
        });
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "FAQ added successfully.";
        return RedirectToAction(nameof(FAQs));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFAQ(FaqViewModel model)
    {
        var faq = await context.Faqs.FindAsync(model.FaqId);
        if (faq is null)
        {
            TempData["ErrorMessage"] = "FAQ not found.";
            return RedirectToAction(nameof(FAQs));
        }

        faq.Question = model.Question;
        faq.Answer = model.Answer;
        faq.SortOrder = model.SortOrder;
        faq.IsActive = model.IsActive;
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "FAQ updated successfully.";
        return RedirectToAction(nameof(FAQs));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFAQ(int id)
    {
        var faq = await context.Faqs.FindAsync(id);
        if (faq is not null)
        {
            context.Faqs.Remove(faq);
            await context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "FAQ deleted successfully.";
        return RedirectToAction(nameof(FAQs));
    }

    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> Categories()
    {
        var model = new AdminCategoriesPageViewModel
        {
            Categories = await context.Categories.OrderBy(x => x.CategoryName).ToListAsync(),
            SubCategories = await context.SubCategories.Include(x => x.Category).OrderBy(x => x.SubCategoryName).ToListAsync()
        };

        ViewBag.CategoryOptions = await GetCategorySelectListAsync();
        return View(model);
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Category form is invalid.";
            return RedirectToAction(nameof(Categories));
        }

        var normalizedCode = model.CategoryCode.Trim().ToUpperInvariant();
        if (await context.Categories.AnyAsync(x => x.CategoryCode == normalizedCode))
        {
            TempData["ErrorMessage"] = $"Category code '{normalizedCode}' already exists.";
            return RedirectToAction(nameof(Categories));
        }

        context.Categories.Add(new Category
        {
            CategoryCode = normalizedCode,
            CategoryName = model.CategoryName,
            IsActive = model.IsActive
        });
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Category added successfully.";
        return RedirectToAction(nameof(Categories));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(CategoryFormViewModel model)
    {
        var category = await context.Categories.FindAsync(model.CategoryId);
        if (category is null)
        {
            TempData["ErrorMessage"] = "Category not found.";
            return RedirectToAction(nameof(Categories));
        }

        var normalizedCode = model.CategoryCode.Trim().ToUpperInvariant();
        if (!string.Equals(category.CategoryCode, normalizedCode, StringComparison.OrdinalIgnoreCase) &&
            await context.Categories.AnyAsync(x => x.CategoryCode == normalizedCode && x.CategoryId != category.CategoryId))
        {
            TempData["ErrorMessage"] = $"Category code '{normalizedCode}' already exists.";
            return RedirectToAction(nameof(Categories));
        }

        category.CategoryCode = normalizedCode;
        category.CategoryName = model.CategoryName;
        category.IsActive = model.IsActive;
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Category updated successfully.";
        return RedirectToAction(nameof(Categories));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null)
        {
            TempData["ErrorMessage"] = "Category not found.";
            return RedirectToAction(nameof(Categories));
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Category deleted successfully.";
        return RedirectToAction(nameof(Categories));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSubCategory(SubCategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Subcategory form is invalid.";
            return RedirectToAction(nameof(Categories));
        }

        context.SubCategories.Add(new SubCategory
        {
            SubCategoryCode = model.SubCategoryCode.ToUpperInvariant(),
            SubCategoryName = model.SubCategoryName,
            CategoryId = model.CategoryId,
            IsActive = model.IsActive
        });
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Subcategory added successfully.";
        return RedirectToAction(nameof(Categories));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSubCategory(SubCategoryFormViewModel model)
    {
        var subCategory = await context.SubCategories.FindAsync(model.SubCategoryId);
        if (subCategory is null)
        {
            TempData["ErrorMessage"] = "Subcategory not found.";
            return RedirectToAction(nameof(Categories));
        }

        subCategory.SubCategoryCode = model.SubCategoryCode.ToUpperInvariant();
        subCategory.SubCategoryName = model.SubCategoryName;
        subCategory.CategoryId = model.CategoryId;
        subCategory.IsActive = model.IsActive;
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Subcategory updated successfully.";
        return RedirectToAction(nameof(Categories));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubCategory(int id)
    {
        var subCategory = await context.SubCategories.FindAsync(id);
        if (subCategory is null)
        {
            TempData["ErrorMessage"] = "Subcategory not found.";
            return RedirectToAction(nameof(Categories));
        }

        context.SubCategories.Remove(subCategory);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Subcategory deleted successfully.";
        return RedirectToAction(nameof(Categories));
    }

    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> Manufacturers()
    {
        var model = new AdminManufacturersPageViewModel
        {
            Manufacturers = await context.Manufacturers.OrderBy(x => x.ManufacturerName).ToListAsync()
        };

        return View(model);
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddManufacturer(ManufacturerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Manufacturer form is invalid.";
            return RedirectToAction(nameof(Manufacturers));
        }

        context.Manufacturers.Add(new Manufacturer
        {
            ManufacturerName = model.ManufacturerName,
            ManufacturerCode = BuildManufacturerCode(model.ManufacturerName),
            ContactInfo = model.ContactInfo,
            IsActive = model.IsActive
        });
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Manufacturer added successfully.";
        return RedirectToAction(nameof(Manufacturers));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditManufacturer(ManufacturerFormViewModel model)
    {
        var manufacturer = await context.Manufacturers.FindAsync(model.ManufacturerId);
        if (manufacturer is null)
        {
            TempData["ErrorMessage"] = "Manufacturer not found.";
            return RedirectToAction(nameof(Manufacturers));
        }

        manufacturer.ManufacturerName = model.ManufacturerName;
        manufacturer.ManufacturerCode = BuildManufacturerCode(model.ManufacturerName);
        manufacturer.ContactInfo = model.ContactInfo;
        manufacturer.IsActive = model.IsActive;
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Manufacturer updated successfully.";
        return RedirectToAction(nameof(Manufacturers));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteManufacturer(int id)
    {
        var manufacturer = await context.Manufacturers.FindAsync(id);
        if (manufacturer is null)
        {
            TempData["ErrorMessage"] = "Manufacturer not found.";
            return RedirectToAction(nameof(Manufacturers));
        }

        context.Manufacturers.Remove(manufacturer);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Manufacturer deleted successfully.";
        return RedirectToAction(nameof(Manufacturers));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetSubCategories(int categoryId)
    {
        var data = await context.SubCategories
            .Where(x => x.CategoryId == categoryId && x.IsActive)
            .OrderBy(x => x.SubCategoryName)
            .Select(x => new { x.SubCategoryId, x.SubCategoryName })
            .ToListAsync();

        return Json(data);
    }

    private async Task<ProductViewModel> BuildProductFormAsync(ProductViewModel model)
    {
        model.Categories = await GetCategorySelectListAsync(model.CategoryId == 0 ? null : model.CategoryId);
        model.SubCategories = await context.SubCategories
            .Where(x => model.CategoryId == 0 || x.CategoryId == model.CategoryId)
            .OrderBy(x => x.SubCategoryName)
            .Select(x => new SelectListItem(x.SubCategoryName, x.SubCategoryId.ToString(), x.SubCategoryId == model.SubCategoryId))
            .ToListAsync();
        model.Manufacturers = await context.Manufacturers
            .OrderBy(x => x.ManufacturerName)
            .Select(x => new SelectListItem(x.ManufacturerName, x.ManufacturerId.ToString(), x.ManufacturerId == model.ManufacturerId))
            .ToListAsync();

        return model;
    }

    private async Task<List<SelectListItem>> GetCategorySelectListAsync(int? selectedCategoryId = null)
    {
        return await context.Categories
            .OrderBy(x => x.CategoryName)
            .Select(x => new SelectListItem(x.CategoryName, x.CategoryId.ToString(), x.CategoryId == selectedCategoryId))
            .ToListAsync();
    }

    private async Task<int> GetNextProductSerialAsync(string categoryCode, string manufacturerName, int? currentProductId = null)
    {
        var prefix = orderService.GenerateProductCode(categoryCode, manufacturerName, 0)[..5];
        var codes = await context.Products
            .Where(x => x.ProductCode.StartsWith(prefix) && (!currentProductId.HasValue || x.ProductId != currentProductId.Value))
            .Select(x => x.ProductCode)
            .ToListAsync();

        var maxSerial = codes
            .Select(code => int.TryParse(code[^2..], out var serial) ? serial : 0)
            .DefaultIfEmpty(0)
            .Max();

        return maxSerial + 1;
    }

    private static string BuildManufacturerCode(string manufacturerName)
    {
        var code = new string(
            manufacturerName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => char.ToUpperInvariant(x[0]))
                .Take(3)
                .ToArray());

        return code.PadRight(3, '0');
    }

    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> ContactMessages()
    {
        var messages = await context.ContactMessages
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ContactMessageViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                Phone = m.Phone ?? "Not provided",
                Subject = m.Subject,
                Message = m.Message,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        var viewModel = new AdminContactMessagesPageViewModel
        {
            ContactMessages = messages,
            TotalMessages = messages.Count,
            UnreadMessages = messages.Count // You can add IsRead field if needed
        };

        ViewBag.UnreadMessagesCount = messages.Count;
        return View(viewModel);
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetContactMessageDetails(int id)
    {
        var message = await context.ContactMessages
            .Where(m => m.Id == id)
            .Select(m => new ContactMessageViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                Phone = m.Phone ?? "Not provided",
                Subject = m.Subject,
                Message = m.Message,
                CreatedAt = m.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (message == null)
        {
            return NotFound();
        }

        return PartialView("_ContactMessageDetails", message);
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteContactMessage(int id)
    {
        var message = await context.ContactMessages.FindAsync(id);
        if (message == null)
        {
            TempData["ErrorMessage"] = "Message not found.";
            return RedirectToAction(nameof(ContactMessages));
        }

        context.ContactMessages.Remove(message);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Message deleted successfully.";
        return RedirectToAction(nameof(ContactMessages));
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMultipleContactMessages(int[] messageIds)
    {
        if (messageIds == null || messageIds.Length == 0)
        {
            TempData["ErrorMessage"] = "No messages selected.";
            return RedirectToAction(nameof(ContactMessages));
        }

        var messages = await context.ContactMessages
            .Where(m => messageIds.Contains(m.Id))
            .ToListAsync();

        context.ContactMessages.RemoveRange(messages);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{messages.Count} message(s) deleted successfully.";
        return RedirectToAction(nameof(ContactMessages));
    }
    [SessionAuthorize(AppRoles.Admin)]
    public async Task<IActionResult> SearchCustomers(string? searchTerm)
    {
        var query = context.Users
            .Where(x => x.Role == AppRoles.Customer)
            .AsQueryable();

        // Simple search
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();
            query = query.Where(x =>
                x.FullName.Contains(searchTerm) ||
                x.Email.Contains(searchTerm) ||
                (x.Phone != null && x.Phone.Contains(searchTerm))
            );
        }

        var customers = await query
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();

        var model = new AdminCustomersPageViewModel
        {
            Customers = customers,
            SearchTerm = searchTerm
        };

        return View("Customers", model);
    }

    [SessionAuthorize(AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await context.Users
            .FirstOrDefaultAsync(x => x.UserId == id && x.Role == AppRoles.Customer);

        if (customer == null)
        {
            TempData["ErrorMessage"] = "Customer not found.";
            return RedirectToAction(nameof(Customers));
        }

        context.Users.Remove(customer);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Customer '{customer.FullName}' deleted successfully.";
        return RedirectToAction(nameof(Customers));
    }

}
