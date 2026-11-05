using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Infrastructure;
using Online_Booking_Store.Models;
using Online_Booking_Store.Services;

namespace Online_Booking_Store.Data;

public static class StartupSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IWebHostEnvironment environment)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ShradhaBookStoreContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

        await context.Database.EnsureCreatedAsync();

        Directory.CreateDirectory(Path.Combine(environment.WebRootPath, "Images", "Products"));
        Directory.CreateDirectory(Path.Combine(environment.WebRootPath, "Images", "Profiles"));
        Directory.CreateDirectory(Path.Combine(environment.WebRootPath, "Images", "Brand"));

        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { CategoryCode = "B1", CategoryName = "Books", IsActive = true },
                new Category { CategoryCode = "M2", CategoryName = "Magazines", IsActive = true },
                new Category { CategoryCode = "C3", CategoryName = "CDs", IsActive = true },
                new Category { CategoryCode = "D4", CategoryName = "DVDs", IsActive = true },
                new Category { CategoryCode = "U5", CategoryName = "Utilities", IsActive = true });
        }

        if (!await context.Faqs.AnyAsync())
        {
            context.Faqs.AddRange(
                new Faq
                {
                    Question = "Will there be any delivery charges apart from product cost?",
                    Answer = "Delivery is completely free if your location is within 3 KMs. For distances more than 3 KMs, charges apply based on the delivery type you choose (Standard or Express).",
                    SortOrder = 1,
                    IsActive = true
                },
                new Faq
                {
                    Question = "How many days will it take to deliver my order?",
                    Answer = "Delivery time depends on order quantity, stock availability, and your chosen payment type. If you choose Payment Before Delivery, your order will only be dispatched after we receive your Demand Draft.",
                    SortOrder = 2,
                    IsActive = true
                },
                new Faq
                {
                    Question = "What if the delivered product is damaged or not in proper condition?",
                    Answer = "If the product delivered is damaged or incorrect, it will be replaced immediately at no extra cost to you.",
                    SortOrder = 3,
                    IsActive = true
                },
                new Faq
                {
                    Question = "Can I cancel my order? Are there any cancellation charges?",
                    Answer = "Yes, you can cancel your order. If cancelled within 24 hours of placing the order, there are no charges. If cancelled after 24 hours and the order has already been dispatched, you will need to bear the delivery charges for both directions.",
                    SortOrder = 4,
                    IsActive = true
                },
                new Faq
                {
                    Question = "How can I make payment for my order?",
                    Answer = "We offer three payment options: (1) Send a Demand Draft before delivery, (2) Pay by Cash or Cheque at time of delivery, (3) Pay online via Credit Card (order dispatches after bank confirms payment).",
                    SortOrder = 5,
                    IsActive = true
                });
        }

        var admin = await context.Users.FirstOrDefaultAsync(x => x.Role == AppRoles.Admin && x.Username == "admin");
        if (admin is null)
        {
            admin = new User
            {
                Username = "admin",
                FullName = "Store Administrator",
                Email = "admin@shradhabookstores.com",
                Phone = "03001234567",
                Address = "Shradha General Book Stores",
                City = "Karachi",
                Role = AppRoles.Admin,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            admin.PasswordHash = passwordService.HashPassword(admin, "Admin@123");
            context.Users.Add(admin);
        }
        else if (admin.PasswordHash == "APP_SEEDED_ON_STARTUP")
        {
            admin.PasswordHash = passwordService.HashPassword(admin, "Admin@123");
        }

        await context.SaveChangesAsync();

        // 🔥 FIRST SAVE CATEGORIES (IMPORTANT)
        await context.SaveChangesAsync();

        // 🔥 THEN ADD SUBCATEGORIES
        if (!await context.SubCategories.AnyAsync())
        {
            var books = await context.Categories.FirstAsync(c => c.CategoryCode == "B1");
            var magazines = await context.Categories.FirstAsync(c => c.CategoryCode == "M2");
            var cds = await context.Categories.FirstAsync(c => c.CategoryCode == "C3");
            var dvds = await context.Categories.FirstAsync(c => c.CategoryCode == "D4");
            var utilities = await context.Categories.FirstAsync(c => c.CategoryCode == "U5");

            context.SubCategories.AddRange(
                // 📚 Books
                new SubCategory { SubCategoryCode = "B102", SubCategoryName = "Comics", CategoryId = books.CategoryId, IsActive = true },
                new SubCategory { SubCategoryCode = "B103", SubCategoryName = "Educational", CategoryId = books.CategoryId, IsActive = true },
                new SubCategory { SubCategoryCode = "B101", SubCategoryName = "Novel", CategoryId = books.CategoryId, IsActive = true },

                // 📰 Magazines
                new SubCategory { SubCategoryCode = "M201", SubCategoryName = "Fashion", CategoryId = magazines.CategoryId, IsActive = true },
                new SubCategory { SubCategoryCode = "M202", SubCategoryName = "Technology", CategoryId = magazines.CategoryId, IsActive = true },

                // 💿 CDs
                new SubCategory { SubCategoryCode = "C301", SubCategoryName = "Readable", CategoryId = cds.CategoryId, IsActive = true },
                new SubCategory { SubCategoryCode = "C302", SubCategoryName = "Rewritable", CategoryId = cds.CategoryId, IsActive = true },

                // 📀 DVDs
                new SubCategory { SubCategoryCode = "D401", SubCategoryName = "Movies", CategoryId = dvds.CategoryId, IsActive = true },
                new SubCategory { SubCategoryCode = "D402", SubCategoryName = "TV Shows", CategoryId = dvds.CategoryId, IsActive = true },

                // 🛠 Utilities
                new SubCategory { SubCategoryCode = "U501", SubCategoryName = "Pencil", CategoryId = utilities.CategoryId, IsActive = true },
                new SubCategory { SubCategoryCode = "U502", SubCategoryName = "Pen", CategoryId = utilities.CategoryId, IsActive = true },
                new SubCategory { SubCategoryCode = "U503", SubCategoryName = "Paper", CategoryId = utilities.CategoryId, IsActive = true },
                new SubCategory { SubCategoryCode = "U504", SubCategoryName = "Sketches", CategoryId = utilities.CategoryId, IsActive = true }
            );

            await context.SaveChangesAsync();
        }
        // 🔥 ADD PRODUCTS PER SUBCATEGORY
        if (!await context.Products.AnyAsync())
        {
            var subCategories = await context.SubCategories.ToListAsync();
            var manufacturer = await context.Manufacturers.FirstOrDefaultAsync();
            if (manufacturer == null)
            {
                manufacturer = new Manufacturer
                {
                    ManufacturerName = "Default Manufacturer",
                    IsActive = true
                };
                context.Manufacturers.Add(manufacturer);
                await context.SaveChangesAsync();
            }

            var products = new List<Product>();

            foreach (var sub in subCategories)
            {
                // Books SubCategories
                if (sub.Category.CategoryCode == "B1")
                {
                    products.Add(new Product
                    {
                        ProductCode = $"BK{sub.SubCategoryCode}",
                        ProductName = $"Sample Book - {sub.SubCategoryName}",
                        CategoryId = sub.CategoryId,
                        SubCategoryId = sub.SubCategoryId,
                        ManufacturerId = manufacturer.ManufacturerId,
                        Price = 499.99m,
                        Description = $"This is a sample {sub.SubCategoryName} book.",
                        AuthorName = "John Doe",
                        PublisherName = "ABC Publishers",
                        ReleaseDate = new DateOnly(2023, 1, 1),
                        Version = "1st Edition",
                        ProductType = "Book",
                        StockQuantity = 100,
                        ImagePath = "/Images/Products/b40b25a5ae90400282bbcde398eb3aca.png",
                        IsActive = true
                    });
                }
                else
                {
                    // Non-book products
                    products.Add(new Product
                    {
                        ProductCode = $"PR{sub.SubCategoryCode}",
                        ProductName = $"Sample Product - {sub.SubCategoryName}",
                        CategoryId = sub.CategoryId,
                        SubCategoryId = sub.SubCategoryId,
                        ManufacturerId = manufacturer.ManufacturerId,
                        Price = 199.99m,
                        Description = $"This is a sample {sub.SubCategoryName} product.",
                        ProductType = "General",
                        StockQuantity = 50,
                        ImagePath = null,
                        IsActive = true
                    });
                }
            }

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}
