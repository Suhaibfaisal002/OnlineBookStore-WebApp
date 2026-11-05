using Microsoft.AspNetCore.Mvc;
using Online_Booking_Store.Infrastructure;
using Online_Booking_Store.Models;

namespace Online_Booking_Store.Controllers;

public abstract class AppController : Controller
{
    protected int? CurrentUserId => HttpContext.Session.GetInt32(SessionKeys.UserId);
    protected string? CurrentUserName => HttpContext.Session.GetString(SessionKeys.UserName);
    protected string? CurrentUserRole => HttpContext.Session.GetString(SessionKeys.UserRole);
    protected string GuestSessionId => HttpContext.Session.GetOrCreateGuestSessionId();

    protected bool IsCustomer => string.Equals(CurrentUserRole, AppRoles.Customer, StringComparison.OrdinalIgnoreCase);
    protected bool IsAdmin => string.Equals(CurrentUserRole, AppRoles.Admin, StringComparison.OrdinalIgnoreCase);

    protected void SignInUser(User user)
    {
        HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
        HttpContext.Session.SetString(SessionKeys.UserName, user.FullName);
        HttpContext.Session.SetString(SessionKeys.UserRole, user.Role);
    }

    protected void SignOutUser()
    {
        HttpContext.Session.Remove(SessionKeys.UserId);
        HttpContext.Session.Remove(SessionKeys.UserName);
        HttpContext.Session.Remove(SessionKeys.UserRole);
    }

    protected IActionResult RedirectToLogin(string? returnUrl = null, string? message = null)
    {
        TempData["WarningMessage"] = message ?? "Please login to continue.";
        return RedirectToAction("Login", "Customer", new
        {
            returnUrl = returnUrl ?? $"{HttpContext.Request.Path}{HttpContext.Request.QueryString}"
        });
    }
}
