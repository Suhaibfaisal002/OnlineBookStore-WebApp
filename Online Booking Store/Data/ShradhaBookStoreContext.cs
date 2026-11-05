using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Infrastructure;
using Online_Booking_Store.Models;

namespace Online_Booking_Store.Data;

public class ShradhaBookStoreContext(DbContextOptions<ShradhaBookStoreContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<User> Users => Set<User>();
    public DbSet<CartItem> Cart => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Feedback> FeedbackEntries => Set<Feedback>();
    public DbSet<Faq> Faqs => Set<Faq>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryCode)
                .HasMaxLength(2)
                .IsRequired();

            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.CategoryCode)
                .IsUnique();

            entity.HasIndex(e => e.CategoryName);
        });

        modelBuilder.Entity<SubCategory>(entity =>
        {
            entity.HasKey(e => e.SubCategoryId);

            entity.Property(e => e.SubCategoryCode)
                .HasMaxLength(5)
                .IsRequired();

            entity.Property(e => e.SubCategoryName)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.SubCategoryCode)
                .IsUnique();

            entity.HasOne(e => e.Category)
                .WithMany(e => e.SubCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade); 
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(e => e.ManufacturerId);

            entity.Property(e => e.ManufacturerName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.ManufacturerCode)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(e => e.ContactInfo)
                .HasMaxLength(300);

            entity.HasIndex(e => e.ManufacturerCode).IsUnique();
        });
        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.ProductCode)
                .HasMaxLength(7)
                .IsRequired();

            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Price)
                .HasPrecision(10, 2);

            entity.Property(e => e.Description)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            entity.Property(e => e.AuthorName)
                .HasMaxLength(200);

            entity.Property(e => e.PublisherName)
                .HasMaxLength(200);

            entity.Property(e => e.Version)
                .HasMaxLength(50);

            entity.Property(e => e.ProductType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.ImagePath)
                .HasMaxLength(300);

            entity.HasIndex(e => e.ProductCode).IsUnique();

            entity.HasOne(e => e.Category)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SubCategory)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.SubCategoryId)
                .OnDelete(DeleteBehavior.SetNull); 

            entity.HasOne(e => e.Manufacturer)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(400).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.ProfileImagePath).HasMaxLength(300);
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique().HasFilter("[Username] IS NOT NULL");
            entity.ToTable(t => t.HasCheckConstraint("CK_Users_Role", $"[Role] IN ('{AppRoles.Admin}', '{AppRoles.Customer}')"));
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartId);
            entity.Property(e => e.SessionId).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.Product)
                .WithMany(e => e.CartItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany(e => e.CartItems)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderNumber).HasMaxLength(8).IsRequired();
            entity.Property(e => e.TotalProductAmount).HasPrecision(10, 2);
            entity.Property(e => e.DeliveryCharge).HasPrecision(10, 2);
            entity.Property(e => e.GrandTotal).HasPrecision(10, 2);
            entity.Property(e => e.Distance).HasPrecision(5, 2);
            entity.Property(e => e.DeliveryType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OrderStatus).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PaymentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PaymentStatus).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CancelCharge).HasPrecision(10, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId);
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
            entity.Property(e => e.SubTotal).HasPrecision(10, 2);
            entity.HasOne(e => e.Order)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.PaymentMode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TransactionRef).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.Order)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId);
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Message).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.AdminReply).HasColumnType("nvarchar(max)");
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.ProductReviewId);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Comment).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.Rating).IsRequired();
            entity.ToTable(t => t.HasCheckConstraint("CK_ProductReviews_Rating", "[Rating] BETWEEN 1 AND 5"));
            entity.HasIndex(e => e.ProductId);
            entity.HasOne(e => e.Product)
                .WithMany(e => e.ProductReviews)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany(e => e.ProductReviews)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.FaqId);
            entity.Property(e => e.Question).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.Answer).HasColumnType("nvarchar(max)").IsRequired();
        });
    }
}
