using Microsoft.AspNetCore.Mvc;
using Online_Booking_Store.Services;
using Online_Booking_Store.ViewModels;

namespace Online_Booking_Store.Controllers;

public class CartController(ICartService cartService) : AppController
{
    public async Task<IActionResult> Index()
    {
        return View(await BuildCartViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int cartId, int quantity)
    {
        await cartService.UpdateQuantityAsync(cartId, quantity, GuestSessionId, CurrentUserId);
        TempData["SuccessMessage"] = "Cart updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int cartId)
    {
        await cartService.RemoveItemAsync(cartId, GuestSessionId, CurrentUserId);
        TempData["SuccessMessage"] = "Item removed from cart.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Checkout()
    {
        if (!CurrentUserId.HasValue || !IsCustomer)
        {
            return RedirectToLogin(Url.Action("PlaceOrder", "Order"), "Please login to place your order");
        }

        return RedirectToAction("PlaceOrder", "Order");
    }

    private async Task<CartViewModel> BuildCartViewModelAsync()
    {
        var items = await cartService.GetCartItemsAsync(GuestSessionId, CurrentUserId);
        return new CartViewModel
        {
            Items = items.Select(x => new CartItemViewModel
            {
                CartId = x.CartId,
                ProductId = x.ProductId,
                ProductName = x.Product?.ProductName ?? "Unknown Product",
                ProductCode = x.Product?.ProductCode,
                ImagePath = x.Product?.ImagePath,
                Quantity = x.Quantity,
                UnitPrice = x.Product?.Price ?? 0,
                SubTotal = (x.Product?.Price ?? 0) * x.Quantity
            }).ToList(),
            TotalAmount = items.Sum(x => (x.Product?.Price ?? 0) * x.Quantity),
            TotalQuantity = items.Sum(x => x.Quantity)
        };
    }
}
