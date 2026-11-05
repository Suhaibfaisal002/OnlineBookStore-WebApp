using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Booking_Store.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFeedbackReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Faqs",
                columns: table => new
                {
                    FaqId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faqs", x => x.FaqId);
                });

            migrationBuilder.CreateTable(
                name: "Manufacturers",
                columns: table => new
                {
                    ManufacturerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManufacturerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ManufacturerCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ContactInfo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturers", x => x.ManufacturerId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProfileImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.CheckConstraint("CK_Users_Role", "[Role] IN ('Admin', 'Customer')");
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    SubCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubCategoryCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    SubCategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.SubCategoryId);
                    table.ForeignKey(
                        name: "FK_SubCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackEntries",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdminReply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepliedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackEntries", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_FeedbackEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalProductAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DeliveryCharge = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Distance = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DeliveryType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OrderStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelCharge = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    ManufacturerId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PublisherName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "ManufacturerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_SubCategories_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "SubCategories",
                        principalColumn: "SubCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionRef = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cart",
                columns: table => new
                {
                    CartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cart", x => x.CartId);
                    table.ForeignKey(
                        name: "FK_Cart_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cart_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                columns: table => new
                {
                    ProductReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVerifiedPurchase = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => x.ProductReviewId);
                    table.CheckConstraint("CK_ProductReviews_Rating", "[Rating] BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_ProductReviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cart_ProductId",
                table: "Cart",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_UserId",
                table: "Cart",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryCode",
                table: "Categories",
                column: "CategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackEntries_UserId",
                table: "FeedbackEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId_UserId",
                table: "ProductReviews",
                columns: new[] { "ProductId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_UserId",
                table: "ProductReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ManufacturerId",
                table: "Products",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode",
                table: "Products",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SubCategoryId",
                table: "Products",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CategoryId",
                table: "SubCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_SubCategoryCode",
                table: "SubCategories",
                column: "SubCategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cart");

            migrationBuilder.DropTable(
                name: "Faqs");

            migrationBuilder.DropTable(
                name: "FeedbackEntries");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ProductReviews");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Manufacturers");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
