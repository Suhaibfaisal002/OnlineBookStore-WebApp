using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Data;
using Online_Booking_Store.Infrastructure;
using Online_Booking_Store.Services;
using Online_Booking_Store.ViewModels;

namespace Online_Booking_Store.Controllers;

public class CustomerController(
    IAuthService authService,
    ICartService cartService,
    ShradhaBookStoreContext context,
    IFileStorageService fileStorageService) : AppController
{
    [HttpGet]
    public IActionResult Register()
    {
        if (CurrentUserId.HasValue)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authService.RegisterCustomerAsync(model.FullName, model.Email, model.Password, model.Phone, model.Address, model.City);
        if (!result.Success || result.User is null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        SignInUser(result.User);
        await cartService.MergeCartAsync(GuestSessionId, result.User.UserId);
        TempData["SuccessMessage"] = "Welcome to Shradha Book Stores.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (CurrentUserId.HasValue)
        {
            return IsAdmin
                ? RedirectToAction("Dashboard", "Admin")
                : RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await authService.AuthenticateAsync(model.EmailOrUsername, model.Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View(model);
        }

        SignInUser(user);

        if (string.Equals(user.Role, AppRoles.Customer, StringComparison.OrdinalIgnoreCase))
        {
            await cartService.MergeCartAsync(GuestSessionId, user.UserId);
        }

        TempData["SuccessMessage"] = $"Welcome back, {user.FullName}.";

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return string.Equals(user.Role, AppRoles.Admin, StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Dashboard", "Admin")
            : RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        SignOutUser();
        TempData["SuccessMessage"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }

    [SessionAuthorize(AppRoles.Customer)]
    public async Task<IActionResult> Profile()
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.UserId == CurrentUserId);
        if (user is null)
        {
            return RedirectToAction("Login");
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

    [SessionAuthorize(AppRoles.Customer)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.UserId == CurrentUserId);
        if (user is null)
        {
            return RedirectToAction("Login");
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

    [SessionAuthorize(AppRoles.Customer)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfilePhoto(IFormFile? profileImage, string? returnUrl = null)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.UserId == CurrentUserId);
        if (user is null)
        {
            return RedirectToAction("Login");
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
}
