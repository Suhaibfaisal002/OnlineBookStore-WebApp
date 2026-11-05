using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Data;

namespace Online_Booking_Store.Controllers;

public class FAQController(ShradhaBookStoreContext context) : AppController
{
    public async Task<IActionResult> Index()
    {
        var faqs = await context.Faqs
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        return View(faqs);
    }
}
