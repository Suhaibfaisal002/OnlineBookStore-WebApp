using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Online_Booking_Store.Data;
using Online_Booking_Store.Models;
using Online_Booking_Store.ViewModels;

namespace Online_Booking_Store.Controllers
{
    public class HomeController : AppController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ShradhaBookStoreContext _context;

        public HomeController(ILogger<HomeController> logger, ShradhaBookStoreContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string? query)
        {
            ViewData["Title"] = "Home";

            var categories = await _context.Categories
                .Where(x => x.IsActive)
                .OrderBy(x => x.CategoryName)
                .ToListAsync();

            var totalProducts = await _context.Products
                .Where(p => p.IsActive)
                .CountAsync();

            var featuredProducts = await _context.Products
                .Include(p => p.Category)
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(8)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.SearchQuery = query ?? "";

            return View();
        }
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Search(string? query) => RedirectToAction("Index", "Product", new { query });

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
