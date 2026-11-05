using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Data;
using Online_Booking_Store.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "ShradhaBookStores.Session";
    options.IdleTimeout = TimeSpan.FromHours(4);
});

builder.Services.AddDbContext<ShradhaBookStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

await StartupSeeder.SeedAsync(app.Services, app.Environment);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
