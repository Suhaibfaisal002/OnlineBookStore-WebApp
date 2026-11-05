using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Models;
using System.Threading.Tasks;
using Online_Booking_Store.Data;
using Online_Booking_Store.Models;

namespace Online_Booking_Store.Controllers
{
    public class ContactController : Controller
    {
        private readonly ShradhaBookStoreContext _context;

        public ContactController(ShradhaBookStoreContext context)
        {
            _context = context;
        }

        // GET: Contact
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessage contactMessage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    contactMessage.CreatedAt = DateTime.UtcNow;
                    _context.ContactMessages.Add(contactMessage);
                    await _context.SaveChangesAsync();

                    // Set success message
                    TempData["SuccessMessage"] = "Thank you for contacting us! We'll get back to you soon.";

                    // Redirect to GET method to clear the form
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError("", "Unable to save message. Please try again later.");
                }
            }

            return View(contactMessage);
        }
    }
}